using System.Reflection;
using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Common;
using Umbraco.Commerce.Common.Events;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.Services;
using Umbraco.Commerce.Portal.Services;

namespace Umbraco.Commerce.Portal.Tests.Services;

/// <summary>
/// Mocks for <see cref="PortalCustomerService"/>: Commerce services, a pass-through unit of work,
/// and real customer/country/region entities built through their public factories.
/// </summary>
internal sealed class PortalCustomerServiceFixture
{
    public static readonly Guid StoreId = Guid.NewGuid();
    public static readonly Guid OtherStoreId = Guid.NewGuid();

    public Mock<ICustomerService> CustomerService { get; } = new();

    public Mock<IUmbracoCommerceApi> CommerceApi { get; } = new();

    public Mock<IUnitOfWorkProvider> UowProvider { get; } = new();

    public Mock<IMemberService> MemberService { get; } = new();

    public Mock<IMemberManager> MemberManager { get; } = new();

    public Mock<IUnitOfWork> Uow { get; } = new();

    public List<CustomerAddress> Addresses { get; } = [];

    public List<Customer> SavedCustomers { get; } = [];

    static PortalCustomerServiceFixture() => InstallNoOpEventBus();

    public PortalCustomerServiceFixture()
    {
        UowProvider
            .Setup(x => x.ExecuteAsync(It.IsAny<Func<IUnitOfWork, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<IUnitOfWork, Task> action, CancellationToken _) => action(Uow.Object));
        UowProvider
            .Setup(x => x.ExecuteAsync(It.IsAny<Func<IUnitOfWork, Task<Customer>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<IUnitOfWork, Task<Customer>> action, CancellationToken _) => action(Uow.Object));

        CustomerService
            .Setup(x => x.SaveCustomerAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Callback((Customer customer, CancellationToken _) => SavedCustomers.Add(customer))
            .Returns(Task.CompletedTask);
        CustomerService
            .Setup(x => x.GetCustomersByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync([]);
        CustomerService
            .Setup(x => x.GetCustomerAddressesAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid customerId) => Addresses.Where(x => x.CustomerId == customerId).ToList());
        CustomerService
            .Setup(x => x.GetCustomerAddressByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => Addresses.FirstOrDefault(x => x.Id == id));
        CustomerService
            .Setup(x => x.SaveCustomerAddressAsync(It.IsAny<CustomerAddress>(), It.IsAny<CancellationToken>()))
            .Callback((CustomerAddress address, CancellationToken _) =>
            {
                if (address.Id == 0)
                {
                    address.Id = Addresses.Count == 0 ? 1 : Addresses.Max(x => x.Id) + 1;
                    Addresses.Add(address);
                }
            })
            .Returns(Task.CompletedTask);

        CommerceApi.Setup(x => x.GetCountriesAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        CommerceApi.Setup(x => x.GetRegionsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        CommerceApi.Setup(x => x.GetRegionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync([]);
    }

    public PortalCustomerService CreateService() => new(
        CustomerService.Object,
        CommerceApi.Object,
        UowProvider.Object,
        MemberService.Object,
        MemberManager.Object);

    public static Mock<IMember> CreateMember(string email, string name = "", Dictionary<string, string>? values = null)
    {
        var member = new Mock<IMember>();
        member.SetupGet(x => x.Key).Returns(Guid.NewGuid());
        member.SetupProperty(x => x.Email, email);
        member.SetupProperty(x => x.Name, name);
        member
            .Setup(x => x.GetValue<string>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns((string alias, string? _, string? _, bool _) => values?.GetValueOrDefault(alias));
        return member;
    }

    public async Task<Customer> CreateCustomerAsync(string email, string firstName = "", string lastName = "", Guid? memberKey = null, bool imported = false, Guid? storeId = null)
    {
        Customer customer = await Customer.CreateAsync(Uow.Object, storeId ?? StoreId, firstName, lastName, email);
        await customer.SetMemberKeyAsync(memberKey);
        if (imported)
        {
            await customer.SetPropertyAsync(UmbracoCommercePortalConstants.Customers.LegacyProfileImportedPropertyAlias, "true");
        }

        return customer;
    }

    public Task<Country> CreateCountryAsync(string code, string name, Guid? storeId = null)
        => Country.CreateAsync(Uow.Object, Guid.NewGuid(), storeId ?? StoreId, code, name);

    public Task<Region> CreateRegionAsync(Guid countryId, string code, string name)
        => Region.CreateAsync(Uow.Object, Guid.NewGuid(), StoreId, countryId, code, name);

    public CustomerAddress AddAddress(Guid customerId, bool isBilling, bool isDefault, string line1 = "Main Street 1", Guid? countryId = null)
    {
        var address = new CustomerAddress(customerId, line1, string.Empty, "Odense", "5000", countryId, null, isDefault, isBilling)
        {
            Id = Addresses.Count == 0 ? 1 : Addresses.Max(x => x.Id) + 1,
        };
        Addresses.Add(address);
        return address;
    }

    public void SetupCountries(params CountryReadOnly[] countries)
    {
        CommerceApi.Setup(x => x.GetCountriesAsync(StoreId)).ReturnsAsync(countries);
        foreach (CountryReadOnly country in countries)
        {
            CommerceApi.Setup(x => x.GetCountryAsync(country.Id)).ReturnsAsync(country);
            CommerceApi.Setup(x => x.GetCountryAsync(country.StoreId, country.Code)).ReturnsAsync(country);
        }
    }

    // Country and region setters raise validation events through Commerce's static EventBus, which
    // resolves its dispatcher from an internal service locator. Point it at no-op handlers (as Core's own tests do).
    private static void InstallNoOpEventBus()
    {
        var handlerFactory = new Mock<IAsyncEventHandlerFactory>();
        handlerFactory.Setup(x => x.CreateFor(It.IsAny<IEvent>())).Returns([]);
        var dispatcher = new Mock<IAsyncEventDispatcher>();
        var factory = new Mock<IFactory>();
        factory.Setup(x => x.GetInstance<IAsyncEventHandlerFactory>()).Returns(handlerFactory.Object);
        factory.Setup(x => x.GetInstance<IAsyncEventDispatcher>()).Returns(dispatcher.Object);

        PropertyInfo factoryProperty = typeof(IFactory).Assembly
            .GetType("Umbraco.Commerce.Common.ServiceLocator", throwOnError: true)!
            .GetProperty("Factory", BindingFlags.Static | BindingFlags.NonPublic)!;
        if (factoryProperty.GetValue(null) is null)
        {
            factoryProperty.SetValue(null, factory.Object);
        }
    }

    public void SetupRegions(Guid countryId, params RegionReadOnly[] regions)
        => CommerceApi.Setup(x => x.GetRegionsAsync(StoreId, countryId)).ReturnsAsync(regions);
}
