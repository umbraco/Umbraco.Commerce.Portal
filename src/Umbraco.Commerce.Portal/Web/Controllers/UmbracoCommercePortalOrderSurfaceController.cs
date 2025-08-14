using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.Portal.Models;

namespace Umbraco.Commerce.Portal.Web.Controllers;

public class UmbracoCommercePortalOrderSurfaceController : SurfaceController
{
    private readonly IUmbracoCommerceApi _commerceApi;

    public UmbracoCommercePortalOrderSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IUmbracoCommerceApi commerceApi)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _commerceApi = commerceApi;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCustomer(CustomerModel customerModel)
    {
        ArgumentNullException.ThrowIfNull(customerModel);

        if (!ModelState.IsValid)
        {
            TempData["UpdateCustomerError"] = true;
            return CurrentUmbracoPage();
        }

        Request.Query.TryGetValue("orderId", out var orderId);
        await _commerceApi.Uow.ExecuteAsync(async uow =>
        {
            var order = await _commerceApi.GetOrderAsync(Guid.Parse(orderId))
                .AsWritableAsync(uow)
                .SetPropertiesAsync(new Dictionary<string, string>
                {
                    { "firstName", customerModel.FirstName },
                    { "lastName", customerModel.LastName },
                    { "email", customerModel.Email },
                    { "billingTelephone", customerModel.Telephone },
                });

            await _commerceApi.SaveOrderAsync(order);

            uow.Complete();
        });

        TempData["UpdateCustomerSuccess"] = "Customer details updated";

        return CurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBillingAddress(AddressModel? billingAddressModel)
    {
        ArgumentNullException.ThrowIfNull(billingAddressModel);

        if (!ModelState.IsValid)
        {
            TempData["UpdateAddressError"] = true;
            return CurrentUmbracoPage();
        }

        Request.Query.TryGetValue("orderId", out var orderId);
        await _commerceApi.Uow.ExecuteAsync(async uow =>
        {
            var order = await _commerceApi.GetOrderAsync(Guid.Parse(orderId))
                .AsWritableAsync(uow)
                .SetPropertiesAsync(new Dictionary<string, string>
                {
                    { "billingAddressLine1", billingAddressModel.AddressLine1 },
                    { "billingAddressLine2", billingAddressModel.AddressLine2 },
                    { "billingAddressCity", billingAddressModel.City },
                    { "billingAddressZipCode", billingAddressModel.ZipCode },
                });

            await _commerceApi.SaveOrderAsync(order);

            uow.Complete();
        });

        TempData["UpdateAddressSuccess"] = "Customer address updated";

        return CurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateShippingAddress(AddressModel? shippingAddressModel)
    {
        ArgumentNullException.ThrowIfNull(shippingAddressModel);

        if (!ModelState.IsValid)
        {
            TempData["UpdateAddressError"] = true;
            return CurrentUmbracoPage();
        }

        Request.Query.TryGetValue("orderId", out var orderId);
        await _commerceApi.Uow.ExecuteAsync(async uow =>
        {
            var order = await _commerceApi.GetOrderAsync(Guid.Parse(orderId))
                .AsWritableAsync(uow)
                .SetPropertiesAsync(new Dictionary<string, string>
                {
                    { "shippingAddressLine1", shippingAddressModel.AddressLine1 },
                    { "shippingAddressLine2", shippingAddressModel.AddressLine2 },
                    { "shippingAddressCity", shippingAddressModel.City },
                    { "shippingAddressZipCode", shippingAddressModel.ZipCode },
                });

            await _commerceApi.SaveOrderAsync(order);

            uow.Complete();
        });

        TempData["UpdateAddressSuccess"] = "Customer address updated";

        return CurrentUmbracoPage();
    }
}
