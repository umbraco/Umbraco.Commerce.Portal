using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Events;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Extensions;

namespace Umbraco.Commerce.Portal.Events;

public class AssignOrderToMemberOnLoginNotificationHandler : INotificationAsyncHandler<OnLoginNotification>
{
    private readonly IUmbracoCommerceApi _commerceApi;

    public AssignOrderToMemberOnLoginNotificationHandler(IUmbracoCommerceApi commerceApi)
    {
        _commerceApi = commerceApi;
    }

    public async Task HandleAsync(OnLoginNotification notification, CancellationToken cancellationToken)
    {
        await _commerceApi.Uow.ExecuteAsync(async uow =>
        {
            var currentOrder = await _commerceApi.GetCurrentOrderAsync(notification.StoreId);
            if (currentOrder is not null)
            {
                var order = await currentOrder
                    .AsWritableAsync(uow)
                    .AssignToCustomerAsync(notification.LoggedInMember.Email);

                await _commerceApi.SaveOrderAsync(order);
            }
        });
    }

}
