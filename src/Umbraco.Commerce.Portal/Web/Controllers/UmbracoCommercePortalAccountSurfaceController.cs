using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Portal.Models;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UmbracoCommercePortalAccountSurfaceController : SurfaceController
{
    private readonly IMemberService _memberService;

    public UmbracoCommercePortalAccountSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberService memberService)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberService = memberService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAccount(AccountModel accountModel)
    {
        ArgumentNullException.ThrowIfNull(accountModel);

        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        var member = _memberService.GetById(accountModel.Id);
        member.Email = accountModel.Email;
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.FirstName, accountModel.FirstName);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.LastName, accountModel.LastName);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Country, accountModel.Country);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Street, accountModel.Street);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.City, accountModel.City);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.State, accountModel.State);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.ZipCode, accountModel.ZipCode);
        _memberService.Save(member);

        TempData["Success"] = "Member account updated.";

        return CurrentUmbracoPage();
    }
}
