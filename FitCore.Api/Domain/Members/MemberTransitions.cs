namespace FitCore.Api.Domain.Members;

public static class MemberTransitions
{
    public static void ActivateOnAssign(Member member)
    {
        if (MemberStatusRules.PromoteToActiveOnAssign.Contains(member.Status))
            member.Status = MemberStatus.Active;
    }

    public static void PauseIfNoOtherActive(Member member, int otherActiveMembershipCount)
    {
        if (otherActiveMembershipCount == 0 && member.Status == MemberStatus.Active)
            member.Status = MemberStatus.Paused;
    }

    public static void ActivateIfPaused(Member member)
    {
        if (member.Status == MemberStatus.Paused)
            member.Status = MemberStatus.Active;
    }

    public static void Cancel(Member member)
    {
        member.Status = MemberStatus.Cancelled;
    }
}
