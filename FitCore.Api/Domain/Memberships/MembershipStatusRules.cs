namespace FitCore.Api.Domain.Memberships;

public static class MembershipStatusRules
{
    public static readonly MembershipStatus[] Unresolved =
    [
        MembershipStatus.Active,
        MembershipStatus.Frozen,
    ];

    public static readonly MembershipStatus[] Schedulable =
    [
        MembershipStatus.Active,
    ];

    public static readonly MembershipStatus[] Cancellable =
    [
        MembershipStatus.Active,
        MembershipStatus.Frozen,
    ];
}
