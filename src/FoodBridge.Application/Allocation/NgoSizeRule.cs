namespace FoodBridge.Application.Allocation;

public sealed class NgoSizeRule : IPriorityRule
{
    public const int Cap = 50;

    public int Score(PriorityFacts facts) => Math.Min(facts.PeopleServed, Cap);
}
