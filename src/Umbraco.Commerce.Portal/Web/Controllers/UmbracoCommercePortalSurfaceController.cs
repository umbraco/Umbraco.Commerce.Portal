using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Portal.Extensions;
using Umbraco.Commerce.Portal.Models;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UmbracoCommercePortalSurfaceController : SurfaceController
{
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;
    private readonly IPasswordHasher _hasher;
    private readonly IUmbracoCommerceApi _commerceApi;

    public UmbracoCommercePortalSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberManager memberManager,
        IMemberService memberService,
        IPasswordHasher hasher,
        IUmbracoCommerceApi commerceApi)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberManager = memberManager;
        _memberService = memberService;
        _hasher = hasher;
        _commerceApi = commerceApi;
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
    {
        ArgumentNullException.ThrowIfNull(resetPasswordModel);

        if (string.IsNullOrEmpty(resetPasswordModel.Email))
        {
            ModelState.AddModelError("memberData", "Invalid request data.");
            return CurrentUmbracoPage();
        }

        // Find member
        var member = _memberService.GetByEmail(resetPasswordModel.Email);
        if (member == null)
        {
            ModelState.AddModelError("memberReference", "Member not found.");
            return CurrentUmbracoPage();
        }

        // Send reset password email
        var template = await _commerceApi.GetEmailTemplateAsync(
            CurrentPage.GetStore().Id,
            UmbracoCommercePortalConstants.EmailTemplates.ResetPasswordEmailTemplate.Key);
        var result = await _commerceApi.SendEmailAsync(
            template,
            new EmailModel(member.Name, $"{CurrentPage.Url(mode: Umbraco.Cms.Core.Models.PublishedContent.UrlMode.Absolute)}?key={member.Key}"),
            member.Email,
            "en-US");

        TempData["Success"] = "A reset password email has been sent to your email address.";

        return CurrentUmbracoPage();
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordModel updatePasswordModel)
    {
        ArgumentNullException.ThrowIfNull(updatePasswordModel);

        var member = _memberService.GetById(updatePasswordModel.MemberId);
        if (member == null)
        {
            ModelState.AddModelError("memberReference", "Member not found.");
            return CurrentUmbracoPage();
        }

        member.RawPasswordValue = _hasher.HashPassword(updatePasswordModel.Password);
        _memberService.Save(member);

        TempData["UpdatePasswordSuccess"] = "Password updated.";

        return CurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterMember(RegisterMemberModel registerModel)
    {
        ArgumentNullException.ThrowIfNull(registerModel);

        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        var name = $"{registerModel.FirstName} {registerModel.LastName}";
        var identityUser = MemberIdentityUser.CreateNew(
            registerModel.Email,
            registerModel.Email,
            "member",
            false,
            name);
        IdentityResult identityResult = await _memberManager.CreateAsync(
            identityUser,
            registerModel.Password);
        if (identityResult.Succeeded)
        {
            var member = _memberService.GetByEmail(registerModel.Email);

            // Send confirm member email
            var template = await _commerceApi.GetEmailTemplateAsync(
                CurrentPage.GetStore().Id,
                UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Key);
            var result = await _commerceApi.SendEmailAsync(
                template,
                new EmailModel(name, $"{CurrentPage.Url(mode: Umbraco.Cms.Core.Models.PublishedContent.UrlMode.Absolute)}?key={member.Key}"),
                registerModel.Email,
                "en-US");

            TempData["Success"] = "Member account created. Please confirm it from the received email.";
        }
        else
        {
            ModelState.AddModelError("memberData", string.Join(", ", identityResult.Errors));
            return CurrentUmbracoPage();
        }

        return CurrentUmbracoPage();
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmMember(ConfirmMemberModel confirmMemberModel)
    {
        var member = _memberService.GetById(confirmMemberModel.MemberId);
        if (member == null)
        {
            ModelState.AddModelError("memberReference", "Member not found.");
            return CurrentUmbracoPage();
        }

        member.IsApproved = true;
        _memberService.Save(member);

        TempData["ConfirmMemberSuccess"] = "Member confirmed.";

        return CurrentUmbracoPage();
    }
}
