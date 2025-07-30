using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Commerce.Core.Api;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UmbracoCommercePortalBaseController : UmbracoPageController, IRenderController
{
    /// <summary>
    /// Gets the Umbraco Commerce API.
    /// </summary>
    protected IUmbracoCommerceApi CommerceApi { get; }

    public UmbracoCommercePortalBaseController(
        ILogger<UmbracoPageController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoCommerceApi commerceApi)
        : base(logger, compositeViewEngine)
    {
        CommerceApi = commerceApi;
    }

    public virtual async Task<IActionResult> Index()
    {
        ArgumentNullException.ThrowIfNull(CurrentPage);

        // If the page has a template, render it
        if (CurrentPage.TemplateId.HasValue && CurrentPage.TemplateId.Value > 0)
        {
            return CurrentTemplate(new ContentModel(CurrentPage));
        }

        return View($"~/Views/UmbracoCommercePortal/UmbracoCommercePortal{CurrentPage.Name.Replace(" ", "")}Page.cshtml", CurrentPage);
    }
}
