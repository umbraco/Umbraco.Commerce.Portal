using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Portal.Models;
using Umbraco.Commerce.Portal.Services;
using static Umbraco.Commerce.Portal.Tests.Services.PortalCustomerServiceFixture;

namespace Umbraco.Commerce.Portal.Tests.Services;

public class PortalCustomerServiceAccountAndOrderTests
{
    private readonly PortalCustomerServiceFixture _fixture = new();
    private readonly PortalCustomerService _service;

    public PortalCustomerServiceAccountAndOrderTests() => _service = _fixture.CreateService();

    [Fact]
    public async Task UpdateAccountAsync_WhenDetailsChange_UpdatesTheCustomerAndTheMember()
    {
        Mock<IMember> member = CreateMember("ada@example.com", "Ada Lovelace");
        Customer customer = await LinkCustomerAsync(member.Object);

        AccountUpdateStatus status = await _service.UpdateAccountAsync(StoreId, member.Object, new AccountModel
        {
            FirstName = " Augusta ",
            LastName = "Byron",
            Email = "augusta@example.com",
            Telephone = "+45 12 34 56 78",
        });

        Assert.Equal(AccountUpdateStatus.Success, status);
        Assert.Equal("Augusta", customer.FirstName);
        Assert.Equal("Byron", customer.LastName);
        Assert.Equal("augusta@example.com", customer.Email);
        Assert.Equal("+45 12 34 56 78", customer.Telephone);
        Assert.Equal("augusta@example.com", member.Object.Email);
        Assert.Equal("Augusta Byron", member.Object.Name);
        _fixture.MemberService.Verify(x => x.Save(member.Object, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAccountAsync_WhenAnotherMemberUsesTheEmail_RefusesAndChangesNothing()
    {
        Mock<IMember> member = CreateMember("ada@example.com", "Ada Lovelace");
        Customer customer = await LinkCustomerAsync(member.Object);
        Mock<IMember> other = CreateMember("taken@example.com");
        _fixture.MemberService.Setup(x => x.GetByEmail("taken@example.com")).Returns(other.Object);

        AccountUpdateStatus status = await _service.UpdateAccountAsync(StoreId, member.Object, new AccountModel
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "taken@example.com",
        });

        Assert.Equal(AccountUpdateStatus.EmailInUse, status);
        Assert.Equal("ada@example.com", customer.Email);
        Assert.Equal("ada@example.com", member.Object.Email);
        Assert.Empty(_fixture.SavedCustomers);
        _fixture.MemberService.Verify(x => x.Save(It.IsAny<IMember>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAccountAsync_WhenAnotherCustomerOfTheStoreUsesTheEmail_Refuses()
    {
        Mock<IMember> member = CreateMember("ada@example.com", "Ada Lovelace");
        await LinkCustomerAsync(member.Object);
        Customer guest = await _fixture.CreateCustomerAsync("guest@example.com");
        _fixture.CustomerService.Setup(x => x.GetCustomersByEmailAsync("guest@example.com")).ReturnsAsync([guest]);

        AccountUpdateStatus status = await _service.UpdateAccountAsync(StoreId, member.Object, new AccountModel
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "guest@example.com",
        });

        Assert.Equal(AccountUpdateStatus.EmailInUse, status);
        Assert.Empty(_fixture.SavedCustomers);
    }

    [Fact]
    public void MergeOrders_WhenSetsOverlap_ReturnsEachOrderOnceNewestFirst()
    {
        var a = new TestOrder(Guid.NewGuid(), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var b = new TestOrder(Guid.NewGuid(), new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        var c = new TestOrder(Guid.NewGuid(), new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        var merged = Merge([a, b], [b, c], [a with { }, c]);

        Assert.Equal([b, c, a], merged);
    }

    [Fact]
    public void MergeOrders_WhenAnOrderIsNotFinalized_LeavesItOut()
    {
        var finalized = new TestOrder(Guid.NewGuid(), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var cart = new TestOrder(Guid.NewGuid(), null);

        Assert.Equal([finalized], Merge([finalized, cart], [cart]));
    }

    [Fact]
    public void MergeOrders_WhenAllSetsAreEmpty_ReturnsEmpty()
        => Assert.Empty(Merge([], []));

    [Fact]
    public async Task GetFinalizedOrdersAsync_WhenCalled_LooksUpTheCustomerLinkTheMemberKeyAndEachEmailOnce()
    {
        Mock<IMember> member = CreateMember("ada@example.com", "Ada Lovelace");
        Customer customer = await _fixture.CreateCustomerAsync("ADA@example.com", "Ada", "Lovelace", member.Object.Key, imported: true);
        _fixture.CommerceApi.Setup(x => x.GetAllOrdersForCustomerAsync(StoreId, customer.Id)).ReturnsAsync([]);
        _fixture.CommerceApi.Setup(x => x.GetFinalizedOrdersForCustomerAsync(StoreId, It.IsAny<string>())).ReturnsAsync([]);

        var orders = await _service.GetFinalizedOrdersAsync(StoreId, member.Object, customer);

        Assert.Empty(orders);
        _fixture.CommerceApi.Verify(x => x.GetAllOrdersForCustomerAsync(StoreId, customer.Id), Times.Once);
        _fixture.CommerceApi.Verify(x => x.GetFinalizedOrdersForCustomerAsync(StoreId, member.Object.Key.ToString()), Times.Once);
        _fixture.CommerceApi.Verify(x => x.GetFinalizedOrdersForCustomerAsync(StoreId, It.Is<string>(e => e.Equals("ada@example.com", StringComparison.OrdinalIgnoreCase))), Times.Once);
    }

    [Fact]
    public void IsShowableOrder_WhenFinalizedInTheStore_IsTrue()
        => Assert.True(PortalCustomerService.IsShowableOrder(StoreId, StoreId, isFinalized: true));

    [Fact]
    public void IsShowableOrder_WhenInAnotherStore_IsFalse()
        => Assert.False(PortalCustomerService.IsShowableOrder(StoreId, OtherStoreId, isFinalized: true));

    [Fact]
    public void IsShowableOrder_WhenNotFinalized_IsFalse()
        => Assert.False(PortalCustomerService.IsShowableOrder(StoreId, StoreId, isFinalized: false));

    [Fact]
    public void IsLinkedOrder_WhenTheCustomerIsLinkedToTheOrder_IsTrue()
    {
        var orderId = Guid.NewGuid();

        Assert.True(PortalCustomerService.IsLinkedOrder([Guid.NewGuid(), orderId], orderId));
    }

    [Fact]
    public void IsLinkedOrder_WhenTheCustomerIsNotLinkedToTheOrder_IsFalse()
        => Assert.False(PortalCustomerService.IsLinkedOrder([Guid.NewGuid()], Guid.NewGuid()));

    [Fact]
    public void IsOrderOwnedBy_WhenOrderIsAssignedToTheMember_IsTrue()
    {
        var memberKey = Guid.NewGuid();

        Assert.True(PortalCustomerService.IsOrderOwnedBy(memberKey.ToString(), "someone@example.com", memberKey, ["ada@example.com"]));
    }

    [Fact]
    public void IsOrderOwnedBy_WhenOrderEmailMatchesIgnoringCase_IsTrue()
        => Assert.True(PortalCustomerService.IsOrderOwnedBy(null, "ADA@example.com", Guid.NewGuid(), ["ada@example.com"]));

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("not-a-guid", "someone@example.com")]
    [InlineData("d2a7d5c8-6a1f-4c55-9a4d-3d0e3f6e2b11", "someone@example.com")]
    public void IsOrderOwnedBy_WhenOrderBelongsToSomeoneElse_IsFalse(string? customerReference, string? orderEmail)
        => Assert.False(PortalCustomerService.IsOrderOwnedBy(customerReference, orderEmail, Guid.NewGuid(), ["ada@example.com"]));

    [Fact]
    public async Task MatchRegion_WhenStateIsANameOrCode_FindsTheRegion()
    {
        Country us = await _fixture.CreateCountryAsync("US", "United States");
        Region california = await _fixture.CreateRegionAsync(us.Id, "US-CA", "California");
        Region newYork = await _fixture.CreateRegionAsync(us.Id, "NY", "New York");
        RegionReadOnly[] regions = [california, newYork];

        Assert.Same(california, PortalCustomerService.MatchRegion(regions, " california "));
        Assert.Same(california, PortalCustomerService.MatchRegion(regions, "us-ca"));
        Assert.Same(california, PortalCustomerService.MatchRegion(regions, "CA"));
        Assert.Same(newYork, PortalCustomerService.MatchRegion(regions, "US-NY"));
        Assert.Null(PortalCustomerService.MatchRegion(regions, "Texas"));
        Assert.Null(PortalCustomerService.MatchRegion(regions, "  "));
        Assert.Null(PortalCustomerService.MatchRegion(regions, null));
    }

    private static IReadOnlyList<TestOrder> Merge(params TestOrder[][] sets)
        => PortalCustomerService.MergeOrders(sets, x => x.Id, x => x.FinalizedDate, x => x.FinalizedDate.HasValue);

    private async Task<Customer> LinkCustomerAsync(IMember member)
    {
        Customer customer = await _fixture.CreateCustomerAsync(member.Email!, "Ada", "Lovelace", member.Key, imported: true).ConfigureAwait(false);
        _fixture.CustomerService.Setup(x => x.GetCustomerByMemberKeyAsync(StoreId, member.Key)).ReturnsAsync(customer);
        return customer;
    }

    private sealed record TestOrder(Guid Id, DateTime? FinalizedDate);
}
