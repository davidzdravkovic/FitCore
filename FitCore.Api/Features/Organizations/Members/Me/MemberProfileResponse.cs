using FitCore.Api.Features.Organizations.Admin.Members.Create;
using FitCore.Api.Features.Organizations.Admin.Memberships.Assign;

namespace FitCore.Api.Features.Organizations.Members.Me;

public record MemberProfileResponse(
    MemberResponse Member,
    IReadOnlyList<MembershipResponse> Memberships);
