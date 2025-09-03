using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Commerce.Core.Api;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UcpPortalAuthPageController
    : UmbracoCommercePortalBaseController
{
    public UcpPortalAuthPageController(ILogger<UmbracoPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoCommerceApi commerceApi)
        : base(logger, compositeViewEngine, commerceApi)
    {
    }
}
