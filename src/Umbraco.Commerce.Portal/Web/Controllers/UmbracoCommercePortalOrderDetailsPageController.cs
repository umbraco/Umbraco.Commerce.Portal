using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Portal.Extensions;
using Umbraco.Commerce.Portal.Models;
using Umbraco.Commerce.Portal.Services;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UcpPortalOrderDetailsPageController
    : UmbracoCommercePortalBaseController
{
    private readonly PortalCustomerService _portalCustomerService;

    [ActivatorUtilitiesConstructor]
    public UcpPortalOrderDetailsPageController(
        ILogger<UmbracoPageController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoCommerceApi commerceApi,
        PortalCustomerService portalCustomerService)
        : base(logger, compositeViewEngine, commerceApi)
        => _portalCustomerService = portalCustomerService;

    [Obsolete("Use the constructor that takes PortalCustomerService. Will be removed in v21.0.0")]
    public UcpPortalOrderDetailsPageController(ILogger<UmbracoPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoCommerceApi commerceApi)
        : this(logger, compositeViewEngine, commerceApi, StaticServiceProvider.Instance.GetRequiredService<PortalCustomerService>())
    {
    }

    public override async Task<IActionResult> Index()
    {
        if (!HttpContext.Request.Query.TryGetValue("orderId", out var orderId)
            || !Guid.TryParse(orderId.ToString(), out var orderGuid))
        {
            return BadRequest("Missing Order ID");
        }

        // Only the signed-in member's own finalized orders in this store are shown.
        var store = CurrentPage!.GetStore();
        PortalCustomerContext? context = await _portalCustomerService.GetCurrentAsync(store.Id);
        var order = context is not null
            ? await _portalCustomerService.GetFinalizedOrderAsync(store.Id, context.Member, context.Customer, orderGuid)
            : null;
        if (order == null)
        {
            return NotFound("Order not found");
        }

        return await base.Index();
    }
}
