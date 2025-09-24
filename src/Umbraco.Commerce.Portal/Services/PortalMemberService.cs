using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Portal.Extensions;
using Umbraco.Commerce.Portal.Models;

namespace Umbraco.Commerce.Portal.Services;

public class PortalMemberService(IMemberService memberService)
{
    public AccountModel Get(string email)
    {
        var member = memberService.GetByEmail(email);
        return member.ToModel();
    }

    public BillingInformationModel GetBillingInformation(string email)
    {
        var member = memberService.GetByEmail(email);
        return member.ToBillingInformationModel();
    }
}
