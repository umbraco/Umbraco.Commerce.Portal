using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Commerce.Portal.Models;
using Umbraco.Commerce.Portal.Web.Controllers;

namespace Umbraco.Commerce.Portal.Tests.Web.Controllers;

public class UmbracoCommercePortalAccountSurfaceControllerTests
{
    [Fact]
    public void Controller_WhenInspected_RequiresASignedInMember()
        => Assert.NotNull(typeof(UmbracoCommercePortalAccountSurfaceController).GetCustomAttribute<UmbracoMemberAuthorizeAttribute>());

    [Theory]
    [InlineData(nameof(UmbracoCommercePortalAccountSurfaceController.UpdateAccount))]
    [InlineData(nameof(UmbracoCommercePortalAccountSurfaceController.SaveAddress))]
    [InlineData(nameof(UmbracoCommercePortalAccountSurfaceController.SetDefaultAddress))]
    [InlineData(nameof(UmbracoCommercePortalAccountSurfaceController.DeleteAddress))]
    [InlineData(nameof(UmbracoCommercePortalAccountSurfaceController.ChangePassword))]
    public void Action_WhenInspected_IsAnAntiForgeryPost(string actionName)
    {
        MethodInfo action = typeof(UmbracoCommercePortalAccountSurfaceController).GetMethod(actionName)!;

        Assert.NotNull(action.GetCustomAttribute<HttpPostAttribute>());
        Assert.NotNull(action.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
    }

    [Theory]
    [InlineData(typeof(AccountModel))]
    [InlineData(typeof(ChangePasswordModel))]
    [InlineData(typeof(AddressBookAddressModel))]
    public void FormModel_WhenInspected_HasNoUsableMemberOrCustomerId(Type model)
    {
        var ids = model.GetProperties()
            .Where(x => x.Name.Contains("Member", StringComparison.Ordinal) || x.Name.Contains("Customer", StringComparison.Ordinal) || x.Name == "Id")
            .Where(x => x.GetCustomAttribute<ObsoleteAttribute>() is null)
            .Where(x => x.PropertyType == typeof(Guid))
            .Select(x => x.Name);

        Assert.Empty(ids);
    }
}
