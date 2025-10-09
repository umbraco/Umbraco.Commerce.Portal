using System;

namespace Umbraco.Commerce.Portal;

public static class UmbracoCommercePortalConstants
{
    public const string UmbracoCommercePortalViewPath = "~/Views/UmbracoCommercePortal";

    public static class EmailTemplates
    {
        public record EmailTemplate(string Key, string Name, string TemplateView);

        public static EmailTemplate ConfirmMemberEmailTemplate => new(
            "UmbracoCommercePortal-ConfirmMember",
            "Umbraco Commerce Portal - Confirm Member",
            "~/Views/Templates/Email/UmbracoCommercePortalConfirmMemberEmail.cshtml");

        public static EmailTemplate ResetPasswordEmailTemplate => new(
            "UmbracoCommercePortal-ResetPassword",
            "Umbraco Commerce Portal - Reset Password",
            "~/Views/Templates/Email/UmbracoCommercePortalResetPasswordEmail.cshtml");
    }

    public static class  DataTypes
    {
        public const string CountryDataTypeName = "[Umbraco Commerce Portal] Country Dropdown";
    }

    public static class ContentTypes
    {
        public static class Aliases
        {
            public const string PortalMemberType = "ucpMember";

            public const string PortalMemberGroup = "Commerce Portal";

            public const string PortalContainerPage = "ucpPortalContainerPage";

            public const string PortalAuthPage = "ucpPortalAuthPage";

            public const string PortalManagementPage = "ucpPortalManagementPage";

            public const string PortalMyAccountPage = "ucpPortalMyAccountPage";

            public const string PortalOrderHistoryPage = "ucpPortalOrderHistoryPage";

            public const string PortalOrderDetailsPage = "ucpPortalOrderDetailsPage";
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

            private const string PortalOrderDetailsPage = "85ef6c57-11d6-44d4-8e80-87f9e917a41d";
            public static readonly Guid PortalOrderDetailsPageGuid = new(PortalOrderDetailsPage);
        }

        public static class Properties
        {
            public const string HomePageAlias = "ucpHomePage";

            public const string TermsAndConditionsPageAlias = "ucpTermsAndConditionsPage";

            public const string PrivacyPolicyPageAlias = "ucpPrivacyPolicyPage";
        }
    }

    public static class ContentNodes
    {
        public const string PortalAuthLoginPageNodeName = "Login";

        public const string PortalAuthRegisterPageNodeName = "Register";

        public const string PortalAuthResetPasswordPageNodeName = "Reset Password";
    }

    public static class Localization
    {
        public record LocalizationEntry(string Key, string DefaultValue);

        public static class AuthEntries
        {
            public static LocalizationEntry EmailAddress => new("UmbracoCommercePortal.Auth.EmailAddress", "Email address");

            public static LocalizationEntry HasAccount => new("UmbracoCommercePortal.Auth.HasAccount", "Already have an account?");

            public static LocalizationEntry LogIn => new("UmbracoCommercePortal.Auth.LogIn", "Log in");

            public static LocalizationEntry SignIn => new("UmbracoCommercePortal.Auth.SignIn", "Sign in");

            public static LocalizationEntry AccountSignIn => new("UmbracoCommercePortal.Auth.AccountSignIn", "Sign in to your account");

            public static LocalizationEntry SignUp => new("UmbracoCommercePortal.Auth.SignUp", "Sign up");

            public static LocalizationEntry SignOut => new("UmbracoCommercePortal.Auth.SignOut", "Sign out");

            public static LocalizationEntry Password => new("UmbracoCommercePortal.Auth.Password", "Password");

            public static LocalizationEntry ForgotPassword => new("UmbracoCommercePortal.Auth.ForgotPassword", "Forgot Password?");

            public static LocalizationEntry ConfirmPassword => new("UmbracoCommercePortal.Auth.ConfirmPassword", "Confirm password");

            public static LocalizationEntry ResetPassword => new("UmbracoCommercePortal.Auth.ResetPassword", "Reset your password");

            public static LocalizationEntry EnterNewPassword => new("UmbracoCommercePortal.Auth.EnterNewPassword", "Enter your new password");

            public static LocalizationEntry UpdatePassword => new("UmbracoCommercePortal.Auth.UpdatePassword", "Update password");

            public static LocalizationEntry EnterEmailForResetPassword => new("UmbracoCommercePortal.Auth.EnterEmailForResetPassword", "Enter your email address and we’ll send you a link to reset your password");

            public static LocalizationEntry ConfirmAccount => new("UmbracoCommercePortal.Auth.ConfirmAccount", "Confirm account");

            public static LocalizationEntry ConfirmYourAccount => new("UmbracoCommercePortal.Auth.ConfirmYourAccount", "Confirm your account");

