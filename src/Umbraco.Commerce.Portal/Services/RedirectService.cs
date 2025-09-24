using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Portal.Services;

public class RedirectService
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IContentService _contentService;
    private readonly IUmbracoContextFactory _umbracoContext;
    private readonly IScopeProvider _scopeProvider;

    public RedirectService(
        IContentTypeService contentTypeService,
        IContentService contentService,
        IUmbracoContextFactory umbracoContext,
        IScopeProvider scopeProvider)
    {
        _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
        _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
        _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
    }

    public string? GetLoginNodeUrl()
    {
        var portalAuthContentType = _contentTypeService.Get(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage);
        var loginContentNode = _contentService
            .GetPagedOfType(portalAuthContentType.Id, 0, 10, out long _, null)
            .FirstOrDefault(x => x.Name == UmbracoCommercePortalConstants.ContentNodes.PortalAuthLoginPageNodeName);
        if (loginContentNode == null)
        {
            return null;
        }

        using (_scopeProvider.CreateScope(autoComplete: true))
        {
            using (var contextReference = _umbracoContext.EnsureUmbracoContext())
            {
                var context = contextReference.UmbracoContext;

                var content = context.Content.GetById(loginContentNode.Id);

                return content?.Url(Thread.CurrentThread.CurrentCulture.Name);
            }
        }
    }

}
