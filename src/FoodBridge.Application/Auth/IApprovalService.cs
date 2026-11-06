namespace FoodBridge.Application.Auth;

public interface IApprovalService
{
    Task<IReadOnlyList<PendingApprovalResponse>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task ApproveAsync(string userId, CancellationToken cancellationToken = default);
}
