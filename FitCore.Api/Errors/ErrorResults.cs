using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Errors;

public static class ErrorResults
{
    public static ActionResult From(string error) => error switch
    {
        ErrorCodes.InvalidCredentials => new UnauthorizedObjectResult(Body(error)),
        ErrorCodes.MissingTenantContext => new UnauthorizedObjectResult(Body(error)),
        ErrorCodes.SignInLinkInvalidOrExpired => new UnauthorizedObjectResult(Body(error)),

        ErrorCodes.OrganizationNotFound
            or ErrorCodes.MemberNotFound
            or ErrorCodes.StaffNotFound
            => new NotFoundObjectResult(Body(error)),

        ErrorCodes.MemberEmailTaken
            or ErrorCodes.MemberPhoneTaken
            or ErrorCodes.StaffEmailTaken
            => new ConflictObjectResult(Body(error)),

        _ => new BadRequestObjectResult(Body(error)),
    };

    private static object Body(string error) => new { message = ToMessage(error) };

    private static string ToMessage(string error) => error switch
    {
        ErrorCodes.InvalidCredentials => "Invalid email or password",
        ErrorCodes.EmailAmbiguousOrg =>
            "This email belongs to more than one organization. Choose an organization to continue.",
        ErrorCodes.InvitationTokenRequired => "Invitation token is required.",
        ErrorCodes.InvitationInvalidOrExpired =>
            "This invitation link is invalid or has expired.",
        ErrorCodes.SignInLinkInvalidOrExpired =>
            "This sign-in link is invalid or has expired.",
        ErrorCodes.OrganizationNotFound => "Organization not found.",
        ErrorCodes.OrganizationNotActive => "This organization is not active.",
        ErrorCodes.OrganizationAlreadyCancelled => "This organization is already cancelled.",
        ErrorCodes.MemberNotFound => "Member not found.",
        ErrorCodes.MemberEmailTaken => "A member with this email already exists.",
        ErrorCodes.MemberPhoneTaken => "A member with this phone already exists.",
        ErrorCodes.MemberEmailRequired =>
            "This member needs an email before they can be invited.",
        ErrorCodes.MemberUnavailable => "This member account is no longer available.",
        ErrorCodes.StaffNotFound => "Staff member not found.",
        ErrorCodes.StaffEmailTaken => "A staff member with this email already exists.",
        ErrorCodes.StaffUnavailable => "This staff account is no longer available.",
        ErrorCodes.CannotDeleteSelf => "You cannot delete your own staff account.",
        ErrorCodes.MissingTenantContext => "Missing tenant context.",
        _ => error,
    };
}
