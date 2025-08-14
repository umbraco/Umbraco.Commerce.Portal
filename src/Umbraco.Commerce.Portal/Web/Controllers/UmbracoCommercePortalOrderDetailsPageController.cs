using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Commerce.Core.Api;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UcpPortalOrderDetailsPageController
    : UmbracoCommercePortalBaseController
{
    public UcpPortalOrderDetailsPageController(ILogger<UmbracoPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoCommerceApi commerceApi)
        : base(logger, compositeViewEngine, commerceApi)
    {
    }

    [UmbracoMemberAuthorize(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberType, "", "")]
    public override async Task<IActionResult> Index()
    {
        if (!HttpContext.Request.Query.TryGetValue("orderId", out var orderId)
            || !Guid.TryParse(orderId.ToString(), out var orderGuid))
        {
            return BadRequest("Missing Order ID");
        }

        var order = await CommerceApi.GetOrderAsync(orderGuid);
        if (order == null)
        {
            return NotFound("Order not found");
        }

        return await base.Index();
    }
}
