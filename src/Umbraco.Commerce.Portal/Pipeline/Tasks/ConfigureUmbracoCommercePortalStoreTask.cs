using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Extensions;

namespace Umbraco.Commerce.Portal.Pipeline.Tasks;

internal class ConfigureUmbracoCommercePortalStoreTask(IUmbracoCommerceApi commerceApi)
    : PipelineTaskBase<InstallPipelineContext>
{
    public override async Task<PipelineResult<InstallPipelineContext>> ExecuteAsync(PipelineArgs<InstallPipelineContext> args, CancellationToken cancellationToken)
    {
        await commerceApi.Uow.ExecuteAsync(
            async uow =>
            {
                // Register Member Email Template
                bool confirmMemberEmailTemplateExists = await commerceApi.EmailTemplateExistsAsync(
                    args.Model.Store.Id,
                    UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Key);

                if (!confirmMemberEmailTemplateExists)
                {
                    await EmailTemplate.CreateAsync(
                        uow,
                        args.Model.Store.Id,
                        UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Key,
                        UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Value);
                }

                var confirmMemberEmailTemplate = await commerceApi.GetEmailTemplateAsync(
                        args.Model.Store.Id,
                        UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Key)
                    .AsWritableAsync(uow)
                    .SetSubjectAsync("Confirm Member")
                    .SetSenderNameAsync("Umbraco Commerce Portal")
                    .SetSenderAddressAsync("portal@localhost")
                    .SetTemplateViewAsync(UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Value);

                await commerceApi.SaveEmailTemplateAsync(confirmMemberEmailTemplate, cancellationToken);

                // Reset Password Email Template
                bool resetPasswordEmailTemplateExists = await commerceApi.EmailTemplateExistsAsync(
                    args.Model.Store.Id,
                    UmbracoCommercePortalConstants.EmailTemplates.ResetPasswordEmailTemplate.Key);

                if (!resetPasswordEmailTemplateExists)
                {
                    await EmailTemplate.CreateAsync(
                        uow,
                        args.Model.Store.Id,
                        UmbracoCommercePortalConstants.EmailTemplates.ResetPasswordEmailTemplate.Key,
                        UmbracoCommercePortalConstants.EmailTemplates.ResetPasswordEmailTemplate.Value);
                }

                var resetPasswordEmailTemplate = await commerceApi.GetEmailTemplateAsync(
                        args.Model.Store.Id,
                        UmbracoCommercePortalConstants.EmailTemplates.ResetPasswordEmailTemplate.Key)
                    .AsWritableAsync(uow)
                    .SetSubjectAsync("Reset Password")
                    .SetSenderNameAsync("Umbraco Commerce Portal")
                    .SetSenderAddressAsync("portal@localhost")
                    .SetTemplateViewAsync(UmbracoCommercePortalConstants.EmailTemplates.ResetPasswordEmailTemplate.Value);

                await commerceApi.SaveEmailTemplateAsync(resetPasswordEmailTemplate, cancellationToken);

                uow.Complete();
            }, cancellationToken);

        return Ok();
    }
}
