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
            public const string PortalContainerPage = "ucpPortalContainerPage";

            public const string PortalAuthPage = "ucpPortalAuthPage";
        }

        public static class Guids
        {
            private const string BasePage = "bfba902f-62b5-4ab7-bb19-4e5e34ae9780";
            public static readonly Guid BasePageGuid = new(BasePage);

            private const string PortalContainerPage = "c9b10102-2793-448d-bf12-04c1d305907b";
            public static readonly Guid PortalContainerPageGuid = new(PortalContainerPage);

            private const string PortalAuthPage = "701bd4fb-892e-47ff-9007-d60fa966cef0";
            public static readonly Guid PortalAuthPageGuid = new(PortalAuthPage);
        }

        public static class Properties
        {
            public const string HomePageAlias = "ucpHomePage";

            public const string TermsAndConditionsPageAlias = "ucpTermsAndConditionsPage";

            public const string PrivacyPolicyPageAlias = "ucpPrivacyPolicyPage";
        }
    }
}
