using System;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Portal.Extensions;
using Umbraco.Commerce.Portal.Models;

namespace Umbraco.Commerce.Portal.Services;

public class PortalMemberService(IMemberService memberService)
{
    [Obsolete("Portal reads profiles from the Umbraco Commerce customer now. Use PortalCustomerService.GetCurrentAsync instead. Will be removed in v21.0.0")]
    public AccountModel Get(string email)
    {
        var member = memberService.GetByEmail(email);
        return member.ToAccountModel();
    }

    [Obsolete("Billing details live in the customer's address book now. Use PortalCustomerService.GetAddressBookAsync instead. Will be removed in v21.0.0")]
    public BillingInformationModel GetBillingInformation(string email)
    {
        var member = memberService.GetByEmail(email);
        return member.ToBillingInformationModel();
    }
}
