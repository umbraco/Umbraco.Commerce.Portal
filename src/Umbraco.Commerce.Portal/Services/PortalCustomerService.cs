using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Common;
using Umbraco.Commerce.Common.Validation;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.Services;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.Portal.Models;

namespace Umbraco.Commerce.Portal.Services;

/// <summary>
/// Links Portal members to Umbraco Commerce customers. The customer of the current store is the
/// source of truth for a member's profile and addresses; the member keeps its name and email in step.
/// </summary>
public class PortalCustomerService
{
    private readonly ICustomerService _customerService;
    private readonly IUmbracoCommerceApi _commerceApi;
    private readonly IUnitOfWorkProvider _uowProvider;
    private readonly IMemberService _memberService;
    private readonly IMemberManager _memberManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PortalCustomerService"/> class.
    /// </summary>
    public PortalCustomerService(
        ICustomerService customerService,
        IUmbracoCommerceApi commerceApi,
        IUnitOfWorkProvider uowProvider,
        IMemberService memberService,
        IMemberManager memberManager)
    {
        _customerService = customerService;
        _commerceApi = commerceApi;
        _uowProvider = uowProvider;
        _memberService = memberService;
        _memberManager = memberManager;
    }

    /// <summary>
    /// Gets the signed-in member, or <c>null</c> when no member is signed in.
    /// </summary>
    public virtual async Task<IMember?> GetCurrentMemberAsync()
    {
        MemberIdentityUser? identity = await _memberManager.GetCurrentMemberAsync();
        return identity is null ? null : _memberService.GetById(identity.Key);
    }

    /// <summary>
    /// Gets the signed-in member and their customer for the given store, creating or linking the
    /// customer on first use. Returns <c>null</c> when no member is signed in.
    /// </summary>
    public virtual async Task<PortalCustomerContext?> GetCurrentAsync(Guid storeId)
    {
        IMember? member = await GetCurrentMemberAsync();
        if (member is null)
        {
            return null;
        }

        CustomerReadOnly customer = await GetOrCreateCustomerAsync(storeId, member);
        return new PortalCustomerContext(member, customer);
    }

    /// <summary>
    /// Gets the member's customer for the given store. Looks it up by member key, then by email
    /// (and links it), and creates one when neither exists. The first time a customer is set up
    /// for a member, the member's old Portal profile properties are copied onto it.
    /// </summary>
    public virtual Task<CustomerReadOnly> GetOrCreateCustomerAsync(Guid storeId, IMember member)
        => GetOrCreateCustomerAsync(storeId, member, null, null);

    /// <inheritdoc cref="GetOrCreateCustomerAsync(Guid, IMember)"/>
    /// <param name="storeId">The store the customer belongs to.</param>
    /// <param name="member">The member.</param>
    /// <param name="firstName">A first name to use when the customer has none yet.</param>
    /// <param name="lastName">A last name to use when the customer has none yet.</param>
    public virtual async Task<CustomerReadOnly> GetOrCreateCustomerAsync(Guid storeId, IMember member, string? firstName, string? lastName)
    {
        ArgumentNullException.ThrowIfNull(member);

        try
        {
            return await ResolveCustomerAsync(storeId, member, firstName, lastName);
        }
        catch (ValidationException)
        {
            // Customer emails are unique per store. A parallel request (or order finalization)
            // can create the customer between our lookup and our save; resolve again to pick it up.
            return await ResolveCustomerAsync(storeId, member, firstName, lastName);
        }
    }

    /// <summary>
    /// Updates the customer's details and keeps the member's name and email in step.
    /// </summary>
    public virtual async Task<AccountUpdateStatus> UpdateAccountAsync(Guid storeId, IMember member, AccountModel model)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(model);

        CustomerReadOnly customer = await GetOrCreateCustomerAsync(storeId, member);

        var email = model.Email.Trim();
        var firstName = model.FirstName.Trim();
        var lastName = model.LastName.Trim();
        var telephone = model.Telephone?.Trim() ?? string.Empty;

