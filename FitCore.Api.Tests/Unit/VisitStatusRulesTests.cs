using FitCore.Api.Domain.Visits;

namespace FitCore.Api.Tests.Unit;

public class VisitStatusRulesTests
{
    [Fact]
    public void Open_contains_only_Scheduled()
    {
        Assert.Equal([VisitStatus.Scheduled], VisitStatusRules.Open);
    }

    [Fact]
    public void OccupiesSlot_includes_scheduled_and_burn_outcomes()
    {
        Assert.Equal(
            [VisitStatus.Scheduled, VisitStatus.Completed, VisitStatus.NoShow],
            VisitStatusRules.OccupiesSlot);
    }

    [Theory]
    [InlineData(VisitStatus.Cancelled)]
    [InlineData(VisitStatus.Voided)]
    [InlineData(VisitStatus.Postponed)]
    public void OccupiesSlot_excludes_non_occupying_outcomes(VisitStatus status)
    {
        Assert.DoesNotContain(status, VisitStatusRules.OccupiesSlot);
    }

    [Fact]
    public void Voidable_contains_only_Scheduled()
    {
        Assert.Equal([VisitStatus.Scheduled], VisitStatusRules.Voidable);
    }

    [Theory]
    [InlineData(VisitStatus.Completed)]
    [InlineData(VisitStatus.NoShow)]
    [InlineData(VisitStatus.Cancelled)]
    public void CreditConsumed_includes_burn_outcomes(VisitStatus status)
    {
        Assert.Contains(status, VisitStatusRules.CreditConsumed);
    }

    [Fact]
    public void Resolvable_contains_only_Scheduled()
    {
        Assert.Equal([VisitStatus.Scheduled], VisitStatusRules.Resolvable);
    }

    [Theory]
    [InlineData(VisitStatus.Postponed)]
    [InlineData(VisitStatus.Voided)]
    public void CreditRestored_includes_restore_outcomes(VisitStatus status)
    {
        Assert.Contains(status, VisitStatusRules.CreditRestored);
    }

    [Fact]
    public void Voided_is_not_open_or_voidable()
    {
        Assert.DoesNotContain(VisitStatus.Voided, VisitStatusRules.Open);
        Assert.DoesNotContain(VisitStatus.Voided, VisitStatusRules.Voidable);
    }
}
