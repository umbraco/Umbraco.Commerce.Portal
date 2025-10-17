using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Portal.Models;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UmbracoCommercePortalAccountSurfaceController : SurfaceController
{
    private readonly IMemberService _memberService;
    private readonly IMemberManager _memberManager;
    private readonly UmbracoHelper _umbracoHelper;

    public UmbracoCommercePortalAccountSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberService memberService,
        IMemberManager memberManager,
        UmbracoHelper umbracoHelper)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberService = memberService;
        _umbracoHelper = umbracoHelper;
        _memberManager = memberManager;
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
        _memberService.Save(member);

        TempData["Success"] = _umbracoHelper.GetDictionaryValueOrDefault(
            UmbracoCommercePortalConstants.Localization.AuthEntries.MemberAccountUpdated.Key,
            UmbracoCommercePortalConstants.Localization.AuthEntries.MemberAccountUpdated.DefaultValue);

        return CurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBillingInformation(BillingInformationModel billingInformationModel)
    {
        ArgumentNullException.ThrowIfNull(billingInformationModel);

        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        var member = _memberService.GetById(billingInformationModel.Id);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Country, billingInformationModel.Country);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Street, billingInformationModel.Street);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.City, billingInformationModel.City);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.State, billingInformationModel.State);
        member.SetValue(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.ZipCode, billingInformationModel.ZipCode);
        _memberService.Save(member);

        TempData["Success"] = _umbracoHelper.GetDictionaryValueOrDefault(
           UmbracoCommercePortalConstants.Localization.AuthEntries.MemberBillingInformationUpdated.Key,
           UmbracoCommercePortalConstants.Localization.AuthEntries.MemberBillingInformationUpdated.DefaultValue);

        return CurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel changePasswordModel)
    {
        ArgumentNullException.ThrowIfNull(changePasswordModel);

        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        var member = _memberService.GetById(changePasswordModel.MemberKey);

        // Validate if current credentials are correct
        var credentialsValidationResult = await _memberManager.ValidateCredentialsAsync(member.Username, changePasswordModel.CurrentPassword);
        if (!credentialsValidationResult)
        {
            ModelState.AddModelError("CurrentPassword", _umbracoHelper.GetDictionaryValueOrDefault(
                UmbracoCommercePortalConstants.Localization.AuthEntries.MemberCurrentPasswordIncorrect.Key,
                UmbracoCommercePortalConstants.Localization.AuthEntries.MemberCurrentPasswordIncorrect.DefaultValue));
            return CurrentUmbracoPage();
        }

        // Validate new password
        var passwordValidationResult = await _memberManager.ValidatePasswordAsync(changePasswordModel.NewPassword);
        if (!passwordValidationResult.Succeeded)
        {
            ModelState.AddModelError("NewPassword", string.Join(", ", passwordValidationResult.Errors));
            return CurrentUmbracoPage();
        }

        var identityMember = await _memberManager.GetCurrentMemberAsync();
        var identityResult = await _memberManager.ChangePasswordAsync(identityMember, changePasswordModel.CurrentPassword, changePasswordModel.NewPassword);
        if (!identityResult.Succeeded)
        {
            ModelState.AddModelError("NewPassword", string.Join(", ", identityResult.Errors));
            return CurrentUmbracoPage();
        }

        TempData["Success"] = _umbracoHelper.GetDictionaryValueOrDefault(
           UmbracoCommercePortalConstants.Localization.AuthEntries.MemberPasswordChanged.Key,
           UmbracoCommercePortalConstants.Localization.AuthEntries.MemberPasswordChanged.DefaultValue);

        return CurrentUmbracoPage();
    }
}
