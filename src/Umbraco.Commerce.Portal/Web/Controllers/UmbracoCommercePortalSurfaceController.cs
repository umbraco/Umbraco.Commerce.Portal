using System;
using System.Linq;
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
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Common.Events;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Portal.Events;
using Umbraco.Commerce.Portal.Extensions;
using Umbraco.Commerce.Portal.Models;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UmbracoCommercePortalSurfaceController : SurfaceController
{
    private readonly IMemberSignInManager _signInManager;
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;
    private readonly IPasswordHasher _hasher;
    private readonly IUmbracoCommerceApi _commerceApi;
    private readonly UmbracoHelper _umbracoHelper;

    public UmbracoCommercePortalSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberSignInManager signInManager,
        IMemberManager memberManager,
        IMemberService memberService,
        IPasswordHasher hasher,
        IUmbracoCommerceApi commerceApi,
        UmbracoHelper umbracoHelper)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _signInManager = signInManager;
        _memberManager = memberManager;
        _memberService = memberService;
        _hasher = hasher;
        _commerceApi = commerceApi;
        _umbracoHelper = umbracoHelper;
    }

    [HttpPost]
    public async Task<IActionResult> Login(Models.LoginModel loginModel)
    {
        ArgumentNullException.ThrowIfNull(loginModel);

        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        var member = _memberService.GetByEmail(loginModel.Email);

        Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(
            member != null ? member.Username : loginModel.Email,
            loginModel.Password,
            loginModel.RememberMe,
            lockoutOnFailure: true);

        if (signInResult.Succeeded)
        {
            var store = CurrentPage.GetStore();

            await EventBus.DispatchAsync(new OnLoginNotification(member, store.Id));

            TempData["LoginSuccess"] = true;

            return Redirect(loginModel.RedirectUrl!);
        }

        if (signInResult.IsLockedOut)
        {
            ModelState.AddModelError(
                "loginModel",
                _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberLockedOut.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberLockedOut.DefaultValue));
        }
        else if (signInResult.IsNotAllowed)
        {
            ModelState.AddModelError(
                "loginModel",
                _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberNotAllowed.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberNotAllowed.DefaultValue));
        }
        else
        {
            ModelState.AddModelError(
                "loginModel",
                _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.InvalidUsernameOrPassword.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.InvalidUsernameOrPassword.DefaultValue));
        }

        return CurrentUmbracoPage();
    }

    [HttpPost]
    public async Task<IActionResult> Logout(PostRedirectModel logoutModel)
    {
        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        var member = _memberService.GetByEmail(HttpContext.User.Identity.Name);
        if (HttpContext.User.Identity?.IsAuthenticated ?? false)
        {
            await _signInManager.SignOutAsync();
        }

        var store = CurrentPage.GetStore();
        await EventBus.DispatchAsync(new OnLogoutNotification(member, store.Id));

        TempData["LogoutSuccess"] = true;

        if (logoutModel.RedirectUrl.IsNullOrWhiteSpace() == false)
        {
            return Redirect(logoutModel.RedirectUrl!);
        }

        return RedirectToCurrentUmbracoPage();
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
    {
        ArgumentNullException.ThrowIfNull(resetPasswordModel);

        if (string.IsNullOrEmpty(resetPasswordModel.Email))
        {
            ModelState.AddModelError(
                "memberData",
                _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.NotAMember.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.NotAMember.DefaultValue));
            return CurrentUmbracoPage();
        }

        // Find member
        var member = _memberService.GetByEmail(resetPasswordModel.Email);
        if (member == null)
        {
            ModelState.AddModelError(
                "memberReference",
                _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberNotFound.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberNotFound.DefaultValue));
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
        if (!result)
        {
            ModelState.AddModelError(
                "memberEmail",
                _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.FailedToSendConfirmationEmail.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.FailedToSendConfirmationEmail.DefaultValue));
            return CurrentUmbracoPage();
        }

        TempData["Success"] = _umbracoHelper.GetDictionaryValueOrDefault(
            UmbracoCommercePortalConstants.Localization.AuthEntries.ResetPasswordEmailSent.Key,
            UmbracoCommercePortalConstants.Localization.AuthEntries.ResetPasswordEmailSent.DefaultValue);

        return CurrentUmbracoPage();
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordModel updatePasswordModel)
    {
        ArgumentNullException.ThrowIfNull(updatePasswordModel);

        var member = _memberService.GetById(updatePasswordModel.MemberId);
        if (member == null)
        {
            ModelState.AddModelError(
                "memberReference",
                _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberNotFound.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberNotFound.DefaultValue));
            return CurrentUmbracoPage();
        }

        member.RawPasswordValue = _hasher.HashPassword(updatePasswordModel.Password);
        _memberService.Save(member);

        TempData["UpdatePasswordSuccess"] = _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.PasswordUpdated.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.PasswordUpdated.DefaultValue);

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
        MemberIdentityUser identityUser = MemberIdentityUser.CreateNew(
            registerModel.Email,
            registerModel.Email,
            UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberType,
            false,
            name);
        IdentityResult identityResult = await _memberManager.CreateAsync(
            identityUser,
            registerModel.Password);
        if (identityResult.Succeeded)
        {
            var member = _memberService.GetByEmail(registerModel.Email);
            member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.FirstName, registerModel.FirstName);
            member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.LastName, registerModel.LastName);
            _memberService.Save(member);

            // Send confirm member email
            var template = await _commerceApi.GetEmailTemplateAsync(
                CurrentPage.GetStore().Id,
                UmbracoCommercePortalConstants.EmailTemplates.ConfirmMemberEmailTemplate.Key);
            var result = await _commerceApi.SendEmailAsync(
                template,
                new EmailModel(name, $"{CurrentPage.Url(mode: Umbraco.Cms.Core.Models.PublishedContent.UrlMode.Absolute)}?key={member.Key}"),
                registerModel.Email,
                "en-US");
            if (!result)
            {
                ModelState.AddModelError(
                    "memberEmail",
                    _umbracoHelper.GetDictionaryValueOrDefault(
                        UmbracoCommercePortalConstants.Localization.AuthEntries.FailedToSendConfirmationEmail.Key,
                        UmbracoCommercePortalConstants.Localization.AuthEntries.FailedToSendConfirmationEmail.DefaultValue));
                return CurrentUmbracoPage();
            }

            TempData["Success"] = _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberAccountCreated.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberAccountCreated.DefaultValue);
        }
        else
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

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
            ModelState.AddModelError(
                "memberReference",
                _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberNotFound.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberNotFound.DefaultValue));
            return CurrentUmbracoPage();
        }

        member.IsApproved = true;
        _memberService.Save(member);

        TempData["ConfirmMemberSuccess"] = _umbracoHelper.GetDictionaryValueOrDefault(
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberConfirmed.Key,
                    UmbracoCommercePortalConstants.Localization.AuthEntries.MemberConfirmed.DefaultValue);

        return CurrentUmbracoPage();
    }
}
