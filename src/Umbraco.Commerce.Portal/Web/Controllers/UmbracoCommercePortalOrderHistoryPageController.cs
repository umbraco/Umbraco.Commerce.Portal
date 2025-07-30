using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Commerce.Core.Api;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UcpPortalOrderHistoryPageController
    : UmbracoCommercePortalBaseController
{
    public UcpPortalOrderHistoryPageController(ILogger<UmbracoPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoCommerceApi commerceApi)
        : base(logger, compositeViewEngine, commerceApi)
    {
    }

    [UmbracoMemberAuthorize(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberType, "", "")]
    public override async Task<IActionResult> Index() => await base.Index();
}
