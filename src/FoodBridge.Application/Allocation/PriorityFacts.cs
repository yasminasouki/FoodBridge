using FoodBridge.Domain.Enums;

namespace FoodBridge.Application.Allocation;

public sealed record PriorityFacts(
    ReceiverType ReceiverType,
    int PeopleServed,
    int ReliabilityScore,
    int UnitsCollectedInLast7Days);
