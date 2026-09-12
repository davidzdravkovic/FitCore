namespace FitCore.Api.Errors;

public static class ErrorCodes
{
    public const string InvalidCredentials = "invalid-credentials";
    public const string EmailAmbiguousOrg = "email-ambiguous-org";
    public const string MissingTenantContext = "missing-tenant-context";

    public const string InvitationTokenRequired = "invitation-token-required";
    public const string InvitationInvalidOrExpired = "invitation-invalid-or-expired";

    public const string OrganizationNotFound = "organization-not-found";
    public const string OrganizationNotActive = "organization-not-active";
    public const string OrganizationAlreadyCancelled = "organization-already-cancelled";

    public const string MemberNotFound = "member-not-found";
    public const string MemberEmailTaken = "member-email-taken";
    public const string MemberPhoneTaken = "member-phone-taken";
    public const string MemberEmailRequired = "member-email-required";
    public const string MemberUnavailable = "member-unavailable";

    public const string StaffNotFound = "staff-not-found";
    public const string StaffEmailTaken = "staff-email-taken";
    public const string StaffUnavailable = "staff-unavailable";
    public const string CannotDeleteSelf = "cannot-delete-self";

    public const string SignInLinkInvalidOrExpired = "sign-in-link-invalid-or-expired";
}
