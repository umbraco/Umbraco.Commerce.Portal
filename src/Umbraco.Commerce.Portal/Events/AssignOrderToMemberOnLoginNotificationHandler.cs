using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Common.Events;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Extensions;

namespace Umbraco.Commerce.Portal.Events;

public class AssignOrderToMemberOnLoginNotificationHandler : NotificationEventHandlerBase<OnLoginNotification>
{
    private readonly IUmbracoCommerceApi _commerceApi;

    public AssignOrderToMemberOnLoginNotificationHandler(IUmbracoCommerceApi commerceApi)
    {
        _commerceApi = commerceApi;
    }

    public override async Task HandleAsync(OnLoginNotification evt, CancellationToken cancellationToken)
    {
        await _commerceApi.Uow.ExecuteAsync(async uow =>
        {
            var currentOrder = await _commerceApi.GetCurrentOrderAsync(evt.StoreId);
            if (currentOrder is not null)
            {
                var order = await currentOrder
                    .AsWritableAsync(uow)
                    .AssignToCustomerAsync(evt.Member.Key.ToString());

                await _commerceApi.SaveOrderAsync(order);
            }
        });
    }
}
