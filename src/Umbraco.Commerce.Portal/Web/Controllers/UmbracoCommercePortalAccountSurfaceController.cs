using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Portal.Extensions;
using Umbraco.Commerce.Portal.Models;
using Umbraco.Commerce.Portal.Services;

namespace Umbraco.Commerce.Portal.Web.Controllers;

/// <summary>
/// My Account actions. Every action acts on the signed-in member and their customer in the
/// current store; member and address ids posted from forms are never used to pick whose data changes.
/// </summary>
[UmbracoMemberAuthorize]
public class UmbracoCommercePortalAccountSurfaceController : SurfaceController
{
    private readonly IMemberManager _memberManager;
    private readonly PortalCustomerService _portalCustomerService;
    private readonly UmbracoHelper _umbracoHelper;

    [ActivatorUtilitiesConstructor]
    public UmbracoCommercePortalAccountSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberManager memberManager,
        PortalCustomerService portalCustomerService,
        UmbracoHelper umbracoHelper)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberManager = memberManager;
        _portalCustomerService = portalCustomerService;
        _umbracoHelper = umbracoHelper;
    }

    [Obsolete("Use the constructor that takes PortalCustomerService. Will be removed in v21.0.0")]
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
        : this(
            umbracoContextAccessor,
            databaseFactory,
            services,
            appCaches,
            profilingLogger,
            publishedUrlProvider,
            memberManager,
            StaticServiceProvider.Instance.GetRequiredService<PortalCustomerService>(),
            umbracoHelper)
    {
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

        var store = CurrentPage!.GetStore();
        PortalCustomerContext? context = await _portalCustomerService.GetCurrentAsync(store.Id);
        if (context is null)
        {
            return Forbid();
        }

        AccountUpdateStatus status = await _portalCustomerService.UpdateAccountAsync(store.Id, context.Member, accountModel);
        if (status == AccountUpdateStatus.EmailInUse)
        {
            ModelState.AddModelError(
                $"{nameof(accountModel)}.{nameof(AccountModel.Email)}",
                Localize(UmbracoCommercePortalConstants.Localization.AuthEntries.EmailInUse));
            return CurrentUmbracoPage();
        }

        TempData["Success"] = Localize(UmbracoCommercePortalConstants.Localization.AuthEntries.MemberAccountUpdated);

        return RedirectToCurrentUmbracoPage();
    }

    [Obsolete("Billing details live in the customer's address book now. Post to SaveAddress instead. Will be removed in v21.0.0")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBillingInformation(BillingInformationModel billingInformationModel)
    {
        ArgumentNullException.ThrowIfNull(billingInformationModel);

        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        var store = CurrentPage!.GetStore();
        PortalCustomerContext? context = await _portalCustomerService.GetCurrentAsync(store.Id);
        if (context is null)
        {
            return Forbid();
        }

        AddressSaveStatus status = await _portalCustomerService.SaveBillingInformationAsync(store.Id, context.Customer.Id, billingInformationModel);
        if (status != AddressSaveStatus.Success)
        {
            ModelState.AddModelError(nameof(billingInformationModel), Localize(ToError(status)));
            return CurrentUmbracoPage();
        }

        TempData["Success"] = Localize(UmbracoCommercePortalConstants.Localization.AuthEntries.MemberBillingInformationUpdated);

        return CurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAddress(AddressBookAddressModel addressModel)
    {
        ArgumentNullException.ThrowIfNull(addressModel);

        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        var store = CurrentPage!.GetStore();
        PortalCustomerContext? context = await _portalCustomerService.GetCurrentAsync(store.Id);
        if (context is null)
        {
            return Forbid();
        }

        AddressSaveStatus status = await _portalCustomerService.SaveAddressAsync(store.Id, context.Customer.Id, addressModel);
        if (status != AddressSaveStatus.Success)
        {
            ModelState.AddModelError(nameof(addressModel), Localize(ToError(status)));
            return CurrentUmbracoPage();
        }

        TempData["Success"] = Localize(UmbracoCommercePortalConstants.Localization.AuthEntries.AddressSaved);

        return RedirectToCurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultAddress(int addressId)
    {
        var store = CurrentPage!.GetStore();
        PortalCustomerContext? context = await _portalCustomerService.GetCurrentAsync(store.Id);
        if (context is null)
        {
            return Forbid();
        }

        if (!await _portalCustomerService.SetDefaultAddressAsync(context.Customer.Id, addressId))
        {
            ModelState.AddModelError(nameof(addressId), Localize(UmbracoCommercePortalConstants.Localization.AuthEntries.AddressNotFound));
            return CurrentUmbracoPage();
        }

        TempData["Success"] = Localize(UmbracoCommercePortalConstants.Localization.AuthEntries.DefaultAddressUpdated);

        return RedirectToCurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int addressId)
    {
        var store = CurrentPage!.GetStore();
        PortalCustomerContext? context = await _portalCustomerService.GetCurrentAsync(store.Id);
        if (context is null)
        {
            return Forbid();
        }

        if (!await _portalCustomerService.DeleteAddressAsync(context.Customer.Id, addressId))
        {
            ModelState.AddModelError(nameof(addressId), Localize(UmbracoCommercePortalConstants.Localization.AuthEntries.AddressNotFound));
            return CurrentUmbracoPage();
        }

        TempData["Success"] = Localize(UmbracoCommercePortalConstants.Localization.AuthEntries.AddressDeleted);

        return RedirectToCurrentUmbracoPage();
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

        MemberIdentityUser? identityMember = await _memberManager.GetCurrentMemberAsync();
        if (identityMember is null)
        {
            return Forbid();
        }

        // Validate if current credentials are correct
        var credentialsValidationResult = await _memberManager.ValidateCredentialsAsync(identityMember.UserName!, changePasswordModel.CurrentPassword);
        if (!credentialsValidationResult)
        {
            ModelState.AddModelError("CurrentPassword", Localize(UmbracoCommercePortalConstants.Localization.AuthEntries.MemberCurrentPasswordIncorrect));
            return CurrentUmbracoPage();
        }

        // Validate new password
        var passwordValidationResult = await _memberManager.ValidatePasswordAsync(changePasswordModel.NewPassword);
        if (!passwordValidationResult.Succeeded)
        {
            ModelState.AddModelError("NewPassword", string.Join(", ", passwordValidationResult.Errors.Select(x => x.Description)));
            return CurrentUmbracoPage();
        }

        var identityResult = await _memberManager.ChangePasswordAsync(identityMember, changePasswordModel.CurrentPassword, changePasswordModel.NewPassword);
        if (!identityResult.Succeeded)
        {
            ModelState.AddModelError("NewPassword", string.Join(", ", identityResult.Errors.Select(x => x.Description)));
            return CurrentUmbracoPage();
        }

        TempData["Success"] = Localize(UmbracoCommercePortalConstants.Localization.AuthEntries.MemberPasswordChanged);

        return CurrentUmbracoPage();
    }

    private static UmbracoCommercePortalConstants.Localization.LocalizationEntry ToError(AddressSaveStatus status) => status switch
    {
        AddressSaveStatus.InvalidCountry => UmbracoCommercePortalConstants.Localization.AuthEntries.InvalidCountry,
        AddressSaveStatus.InvalidRegion => UmbracoCommercePortalConstants.Localization.AuthEntries.InvalidRegion,
        _ => UmbracoCommercePortalConstants.Localization.AuthEntries.AddressNotFound,
    };

    private string Localize(UmbracoCommercePortalConstants.Localization.LocalizationEntry entry)
        => _umbracoHelper.GetDictionaryValueOrDefault(entry.Key, entry.DefaultValue);
}
