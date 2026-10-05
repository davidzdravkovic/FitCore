using FitCore.Api.Data.Stores.OrganizationOwner.MembersStore;
using FitCore.Api.Data.Stores.OrganizationOwner.MembershipsStore;
using FitCore.Api.Data.Stores.OrganizationOwner.VisitsStore;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Members;
using FitCore.Api.Features.Organizations.Admin.Memberships;
using FitCore.Api.Features.Organizations.Admin.Visits;

namespace FitCore.Api.Features.Organizations.Members.Me;

public class MemberMeService(
    IMemberStore memberStore,
    IMembershipStore membershipStore,
    IVisitStore visitStore)
{
    private const int DefaultWindowDays = 30;

    public async Task<Result<MemberProfileResponse>> GetAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var member = await memberStore.FindActiveByIdAsync(
            tenantId,
            memberId,
            cancellationToken);

        if (member is null)
            return Result<MemberProfileResponse>.Fail(ErrorCodes.MemberNotFound);

        var memberships = await membershipStore.ListByMemberAsync(
            tenantId,
            memberId,
            cancellationToken);

        return Result<MemberProfileResponse>.Success(
            new MemberProfileResponse(
                MemberService.ToResponse(member),
                memberships.Select(MembershipService.ToResponse).ToList()));
    }

    public async Task<IReadOnlyList<VisitResponse>> ListVisitsAsync(
        Guid tenantId,
        Guid memberId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var windowStart = from is null ? DateTime.UtcNow.Date : ToUtc(from.Value);
        var windowEnd = to is null
            ? windowStart.AddDays(DefaultWindowDays)
            : ToUtc(to.Value);

        if (windowEnd <= windowStart)
            windowEnd = windowStart.AddDays(DefaultWindowDays);

        var visits = await visitStore.ListByMemberAsync(
            tenantId,
            memberId,
            windowStart,
            windowEnd,
            cancellationToken);

        return visits.Select(VisitService.ToResponse).ToList();
    }

    private static DateTime ToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
}
