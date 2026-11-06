namespace FoodBridge.Application.Allocation;

public sealed class RecentReceiptsRule : IPriorityRule
{
    public const int StartingPoints = 20;

    public int Score(PriorityFacts facts) =>
        Math.Max(0, StartingPoints - facts.UnitsCollectedInLast7Days);
}
