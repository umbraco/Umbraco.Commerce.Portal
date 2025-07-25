using System;
using System.Collections.Generic;

namespace Umbraco.Commerce.Portal;

public static class UmbracoCommercePortalConstants
{
    public const string UmbracoCommercePortalViewPath = "~/Views/UmbracoCommercePortal";

    public static class EmailTemplates
    {
        public static KeyValuePair<string, string> ConfirmMemberEmailTemplate => new(
            "UmbracoCommercePortal-ConfirmMember",
            "~/Views/Templates/Email/UmbracoCommercePortalConfirmMemberEmail.cshtml");

        public static KeyValuePair<string, string> ResetPasswordEmailTemplate => new(
            "UmbracoCommercePortal-ResetPassword",
            "~/Views/Templates/Email/UmbracoCommercePortalResetPasswordEmail.cshtml");
    }

    public static class ContentTypes
    {
        public static class Aliases
        {
            public const string PortalMemberType = "ucpMember";

            public const string PortalContainerPage = "ucpPortalContainerPage";

            public const string PortalAuthPage = "ucpPortalAuthPage";

            public const string PortalManagementPage = "ucpPortalManagementPage";

            public const string PortalMyAccountPage = "ucpPortalMyAccountPage";

            public const string PortalOrderHistoryPage = "ucpPortalOrderHistoryPage";
        }

        public static class MemberTypeAliases
        {
            public const string FirstName = "ucpMemberFirstName";

            public const string LastName = "ucpMemberLastName";

            public const string Country = "ucpMemberCountry";

            public const string Street = "ucpMemberStreet";

            public const string City = "ucpMemberCity";

            public const string State = "ucpMemberState";

            public const string ZipCode = "ucpZipCode";
        }

        public static class Guids
        {
            private const string BasePage = "bfba902f-62b5-4ab7-bb19-4e5e34ae9780";
            public static readonly Guid BasePageGuid = new(BasePage);

            private const string PortalContainerPage = "c9b10102-2793-448d-bf12-04c1d305907b";
            public static readonly Guid PortalContainerPageGuid = new(PortalContainerPage);

            private const string PortalAuthPage = "701bd4fb-892e-47ff-9007-d60fa966cef0";
            public static readonly Guid PortalAuthPageGuid = new(PortalAuthPage);

            private const string PortalManagementPage = "1587fb8f-ec84-4d2b-a3b5-8842cb05506f";
            public static readonly Guid PortalManagementPageGuid = new(PortalManagementPage);

            private const string PortalMyAccountPage = "648aa4a5-3404-4076-90dc-d6ec2d7658e6";
            public static readonly Guid PortalMyAccountPageGuid = new(PortalMyAccountPage);

            private const string PortalOrderHistoryPage = "b5eb4942-cb4d-458e-bffd-91d1175af4d9";
            public static readonly Guid PortalOrderHistoryPageGuid = new(PortalOrderHistoryPage);
        }

        public static class Properties
        {
            public const string HomePageAlias = "ucpHomePage";

            public const string TermsAndConditionsPageAlias = "ucpTermsAndConditionsPage";

            public const string PrivacyPolicyPageAlias = "ucpPrivacyPolicyPage";
        }
    }
}