            public static LocalizationEntry CreateAccount => new("UmbracoCommercePortal.Auth.CreateAccount", "Create account");

            public static LocalizationEntry CreateAnAccount => new("UmbracoCommercePortal.Auth.CreateAnAccount", "Create an account");

            public static LocalizationEntry PleaseConfirmAccount => new("UmbracoCommercePortal.Auth.PleaseConfirmAccount", "Please confirm your account by clicking the button below");

            public static LocalizationEntry FirstName => new("UmbracoCommercePortal.Auth.FirstName", "First Name");

            public static LocalizationEntry LastName => new("UmbracoCommercePortal.Auth.LastName", "Last Name");

            public static LocalizationEntry NotAMember => new("UmbracoCommercePortal.Auth.NotMember", "Not a member ?");

            public static LocalizationEntry MemberAccountUpdated => new("UmbracoCommercePortal.Auth.MemberAccountUpdated", "Member account updated.");

            public static LocalizationEntry MemberBillingInformationUpdated => new("UmbracoCommercePortal.Auth.MemberBillingInformationUpdated", "Member billing information updated.");

            public static LocalizationEntry ResetPasswordEmailSent => new("UmbracoCommercePortal.Auth.ResetPasswordEmailSent", "A reset password email has been sent to your email address.");

            public static LocalizationEntry FailedToSendConfirmationEmail => new("UmbracoCommercePortal.Auth.FailedToSendConfirmationEmail", "Failed to send confirmation email.");

            public static LocalizationEntry InvalidRequestData => new("UmbracoCommercePortal.Auth.InvalidRequestData", "Invalid request data.");

            public static LocalizationEntry MemberNotFound => new("UmbracoCommercePortal.Auth.MemberNotFound", "Member not found.");

            public static LocalizationEntry PasswordUpdated => new("UmbracoCommercePortal.Auth.PasswordUpdated", "Password updated.");

            public static LocalizationEntry MemberAccountCreated => new("UmbracoCommercePortal.Auth.MemberAccountCreated", "Member account created. Please confirm it from the received email.");

            public static LocalizationEntry MemberConfirmed => new("UmbracoCommercePortal.Auth.MemberConfirmed", "Member confirmed.");

            public static LocalizationEntry MemberLockedOut => new("UmbracoCommercePortal.Auth.MemberLockedOut", "Member is locked out");

            public static LocalizationEntry MemberNotAllowed => new("UmbracoCommercePortal.Auth.MemberNotAllowed", "Member is not allowed");

            public static LocalizationEntry InvalidUsernameOrPassword => new("UmbracoCommercePortal.Auth.InvalidUsernameOrPassword", "Invalid username or password");

            public static LocalizationEntry MemberCurrentPasswordIncorrect => new("UmbracoCommercePortal.Auth.MemberCurrentPasswordIncorrect", "The current password is incorrect");

            public static LocalizationEntry MemberPasswordChanged => new("UmbracoCommercePortal.Auth.MemberPasswordChanged", "Password changed");
        }

        public static class OrderInfoEntries
        {
            public static LocalizationEntry OrderInfo => new("UmbracoCommercePortal.OrderInfo", "Order Info");

            public static LocalizationEntry OrderNumber => new("UmbracoCommercePortal.OrderInfo.OrderNumber", "Order Number");

            public static LocalizationEntry OrderDate => new("UmbracoCommercePortal.OrderInfo.OrderDate", "Order Date");

            public static LocalizationEntry Status => new("UmbracoCommercePortal.OrderInfo.Status", "Status");

            public static LocalizationEntry PaymentStatus => new("UmbracoCommercePortal.OrderInfo.PaymentStatus", "Payment Status");

            public static LocalizationEntry PaymentMethod => new("UmbracoCommercePortal.OrderInfo.PaymentMethod", "Payment Method");

            public static LocalizationEntry YourOrders => new("UmbracoCommercePortal.OrderInfo.YourOrders", "Your Orders");

            public static LocalizationEntry OrderHistory => new("UmbracoCommercePortal.OrderInfo.OrderHistory", "Order History");

            public static LocalizationEntry ViewOrder => new("UmbracoCommercePortal.OrderInfo.ViewOrder", "View Order");

            public static LocalizationEntry ViewAndTrackPastOrders => new("UmbracoCommercePortal.OrderInfo.ViewAndTrackPastOrders", "View and track your past orders");

            public static LocalizationEntry Subtotal => new("UmbracoCommercePortal.OrderInfo.Subtotal", "Subtotal");

            public static LocalizationEntry SubtotalFees => new("UmbracoCommercePortal.OrderInfo.SubtotalFees", "Subtotal Fees");

            public static LocalizationEntry Taxes => new("UmbracoCommercePortal.OrderInfo.Taxes", "Taxes");

