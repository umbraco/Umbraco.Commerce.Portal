using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

    public override async Task<IActionResult> Index()
    {
        ArgumentNullException.ThrowIfNull(CurrentPage);

        // If the page has a template, render it
        if (CurrentPage.TemplateId.HasValue && CurrentPage.TemplateId.Value > 0)
        {
            return await base.Index();
        }

        return View($"~/Views/UmbracoCommercePortal/UmbracoCommercePortal{CurrentPage.Name.Replace(" ", "")}Page.cshtml", CurrentPage);
    }
}
