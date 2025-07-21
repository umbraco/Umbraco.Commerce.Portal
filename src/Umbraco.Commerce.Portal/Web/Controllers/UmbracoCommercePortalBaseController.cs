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

    public virtual Task<IActionResult> Index()
          => Task.FromResult(CurrentTemplate(new ContentModel(CurrentPage)));
}