        if (!EmailEquals(member.Email, email) || !EmailEquals(customer.Email, email))
        {
            IMember? otherMember = _memberService.GetByEmail(email);
            if (otherMember is not null && otherMember.Key != member.Key)
            {
                return AccountUpdateStatus.EmailInUse;
            }

            CustomerReadOnly? otherCustomer = await FindCustomerByEmailAsync(storeId, email);
            if (otherCustomer is not null && otherCustomer.Id != customer.Id)
            {
                return AccountUpdateStatus.EmailInUse;
            }
        }

        await _uowProvider.ExecuteAsync(async uow =>
        {
            Customer writable = await customer.AsWritableAsync(uow);
            await writable.SetFirstNameAsync(firstName);
            await writable.SetLastNameAsync(lastName);
            await writable.SetEmailAsync(email);
            await writable.SetTelephoneAsync(telephone);
            await _customerService.SaveCustomerAsync(writable);
            uow.Complete();
        });

        var name = $"{firstName} {lastName}".Trim();
        if (member.Email != email || member.Name != name)
        {
            // The username follows the email (SynchronizeMemberUsernameOnEmailUpdateNotificationHandler).
            member.Email = email;
            member.Name = name;
            _memberService.Save(member);
        }

        return AccountUpdateStatus.Success;
    }

    /// <summary>
    /// Gets the customer's addresses, split by kind, with default addresses first.
    /// </summary>
    public virtual async Task<AddressBook> GetAddressBookAsync(Guid storeId, Guid customerId)
    {
        var addresses = (await _customerService.GetCustomerAddressesAsync(customerId)).ToList();

        var countries = (await _commerceApi.GetCountriesAsync(storeId)).ToDictionary(x => x.Id);
        var regions = (await _commerceApi.GetRegionsAsync(storeId)).ToDictionary(x => x.Id);

        AddressBookEntry ToEntry(CustomerAddress address) => new(
            address,
            address.CountryId.HasValue && countries.TryGetValue(address.CountryId.Value, out CountryReadOnly? country) ? country.Name : string.Empty,
            address.RegionId.HasValue && regions.TryGetValue(address.RegionId.Value, out RegionReadOnly? region) ? region.Name : null);

        List<AddressBookEntry> OfKind(bool isBilling) => addresses
            .Where(x => x.IsBilling == isBilling)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Id)
            .Select(ToEntry)
            .ToList();

        return new AddressBook(OfKind(true), OfKind(false));
    }

    /// <summary>
    /// Gets an address only when it belongs to the given customer; otherwise <c>null</c>.
    /// </summary>
    public virtual async Task<CustomerAddress?> GetCustomerAddressAsync(Guid customerId, int addressId)
    {
        if (addressId <= 0)
        {
            return null;
        }

        CustomerAddress? address = await _customerService.GetCustomerAddressByIdAsync(addressId);
        return address is not null && address.CustomerId == customerId ? address : null;
    }

    /// <summary>
    /// Adds an address to, or updates an address of, the given customer.
    /// </summary>
    public virtual async Task<AddressSaveStatus> SaveAddressAsync(Guid storeId, Guid customerId, AddressBookAddressModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        CustomerAddress? existing = null;
        if (model.Id != 0)
        {
            existing = await GetCustomerAddressAsync(customerId, model.Id);
            if (existing is null)
            {
                return AddressSaveStatus.NotFound;
            }
        }

        CountryReadOnly? country = model.CountryId.HasValue
            ? (await _commerceApi.GetCountriesAsync(storeId)).FirstOrDefault(x => x.Id == model.CountryId.Value)
            : null;
        if (country is null)
        {
            return AddressSaveStatus.InvalidCountry;
        }

        RegionReadOnly? region = null;
        if (model.RegionId.HasValue)
        {
            region = (await _commerceApi.GetRegionsAsync(storeId, country.Id)).FirstOrDefault(x => x.Id == model.RegionId.Value);
            if (region is null)
            {
                return AddressSaveStatus.InvalidRegion;
            }
        }

        if (existing is not null)
        {
            existing.AddressLine1 = model.AddressLine1.Trim();
            existing.AddressLine2 = model.AddressLine2?.Trim() ?? string.Empty;
            existing.City = model.City.Trim();
            existing.ZipCode = model.ZipCode.Trim();
            existing.CountryId = country.Id;
            existing.RegionId = region?.Id;

            // Unsetting a default would leave the kind without one; defaults move with "Set as default".
            existing.IsDefault = existing.IsDefault || model.IsDefault;

            await _customerService.SaveCustomerAddressAsync(existing);
            return AddressSaveStatus.Success;
        }

        // The first address of a kind becomes its default.
        var hasAddressOfKind = (await _customerService.GetCustomerAddressesAsync(customerId))
            .Any(x => x.IsBilling == model.IsBilling);

        var address = new CustomerAddress(
            customerId,
            model.AddressLine1.Trim(),
            model.AddressLine2?.Trim() ?? string.Empty,
            model.City.Trim(),
            model.ZipCode.Trim(),
            country.Id,
            region?.Id,
            model.IsDefault || !hasAddressOfKind,
            model.IsBilling);

        await _customerService.SaveCustomerAddressAsync(address);
        return AddressSaveStatus.Success;
    }

    /// <summary>
    /// Makes an address of the given customer the default of its kind. Returns <c>false</c> when
    /// the address doesn't belong to the customer.
    /// </summary>
    public virtual async Task<bool> SetDefaultAddressAsync(Guid customerId, int addressId)
    {
        CustomerAddress? address = await GetCustomerAddressAsync(customerId, addressId);
        if (address is null)
        {
            return false;
        }

        if (!address.IsDefault)
        {
            // Core demotes the previous default of the same kind.
            address.IsDefault = true;
            await _customerService.SaveCustomerAddressAsync(address);
        }

        return true;
    }

    /// <summary>
    /// Deletes an address of the given customer. Returns <c>false</c> when the address doesn't
    /// belong to the customer.
    /// </summary>
    public virtual async Task<bool> DeleteAddressAsync(Guid customerId, int addressId)
    {
        CustomerAddress? address = await GetCustomerAddressAsync(customerId, addressId);
        if (address is null)
        {
            return false;
        }

        // Core promotes another address of the same kind when the default is deleted.
        await _customerService.DeleteCustomerAddressAsync(address.Id);
        return true;
    }

    /// <summary>
    /// Gets the member's finalized orders in the given store, newest first: orders linked to their
    /// customer, plus orders assigned to the member or placed with their email before the customer
    /// record existed.
    /// </summary>
    public virtual async Task<IReadOnlyList<OrderReadOnly>> GetFinalizedOrdersAsync(Guid storeId, IMember member, CustomerReadOnly customer)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(customer);

        var sets = new List<IEnumerable<OrderReadOnly>>
        {
            await _commerceApi.GetAllOrdersForCustomerAsync(storeId, customer.Id),
            await _commerceApi.GetFinalizedOrdersForCustomerAsync(storeId, member.Key.ToString()),
        };

        foreach (var email in GetEmails(member, customer))
        {
            sets.Add(await _commerceApi.GetFinalizedOrdersForCustomerAsync(storeId, email));
        }

        return MergeOrders(sets, x => x.Id, x => x.FinalizedDate, x => x.IsFinalized);
    }

    /// <summary>
    /// Gets a finalized order of the given store only when it belongs to the member: it's linked to
    /// their customer, assigned to the member, or was placed with their email. Otherwise <c>null</c>.
    /// </summary>
    public virtual async Task<OrderReadOnly?> GetFinalizedOrderAsync(Guid storeId, IMember member, CustomerReadOnly customer, Guid orderId)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(customer);

        OrderReadOnly? order = await _commerceApi.GetOrderAsync(orderId);
        if (order is null || !IsShowableOrder(storeId, order.StoreId, order.IsFinalized))
        {
            return null;
        }

        if (IsOrderOwnedBy(order.CustomerInfo?.CustomerReference, order.CustomerInfo?.Email, member.Key, GetEmails(member, customer)))
        {
            return order;
        }

        var linked = await _commerceApi.GetAllOrdersForCustomerAsync(storeId, customer.Id);
        return IsLinkedOrder(linked.Select(x => x.Id), orderId) ? order : null;
    }

    /// <summary>
    /// Saves the old single billing form as the customer's default billing address.
    /// </summary>
    [Obsolete("Billing details live in the customer's address book now. Use SaveAddressAsync instead. Will be removed in v21.0.0")]
    internal async Task<AddressSaveStatus> SaveBillingInformationAsync(Guid storeId, Guid customerId, BillingInformationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        CountryReadOnly? country = await FindLegacyCountryAsync(storeId, model.Country);
        if (country is null)
        {
            return AddressSaveStatus.InvalidCountry;
        }

        RegionReadOnly? region = MatchRegion(await _commerceApi.GetRegionsAsync(storeId, country.Id), model.State);
        CustomerAddress? currentDefault = (await _customerService.GetCustomerAddressesAsync(customerId))
            .FirstOrDefault(x => x.IsBilling && x.IsDefault);

        return await SaveAddressAsync(storeId, customerId, new AddressBookAddressModel
        {
            Id = currentDefault?.Id ?? 0,
            IsBilling = true,
            AddressLine1 = model.Street,
            City = model.City,
            ZipCode = model.ZipCode,
            CountryId = country.Id,
            RegionId = region?.Id,
            IsDefault = true,
        });
    }

    internal static bool IsShowableOrder(Guid storeId, Guid orderStoreId, bool isFinalized)
        => isFinalized && orderStoreId == storeId;

    internal static bool IsLinkedOrder(IEnumerable<Guid> linkedOrderIds, Guid orderId)
        => linkedOrderIds.Contains(orderId);

    internal static bool IsOrderOwnedBy(string? customerReference, string? orderEmail, Guid memberKey, IEnumerable<string> emails)
    {
        if (!string.IsNullOrWhiteSpace(customerReference)
            && Guid.TryParse(customerReference, out Guid referenceKey)
            && referenceKey == memberKey)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(orderEmail) && emails.Any(x => EmailEquals(x, orderEmail));
    }

    internal static IReadOnlyList<T> MergeOrders<T>(IEnumerable<IEnumerable<T>> sets, Func<T, Guid> idSelector, Func<T, DateTime?> dateSelector, Func<T, bool> isFinalized)
    {
        var seen = new HashSet<Guid>();
        var merged = new List<T>();
        foreach (IEnumerable<T> set in sets)
        {
            foreach (T item in set.Where(isFinalized))
            {
                if (seen.Add(idSelector(item)))
                {
                    merged.Add(item);
                }
            }
        }

        return merged.OrderByDescending(dateSelector).ToList();
    }

    internal static RegionReadOnly? MatchRegion(IEnumerable<RegionReadOnly> regions, string? state)
    {
        var value = state?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var list = regions.ToList();

        // Region codes may carry the country prefix (e.g. "US-CA"); compare the part after the dash too.
        static string Local(string code) => code.Contains('-', StringComparison.Ordinal) ? code[(code.IndexOf('-', StringComparison.Ordinal) + 1)..] : code;

        return list.FirstOrDefault(x => string.Equals(x.Name, value, StringComparison.OrdinalIgnoreCase))
            ?? list.FirstOrDefault(x => string.Equals(x.Code, value, StringComparison.OrdinalIgnoreCase))
            ?? list.FirstOrDefault(x => !string.IsNullOrEmpty(x.Code) && string.Equals(Local(x.Code), Local(value), StringComparison.OrdinalIgnoreCase));
    }

    private async Task<CustomerReadOnly> ResolveCustomerAsync(Guid storeId, IMember member, string? firstName, string? lastName)
    {
        var email = member.Email?.Trim() ?? string.Empty;

        CustomerReadOnly? byKey = await _customerService.GetCustomerByMemberKeyAsync(storeId, member.Key);
        CustomerReadOnly? found = byKey is not null && EmailEquals(byKey.Email, email)
            ? byKey
            : await FindCustomerByEmailAsync(storeId, email) ?? byKey;

        var needsLegacyImport = found is null
            || !found.Properties.ContainsKey(UmbracoCommercePortalConstants.Customers.LegacyProfileImportedPropertyAlias);

        if (found is not null
            && !needsLegacyImport
            && found.MemberKey == member.Key
            && EmailEquals(found.Email, email))
        {
            return found;
        }

        LegacyMemberProfile profile = LegacyMemberProfile.FromMember(member);
        var first = FirstNonEmpty(firstName, profile.FirstName, SplitName(member.Name).First);
        var last = FirstNonEmpty(lastName, profile.LastName, SplitName(member.Name).Last);

        Customer customer = await _uowProvider.ExecuteAsync(async uow =>
        {
            Customer writable = found is not null
                ? await found.AsWritableAsync(uow)
                : await Customer.CreateAsync(uow, storeId, first, last, email);

            await writable.SetMemberKeyAsync(member.Key);
            if (!string.IsNullOrEmpty(email))
            {
                await writable.SetEmailAsync(email);
            }

            if (string.IsNullOrWhiteSpace(writable.FirstName) && string.IsNullOrWhiteSpace(writable.LastName))
            {
                await writable.SetFirstNameAsync(first);
                await writable.SetLastNameAsync(last);
            }

            await _customerService.SaveCustomerAsync(writable);
            return uow.Complete(writable);
        });

        if (needsLegacyImport)
        {
            await ImportLegacyAddressAsync(storeId, customer.Id, profile);

            // Mark the copy as done only once the address is saved, so a failed copy is retried on the next visit.
            customer = await _uowProvider.ExecuteAsync(async uow =>
            {
                Customer writable = await customer.AsWritableAsync(uow);
                await writable.SetPropertyAsync(UmbracoCommercePortalConstants.Customers.LegacyProfileImportedPropertyAlias, "true");
                await _customerService.SaveCustomerAsync(writable);
                return uow.Complete(writable);
            });
        }

        return customer.AsReadOnly();
    }

    private async Task ImportLegacyAddressAsync(Guid storeId, Guid customerId, LegacyMemberProfile profile)
    {
        if (!profile.HasAddress)
        {
            return;
        }

        CountryReadOnly? country = await FindLegacyCountryAsync(storeId, profile.Country);
        RegionReadOnly? region = country is not null
            ? MatchRegion(await _commerceApi.GetRegionsAsync(storeId, country.Id), profile.State)
            : null;

        var billing = (await _customerService.GetCustomerAddressesAsync(customerId))
            .Where(x => x.IsBilling)
            .ToList();

        var isDuplicate = billing.Any(x =>
            TextEquals(x.AddressLine1, profile.Street)
            && TextEquals(x.AddressLine2, string.Empty)
            && TextEquals(x.City, profile.City)
            && TextEquals(x.ZipCode, profile.ZipCode)
            && x.CountryId == country?.Id);
        if (isDuplicate)
        {
            return;
        }

        var address = new CustomerAddress(
            customerId,
            profile.Street,
            string.Empty,
            profile.City,
            profile.ZipCode,
            country?.Id,
            region?.Id,
            !billing.Any(x => x.IsDefault),
            true);

        await _customerService.SaveCustomerAddressAsync(address);
    }

    // The old country property is a store entity picker (country id); the old billing form posted a country code.
    private async Task<CountryReadOnly?> FindLegacyCountryAsync(Guid storeId, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        CountryReadOnly? country = Guid.TryParse(value, out Guid countryId)
            ? await _commerceApi.GetCountryAsync(countryId)
            : await _commerceApi.GetCountryAsync(storeId, value.Trim());

        return country is not null && country.StoreId == storeId ? country : null;
    }

    private async Task<CustomerReadOnly?> FindCustomerByEmailAsync(Guid storeId, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return (await _customerService.GetCustomersByEmailAsync(email))
            .FirstOrDefault(x => x.StoreId == storeId && EmailEquals(x.Email, email));
    }

    private static IEnumerable<string> GetEmails(IMember member, CustomerReadOnly customer)
        => new[] { member.Email, customer.Email }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);

    private static (string First, string Last) SplitName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        var space = trimmed.IndexOf(' ', StringComparison.Ordinal);
        return space < 0 ? (trimmed, string.Empty) : (trimmed[..space], trimmed[(space + 1)..].Trim());
    }

    private static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim() ?? string.Empty;

    private static bool EmailEquals(string? a, string? b)
        => string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool TextEquals(string? a, string? b)
        => string.Equals(a?.Trim() ?? string.Empty, b?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
}
