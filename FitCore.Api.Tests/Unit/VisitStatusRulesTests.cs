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
    public void Voidable_contains_only_Scheduled()
    {
        Assert.Equal([VisitStatus.Scheduled], VisitStatusRules.Voidable);
    }

    [Theory]
    [InlineData(VisitStatus.Completed)]
    [InlineData(VisitStatus.Cancelled)]
    public void CreditConsumed_includes_burn_outcomes(VisitStatus status)
    {
        Assert.Contains(status, VisitStatusRules.CreditConsumed);
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
