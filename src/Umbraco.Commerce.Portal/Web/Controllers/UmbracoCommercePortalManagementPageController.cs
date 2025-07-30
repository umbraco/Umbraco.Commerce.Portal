using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Commerce.Core.Api;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UcpPortalManagementPageController : UmbracoCommercePortalBaseController
{
    public UcpPortalManagementPageController(ILogger<UmbracoPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoCommerceApi commerceApi)
        : base(logger, compositeViewEngine, commerceApi)
    {
    }

    public override async Task<IActionResult> Index()
    {
        // If no template is set, return first child if available
        var children = CurrentPage.Children().ToList();
        if (children.Count > 0)
        {
            var firstChild = children.First();
            return RedirectPermanent(firstChild.Url());
        }

        return await base.Index();
    }
}
