using FitCore.Api.Domain.Visits;
using FitCore.Api.Errors.Business;

namespace FitCore.Api.Domain.Memberships;

public static class MembershipTransitions
{
    public static Result TryReserveEntitlementForSchedule(Membership membership)
    {
        if (membership.SessionsAvailable <= 0)
            return Result.Fail(ErrorCodes.NoSessionCredit);

        membership.SessionsReserved++;
        return Result.Success();
    }

    /// <summary>
    /// Backfill past visit — burn one available credit without a prior reserve.
    /// </summary>
    public static Result TryBurnAvailableOnRecord(Membership membership)
    {
        if (membership.SessionsAvailable <= 0)
            return Result.Fail(ErrorCodes.NoSessionCredit);

        membership.SessionsBurned++;
        TryExpireIfPackExhausted(membership);
        return Result.Success();
    }

    public static bool TryRestorePackCreditOnVoid(Visit visit)
    {
        if (!visit.ConsumedSessionCredit)
            return false;

        if (visit.Membership.SessionsReserved <= 0)
            return false;

        visit.Membership.SessionsReserved--;
        visit.ConsumedSessionCredit = false;
        visit.Membership.Status = MembershipStatus.Active;
        return true;
    }

    /// <summary>Open visit cancelled with the membership — forfeit the held credit.</summary>
    public static void ForfeitReservedOnVisitCancel(Membership membership, Visit visit)
    {
        if (!visit.ConsumedSessionCredit || membership.SessionsReserved <= 0)
            return;

        membership.SessionsReserved--;
        membership.SessionsBurned++;
        TryExpireIfPackExhausted(membership);
    }

    /// <summary>
    /// Scheduled visit completed — move the held credit into burned.
    /// May expire the membership when no available or reserved credits remain.
    /// </summary>
    public static void BurnReservedOnComplete(Membership membership, Visit visit)
    {
        if (!visit.ConsumedSessionCredit || membership.SessionsReserved <= 0)
            return;

        membership.SessionsReserved--;
        membership.SessionsBurned++;
        TryExpireIfPackExhausted(membership);
    }

    public static bool TryExpireIfPackExhausted(Membership membership)
    {
        if (membership.Status != MembershipStatus.Active)
            return false;

        if (membership.SessionsAvailable > 0 || membership.SessionsReserved > 0)
            return false;

        membership.Status = MembershipStatus.Expired;
        return true;
    }

    public static void Cancel(
        Membership membership,
        MembershipCancelReason reason,
        string? note,
        Guid cancelledByStaffId,
        DateTime cancelledAtUtc)
    {
        membership.Status = MembershipStatus.Cancelled;
        membership.CancelReason = reason;
        membership.CancelNote = note;
        membership.CancelledAt = cancelledAtUtc;
        membership.CancelledByStaffId = cancelledByStaffId;
    }
}
