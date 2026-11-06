namespace FoodBridge.Application.Allocation;

public sealed class ReliabilityRule : IPriorityRule
{
    public const int PointsPerFive = 5;

    public int Score(PriorityFacts facts) => facts.ReliabilityScore / PointsPerFive;
}
