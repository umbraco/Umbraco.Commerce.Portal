using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Api.Management.ViewModels.RelationType.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using Umbraco.Commerce.Core.Api;
using UmbracoConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Commerce.Portal.Pipeline.Tasks;

internal class CreateUmbracoCommercePortalMemberTypeTask(
    IDataTypeService dataTypeService,
    IMemberGroupService memberGroupService,
    IMemberTypeService memberTypeService,
    IShortStringHelper shortStringHelper,
    PropertyEditorCollection propertyEditorCollection,
    IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
    IUmbracoCommerceApi commerceApi)
    : PipelineTaskBase<InstallPipelineContext>
{
    public override async Task<PipelineResult<InstallPipelineContext>> ExecuteAsync(PipelineArgs<InstallPipelineContext> args, CancellationToken cancellationToken)
    {
        var createMemberGroupAttempt = await CreateMemberGroupAsync();
        if (!createMemberGroupAttempt.Success)
        {
            return Fail(createMemberGroupAttempt.Exception);
        }

        if (memberTypeService.Get(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberType) is not null)
        {
            return Ok();
        }

        var createMemberTypeAttempt = await CreateMemberTypeAsync(args.Model.Store.Id);
        if (!createMemberTypeAttempt.Success)
        {
            return Fail(createMemberTypeAttempt.Exception);
        }

        return Ok();
    }

    private async Task<Attempt<IMemberGroup?, MemberGroupOperationStatus>> CreateMemberGroupAsync()
    {
        var memberGroup = await memberGroupService.GetByNameAsync(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberGroup);
        if (memberGroup is not null)
        {
            return Attempt.SucceedWithStatus(MemberGroupOperationStatus.Success, memberGroup);
        }

        memberGroup = new MemberGroup
        {
            Name = UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberGroup,
        };
        return await memberGroupService.CreateAsync(memberGroup);
    }

    private async Task<Attempt<ContentTypeOperationStatus>> CreateMemberTypeAsync(Guid storeId)
    {
        var textStringDataType = new Lazy<Task<IDataType?>>(() => dataTypeService.GetAsync(UmbracoConstants.DataTypes.Guids.TextstringGuid));
        var countryPickerDataType = await dataTypeService.GetAsync(UmbracoCommercePortalConstants.DataTypes.CountryPickerDataTypeName);
        if (countryPickerDataType is null)
        {
            countryPickerDataType = await CreateCountryPickerDataTypeAsync(storeId);
        }

        PropertyType[] memberTypeProps = [
            CreatePropertyType(await textStringDataType.Value, x =>
            {
                x.Alias = UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.FirstName;
                x.Name = "First Name";
                x.Description = "Member first name";
                x.SortOrder = 40;
            }),
            CreatePropertyType(await textStringDataType.Value, x =>
            {
                x.Alias = UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.LastName;
                x.Name = "Last Name";
                x.Description = "Member last name";
                x.SortOrder = 50;
            }),
            CreatePropertyType(countryPickerDataType, x =>
            {
                x.Alias = UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Country;
                x.Name = "Country";
                x.Description = "Member country";
                x.SortOrder = 60;
            }),
            CreatePropertyType(await textStringDataType.Value, x =>
            {
                x.Alias = UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Street;
                x.Name = "Street";
                x.Description = "Member street";
                x.SortOrder = 70;
            }),
            CreatePropertyType(await textStringDataType.Value, x =>
            {
                x.Alias = UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.City;
                x.Name = "City";
                x.Description = "Member city";
                x.SortOrder = 80;
            }),
            CreatePropertyType(await textStringDataType.Value, x =>
            {
                x.Alias = UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.State;
                x.Name = "State";
                x.Description = "Member state";
                x.SortOrder = 90;
            }),
            CreatePropertyType(await textStringDataType.Value, x =>
            {
                x.Alias = UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.ZipCode;
                x.Name = "Zip Code";
                x.Description = "Member zip code";
                x.SortOrder = 100;
            })
        ];

        var memberType = new MemberType(shortStringHelper, -1)
        {
            Alias = UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberType,
            Name = "Umbraco Commerce Portal Member",
            Icon = "icon-user"
        };

        memberType.PropertyGroups = new PropertyGroupCollection([
            new PropertyGroup(new PropertyTypeCollection(true, memberTypeProps))
            {
                Alias = "details",
                Name = "Details",
                Type = PropertyGroupType.Group,
                SortOrder = 0
            }
        ]);

        return await memberTypeService.CreateAsync(memberType, UmbracoConstants.Security.SuperUserKey);
    }

    private async Task<IDataType?> CreateCountryPickerDataTypeAsync(Guid storeId)
    {
        var countryPickerDataType = new DataType(propertyEditorCollection[Cms.Constants.PropertyEditors.Aliases.StoreEntityPicker], configurationEditorJsonSerializer)
        {
            EditorUiAlias = "Uc.PropertyEditorUi.StoreEntityPicker",
            ConfigurationData = new Dictionary<string, object>
            {
                { "entityType",  "Country" },
                { "storeId", storeId.ToString() }
            },
            Name = UmbracoCommercePortalConstants.DataTypes.CountryPickerDataTypeName,
            CreateDate = DateTime.UtcNow,
        };
        var attempt = await dataTypeService.CreateAsync(countryPickerDataType, Constants.Security.SuperUserKey);

        return attempt.Success ? attempt.Result : null;
    }

    private PropertyType CreatePropertyType(IDataType? dataType, Action<PropertyType> config)
    {
        ArgumentNullException.ThrowIfNull(dataType);

        PropertyType propertyType = new(shortStringHelper, dataType);

        config.Invoke(propertyType);

        return propertyType;
    }
}
