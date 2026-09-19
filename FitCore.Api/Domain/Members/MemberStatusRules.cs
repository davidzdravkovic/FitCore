namespace FitCore.Api.Domain.Members;

public static class MemberStatusRules
{
    public static readonly MemberStatus[] CanAssign =
    [
        MemberStatus.Lead,
        MemberStatus.Paused,
        MemberStatus.Active,
    ];

    public static readonly MemberStatus[] PromoteToActiveOnAssign =
    [
        MemberStatus.Lead,
        MemberStatus.Paused,
    ];

    public static readonly MemberStatus[] Cancellable =
    [
        MemberStatus.Paused,
        MemberStatus.Lead,
    ];

    public static readonly MemberStatus[] OnRoster =
    [
        MemberStatus.Lead,
        MemberStatus.Trial,
        MemberStatus.Active,
        MemberStatus.Paused,
    ];

    public static bool IsOnRoster(MemberStatus status) =>
        OnRoster.Contains(status);
}