            public static LocalizationEntry SubtotalDiscount => new("UmbracoCommercePortal.OrderInfo.SubtotalDiscount", "Subtotal Discount");

            public static LocalizationEntry NetTotalFees => new("UmbracoCommercePortal.OrderInfo.NetTotalFees", "Net Total Fees");

            public static LocalizationEntry NetTotalDiscount => new("UmbracoCommercePortal.OrderInfo.NetTotalDiscount", "Net Total Discount");

            public static LocalizationEntry Total => new("UmbracoCommercePortal.OrderInfo.Total", "Total");

            public static LocalizationEntry TotalAmount => new("UmbracoCommercePortal.OrderInfo.TotalAmount", "Total Amount");

            public static LocalizationEntry Quantity => new("UmbracoCommercePortal.OrderInfo.Quantity", "Quantity");

            public static LocalizationEntry YourOrderDetails => new("UmbracoCommercePortal.OrderInfo.YourOrderDetails", "Your Order Details");

            public static LocalizationEntry ThankYou => new("UmbracoCommercePortal.OrderInfo.ThankYou", "Thank you for shopping with us!");

            public static LocalizationEntry BackToOrders => new("UmbracoCommercePortal.OrderInfo.BackToOrders", "Back to Orders");
        }

        public static class CustomerEntries
        {
            public static LocalizationEntry Customer => new("UmbracoCommercePortal.Customer", "Customer");

            public static LocalizationEntry Name => new("UmbracoCommercePortal.Customer.Name", "Name");

            public static LocalizationEntry Email => new("UmbracoCommercePortal.Customer.Email", "Email");

            public static LocalizationEntry PhoneNumber => new("UmbracoCommercePortal.Customer.PhoneNumber", "Phone number");

            public static LocalizationEntry Address => new("UmbracoCommercePortal.Customer.Address", "Address");

            public static LocalizationEntry BillingAddress => new("UmbracoCommercePortal.Customer.BillingAddress", "Billing Address");

            public static LocalizationEntry ShippingAddress => new("UmbracoCommercePortal.Customer.ShippingAddress", "Shipping Address");
        }

        public static class ProductEntries
        {
            public static LocalizationEntry Quantity => new("UmbracoCommercePortal.Product.Quantity", "Quantity");
        }

        public static class AccountEntries
        {
            public static LocalizationEntry PersonalInformation => new("UmbracoCommercePortal.Account.PersonalInformation", "Personal Information");

            public static LocalizationEntry MyAccount => new("UmbracoCommercePortal.Account.MyAccount", "My Account");

            public static LocalizationEntry ManageAccountDetails => new("UmbracoCommercePortal.Account.ManageAccountDetails", "Manage your account details and preferences");

            public static LocalizationEntry FirstName => new("UmbracoCommercePortal.Account.FirstName", "First name");

            public static LocalizationEntry LastName => new("UmbracoCommercePortal.Account.LastName", "Last name");

            public static LocalizationEntry EmailAddress => new("UmbracoCommercePortal.Account.EmailAddress", "Email address");

            public static LocalizationEntry BillingInformation => new("UmbracoCommercePortal.Account.BillingInformation", "Billing Information");

            public static LocalizationEntry ShippingInformation => new("UmbracoCommercePortal.Account.ShippingInformation", "Shipping Information");

            public static LocalizationEntry Country => new("UmbracoCommercePortal.Account.Country", "Country");

            public static LocalizationEntry StreetAddress => new("UmbracoCommercePortal.Account.StreetAddress", "Street Address");

            public static LocalizationEntry City => new("UmbracoCommercePortal.Account.City", "City");

            public static LocalizationEntry State => new("UmbracoCommercePortal.Account.State", "State / Province");

            public static LocalizationEntry ZipCode => new("UmbracoCommercePortal.Account.ZipCode", "ZIP / Postal code");

            public static LocalizationEntry ChangePassword => new("UmbracoCommercePortal.Account.ChangePassword", "Change Password");

            public static LocalizationEntry CurrentPassword => new("UmbracoCommercePortal.Account.CurrentPassword", "Current Password");

            public static LocalizationEntry NewPassword => new("UmbracoCommercePortal.Account.NewPassword", "New Password");
        }

        public static class ManagementEntries
        {
            public static LocalizationEntry MyAccount => new("UmbracoCommercePortal.ManagementNavigation.MyAccount", "My Account");

            public static LocalizationEntry OrderHistory => new("UmbracoCommercePortal.ManagementNavigation.OrderHistory", "Order History");
        }

        public static class FooterEntries
        {
            public static LocalizationEntry TermsOfService => new("UmbracoCommercePortal.FooterLinks.TermsOfService", "Terms of Service");

            public static LocalizationEntry PrivacyPolicy => new("UmbracoCommercePortal.FooterLinks.PrivacyPolicy", "Privacy Policy");
        }
    }
}
