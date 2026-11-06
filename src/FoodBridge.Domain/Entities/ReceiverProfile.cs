using FoodBridge.Domain.Enums;

namespace FoodBridge.Domain.Entities;

public sealed class ReceiverProfile
{
    public const int StartingScore = 80;
    public const int NoShowPenalty = 20;
    public const int BlockThreshold = 40;
    public const int BlockDays = 3;

    public string UserId { get; private set; } = string.Empty;
    public ReceiverType ReceiverType { get; private set; }
    public int PeopleServed { get; private set; }
    public bool IsApproved { get; private set; }
    public int ReliabilityScore { get; private set; } = StartingScore;
    public DateTime? BlockedUntil { get; private set; }

    private ReceiverProfile()
    {
    }

    public ReceiverProfile(string userId, ReceiverType receiverType, int peopleServed)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User is required.", nameof(userId));
        }

        if (peopleServed <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(peopleServed));
        }

        UserId = userId;
        ReceiverType = receiverType;
        PeopleServed = peopleServed;
        IsApproved = receiverType == ReceiverType.Individual;
    }

    public void Approve()
    {
        IsApproved = true;
    }

    public void ApplyNoShow(DateTime utcNow)
    {
        ReliabilityScore = Math.Max(0, ReliabilityScore - NoShowPenalty);
        if (ReliabilityScore < BlockThreshold)
        {
            BlockedUntil = utcNow.AddDays(BlockDays);
        }
    }
}
