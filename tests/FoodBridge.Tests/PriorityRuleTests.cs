using FoodBridge.Application.Allocation;
using FoodBridge.Domain.Enums;

namespace FoodBridge.Tests;

public class PriorityRuleTests
{
    private readonly IPriorityRule[] _rules =
    [
        new NgoSizeRule(),
        new RecentReceiptsRule(),
        new ReliabilityRule()
    ];

    [Fact]
    public void NgoSize_CapsAt50()
    {
        Assert.Equal(50, new NgoSizeRule().Score(Facts(peopleServed: 80)));
        Assert.Equal(1, new NgoSizeRule().Score(Facts(peopleServed: 1)));
    }

    [Fact]
    public void RecentReceipts_DropsAsThisWeekGrows()
    {
        var rule = new RecentReceiptsRule();

        Assert.Equal(20, rule.Score(Facts(unitsThisWeek: 0)));
        Assert.Equal(13, rule.Score(Facts(unitsThisWeek: 7)));
        Assert.Equal(0, rule.Score(Facts(unitsThisWeek: 25)));
    }

    [Fact]
    public void Reliability_GivesOnePointForEveryFive()
    {
        Assert.Equal(16, new ReliabilityRule().Score(Facts(reliability: 80)));
        Assert.Equal(8, new ReliabilityRule().Score(Facts(reliability: 40)));
    }

    [Fact]
    public void Total_AddsTheThreeRules()
    {
        var ngo = Facts(ReceiverType.Ngo, peopleServed: 80, reliability: 80, unitsThisWeek: 0);
        var person = Facts(ReceiverType.Individual, peopleServed: 1, reliability: 80, unitsThisWeek: 0);

        Assert.Equal(86, _rules.Sum(rule => rule.Score(ngo)));
        Assert.Equal(37, _rules.Sum(rule => rule.Score(person)));
    }

    private static PriorityFacts Facts(
        ReceiverType type = ReceiverType.Ngo,
        int peopleServed = 1,
        int reliability = 80,
        int unitsThisWeek = 0)
    {
        return new PriorityFacts(type, peopleServed, reliability, unitsThisWeek);
    }
}
