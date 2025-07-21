using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using UmbracoConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Commerce.Portal.Pipeline.Tasks;

public class CreateUmbracoCommercePortalDocumentTypesTask(
    ILogger<CreateUmbracoCommercePortalDocumentTypesTask> logger,
    IDataTypeService dataTypeService,
    IContentTypeContainerService contentTypeContainerService,
    IContentTypeService contentTypeService,
    IShortStringHelper shortStringHelper)
    : PipelineTaskBase<InstallPipelineContext>
{
    private const string ContentTypePageName = "[Umbraco Commerce Portal] {0} Page";

    public override async Task<PipelineResult> ExecuteAsync(PipelineArgs args, CancellationToken cancellationToken)
    {
        logger.LogInformation("Begin CreateUmbracoCommercePortalDocumentTypesTask");

        // Setup lazy data types
        var booleanDataType = new Lazy<Task<IDataType?>>(() => dataTypeService.GetAsync(UmbracoConstants.DataTypes.Guids.CheckboxGuid));
        var contentPickerDataType = new Lazy<Task<IDataType?>>(() => dataTypeService.GetAsync(UmbracoConstants.DataTypes.Guids.ContentPickerGuid));

        // Portal container content type folder
        logger.LogInformation("Create or update portal container document type folder");
        EntityContainer? portalContentTypeFolder = await contentTypeContainerService
            .GetAsync(UmbracoCommercePortalConstants.ContentTypes.Guids.BasePageGuid);
        if (portalContentTypeFolder == null)
        {
            logger.LogInformation("Portal container document type folder is not found, creating a new folder");
            Attempt<EntityContainer?, EntityContainerOperationStatus> folderCreateAttempt = await contentTypeContainerService.CreateAsync(
                UmbracoCommercePortalConstants.ContentTypes.Guids.BasePageGuid,
                "[Umbraco Commerce Portal] Pages",
                null,
                UmbracoConstants.Security.SuperUserKey);
            if (!folderCreateAttempt.Success)
            {
                throw new InvalidOperationException("Unable to create a folder to store portal package content types");
            }

            portalContentTypeFolder = folderCreateAttempt.Result;
        }

        // Portal Auth Page
        Attempt<IContentType?> portalAuthPageAttempt = await AddOrUpdatePageContentTypeAsync(
            UmbracoCommercePortalConstants.ContentTypes.Guids.PortalAuthPageGuid,
            null,
            UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage,
            string.Format(ContentTypePageName, "Auth"),
            "icon-user color-blue",
            null,
            portalContentTypeFolder.Key);
        if (!portalAuthPageAttempt.Success)
        {
            logger.LogError(
                portalAuthPageAttempt.Exception,
                "Create or update portal auth page attempt status {AttemptStatus}.",
                portalAuthPageAttempt.Result);
            return Fail(portalAuthPageAttempt.Exception);
        }

        // Portal Container Page
        PropertyType[] portalPageProps = [
             CreatePropertyType(await contentPickerDataType.Value, x =>
            {
                x.Alias = UmbracoCommercePortalConstants.ContentTypes.Properties.HomePageAlias;
                x.Name = "Portal Home Page";
                x.Description = "The portal's home page.";
                x.SortOrder = 40;
            }),
            CreatePropertyType(await contentPickerDataType.Value, x =>
            {
                x.Alias = UmbracoCommercePortalConstants.ContentTypes.Properties.TermsAndConditionsPageAlias;
                x.Name = "Terms and Conditions Page";
                x.Description = "The page on the site containing the terms and conditions.";
                x.SortOrder = 50;
            }),
            CreatePropertyType(await contentPickerDataType.Value, x =>
            {
                x.Alias = UmbracoCommercePortalConstants.ContentTypes.Properties.PrivacyPolicyPageAlias;
                x.Name = "Privacy Policy Page";
                x.Description = "The page on the site containing the privacy policy.";
                x.SortOrder = 60;
            }),
            CreatePropertyType(await booleanDataType.Value, x =>
            {
                x.Alias = "ucpCollectBillingInfo";
                x.Name = "Collect Billing Info";
                x.Description = "Select whether to collect billing information or not. Not necessary if you are only dealing with digital downloads.";
                x.SortOrder = 70;
            }),
            CreatePropertyType(await booleanDataType.Value, x =>
            {
                x.Alias = "ucpCollectShippingInfo";
                x.Name = "Collect Shipping Info";
                x.Description = "Select whether to collect shipping information or not. Not necessary if you are only dealing with digital downloads.";
                x.SortOrder = 80;
            })
        ];

        Attempt<IContentType?> portalContainerPageAttempt = await AddOrUpdatePageContentTypeAsync(
            UmbracoCommercePortalConstants.ContentTypes.Guids.PortalContainerPageGuid,
            portalPageProps,
            UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage,
            string.Format(ContentTypePageName, "Portal"),
            "icon-users color-blue",
            new[] { new ContentTypeSort(portalAuthPageAttempt.Result!.Key, 1, portalAuthPageAttempt.Result!.Alias) },
            portalContentTypeFolder.Key);
        if (!portalContainerPageAttempt.Success)
        {
            logger.LogError(
                portalContainerPageAttempt.Exception,
                "Create or update portal container page attempt status {AttemptStatus}.",
                portalContainerPageAttempt.Result);
            return Fail(portalContainerPageAttempt.Exception);
        }

        // Portal Management Page

        // Portal My Account Page

        // Portal Order History Page

        return Ok();
    }

    private PropertyType CreatePropertyType(IDataType? dataType, Action<PropertyType> config)
    {
        ArgumentNullException.ThrowIfNull(dataType);

        PropertyType propertyType = new(shortStringHelper, dataType);

        config.Invoke(propertyType);

        return propertyType;
    }

    private async Task<Attempt<IContentType?>> AddOrUpdatePageContentTypeAsync(
        Guid pageGuid,
        PropertyType[]? pageProperties,
        string alias,
        string name,
        string icon,
        IEnumerable<ContentTypeSort>? allowedContentTypes,
        Guid? containerKey)
    {
        logger.LogInformation("Create or update page document type {Name}", name);
        IContentType? pageContentType = await contentTypeService
            .GetAsync(UmbracoCommercePortalConstants.ContentTypes.Guids.PortalContainerPageGuid);
        if (pageContentType == null)
        {
            pageContentType = new ContentType(shortStringHelper, -1)
            {
                Key = pageGuid,
                Alias = alias,
                Name = name,
                Icon = icon
            };

            if (allowedContentTypes is not null)
            {
                pageContentType.AllowedContentTypes = allowedContentTypes;
            }

            if (pageProperties is not null)
            {
                pageContentType.PropertyGroups = new PropertyGroupCollection(
                    [
                        new PropertyGroup(new PropertyTypeCollection(true, pageProperties))
                        {
                            Alias = "settings",
                            Name = "Settings",
                            Type = PropertyGroupType.Group,
                            SortOrder = 50,
                        },
                    ]);
            }

            Attempt<ContentTypeOperationStatus> createAttempt = await contentTypeService
                .CreateAsync(pageContentType, UmbracoConstants.Security.SuperUserKey);
            if (!createAttempt.Success)
            {
                return Attempt.Fail(pageContentType, createAttempt.Exception!);
            }
        }
        else
        {
            bool safeExisting = false;
            bool hasSettingsGroup = pageContentType.PropertyGroups.Contains("Settings");
            PropertyGroup settingsGroup = hasSettingsGroup
                ? pageContentType.PropertyGroups["Settings"]
                : new PropertyGroup(new PropertyTypeCollection(true, pageProperties))
                {
                    Alias = "settings",
                    Name = "Settings",
                    Type = PropertyGroupType.Group,
                    SortOrder = 100,
                };

            foreach (PropertyType prop in pageProperties)
            {
                if (settingsGroup.PropertyTypes != null && !settingsGroup.PropertyTypes.Contains(prop.Alias))
                {
                    settingsGroup.PropertyTypes.Add(prop);
                    safeExisting = true;
                }
            }

            if (!hasSettingsGroup)
            {
                pageContentType.PropertyGroups.Add(settingsGroup);
                safeExisting = true;
            }

            if (safeExisting)
            {
                Attempt<ContentTypeOperationStatus> updateAttempt = await contentTypeService
                    .UpdateAsync(pageContentType, UmbracoConstants.Security.SuperUserKey);
                if (!updateAttempt.Success)
                {
                    return Attempt.Fail(pageContentType, updateAttempt.Exception!);
                }
            }
        }

        // Move to folder
        if (containerKey is not null)
        {
            await contentTypeService.MoveAsync(pageContentType.Key, containerKey);
        }

        return Attempt.Succeed(pageContentType);
    }
}
