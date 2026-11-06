namespace FoodBridge.Application.Allocation;

public interface IPriorityRule
{
    int Score(PriorityFacts facts);
}
