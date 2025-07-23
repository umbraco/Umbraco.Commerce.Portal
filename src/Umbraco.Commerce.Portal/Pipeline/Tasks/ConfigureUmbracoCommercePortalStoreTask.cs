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

                EmailTemplate? emailTemplate = null;
                if (!confirmMemberEmailTemplateExists)
                {
                    emailTemplate = await EmailTemplate.CreateAsync(
                        uow,
                        args.Model.Store.Id,
                        UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Key,
                        UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Value);
                }

                if (emailTemplate == null)
                {
                    emailTemplate = await commerceApi.GetEmailTemplateAsync(
                                        args.Model.Store.Id,
                                        UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Key)
                                    .AsWritableAsync(uow);
                }

                await emailTemplate
                    .SetSubjectAsync("Confirm Member")
                    .SetSenderNameAsync("Umbraco Commerce Portal")
                    .SetSenderAddressAsync("portal@localhost")
                    .SetTemplateViewAsync(UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Value);

                await commerceApi.SaveEmailTemplateAsync(emailTemplate, cancellationToken);

                // Reset Password Email Template
                bool resetPasswordEmailTemplateExists = await commerceApi.EmailTemplateExistsAsync(
                    args.Model.Store.Id,
                    UmbracoCommercePortalConstants.EmailTemplates.ResetPasswordEmailTemplate.Key);

                EmailTemplate? resetPasswordEmailTemplate = null;
                if (!resetPasswordEmailTemplateExists)
                {
                    resetPasswordEmailTemplate = await EmailTemplate.CreateAsync(
                        uow,
                        args.Model.Store.Id,
                        UmbracoCommercePortalConstants.EmailTemplates.ResetPasswordEmailTemplate.Key,
                        UmbracoCommercePortalConstants.EmailTemplates.ResetPasswordEmailTemplate.Value);
                }

                if (resetPasswordEmailTemplate == null)
                {
                    resetPasswordEmailTemplate = await commerceApi.GetEmailTemplateAsync(
                        args.Model.Store.Id,
                        UmbracoCommercePortalConstants.EmailTemplates.ResetPasswordEmailTemplate.Key)
                        .AsWritableAsync(uow);
                }

                await resetPasswordEmailTemplate
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
