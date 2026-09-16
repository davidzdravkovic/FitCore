using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Errors.Business;

public static class ErrorResults
{
    public static ActionResult From(string error, object? details = null) => error switch
    {
        ErrorCodes.InvalidCredentials => new UnauthorizedObjectResult(Body(error)),
        ErrorCodes.MissingTenantContext => new UnauthorizedObjectResult(Body(error)),
        ErrorCodes.SignInLinkInvalidOrExpired => new UnauthorizedObjectResult(Body(error)),

        ErrorCodes.OrganizationNotFound
            or ErrorCodes.MemberNotFound
            or ErrorCodes.StaffNotFound
            or ErrorCodes.ServiceNotFound
            or ErrorCodes.PlanNotFound
            or ErrorCodes.MembershipNotFound
            or ErrorCodes.VisitNotFound
            => new NotFoundObjectResult(Body(error)),

        ErrorCodes.MemberEmailTaken
            or ErrorCodes.MemberPhoneTaken
            or ErrorCodes.StaffEmailTaken
            or ErrorCodes.ServiceNameTaken
            or ErrorCodes.PlanNameTaken
            or ErrorCodes.MemberHasUnresolvedMemberships
            or ErrorCodes.VisitAlreadyScheduled
            or ErrorCodes.MemberVisitConflict
            or ErrorCodes.CoachUnavailable
            => new ConflictObjectResult(Body(error, details)),

        _ => new BadRequestObjectResult(Body(error, details)),
    };

    private static object Body(string error, object? details = null)
    {
        if (details is null)
            return new { message = ToMessage(error) };

        return new { message = ToMessage(error), memberships = details };
    }

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
        ErrorCodes.MemberHasUnresolvedMemberships =>
            "This member still has active or frozen memberships. Resolve those before cancelling the member.",
        ErrorCodes.StaffNotFound => "Staff member not found.",
        ErrorCodes.StaffEmailTaken => "A staff member with this email already exists.",
        ErrorCodes.StaffUnavailable => "This staff account is no longer available.",
        ErrorCodes.CannotDeleteSelf => "You cannot delete your own staff account.",
        ErrorCodes.ServiceNotFound => "Service not found.",
        ErrorCodes.ServiceNameTaken => "A service with this name already exists.",
        ErrorCodes.PlanNotFound => "Plan not found.",
        ErrorCodes.PlanNameTaken => "A plan with this name already exists.",
        ErrorCodes.MembershipNotFound => "Membership not found.",
        ErrorCodes.MembershipNotCancellable =>
            "Only active or frozen memberships can be cancelled.",
        ErrorCodes.MembershipNotSchedulable =>
            "Only active memberships can be scheduled.",
        ErrorCodes.VisitNotFound => "Visit not found.",
        ErrorCodes.VisitNotVoidable =>
            "Only scheduled visits can be voided as a scheduling mistake.",
        ErrorCodes.VisitAlreadyScheduled =>
            "A scheduled visit already exists for this membership, coach, and time.",
        ErrorCodes.MemberVisitConflict =>
            "This member already has a scheduled visit that overlaps this time.",
        ErrorCodes.CoachUnavailable =>
            "This coach already has a scheduled visit that overlaps this time.",
        ErrorCodes.NoSessionCredit =>
            "This membership has no remaining session credits.",
        ErrorCodes.InvalidVisitInterval => "EndAt must be after StartAt.",
        ErrorCodes.MembershipOutsideWindow =>
            "Visit time is outside this membership's access window.",
        ErrorCodes.MissingTenantContext => "Missing tenant context.",
        _ => error,
    };
}
