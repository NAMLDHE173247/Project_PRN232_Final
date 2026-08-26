using EbayClone.API.DTOs.Returns;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AdminReturnRequestService(IReturnRequestRepository repository, IAdminAuditRepository auditRepository) : IAdminReturnRequestService
{
    public async Task<PagedReturnRequestResultDto> GetPageAsync(
        string? status, int? userId, int? orderId, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await repository.GetPageAsync(status, userId, orderId, from, to, page, pageSize, cancellationToken);
        return new(page, pageSize, result.Total, result.Items);
    }

    public Task<ReturnRequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        repository.GetDtoByIdAsync(id, cancellationToken);

    public Task<ReturnRequestDto?> ApproveAsync(int id, int adminId, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, "Approved", cancellationToken);

    public Task<ReturnRequestDto?> RejectAsync(int id, int adminId, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, "Rejected", cancellationToken);

    private async Task<ReturnRequestDto?> ChangeStatusAsync(int id, int adminId, string nextStatus, CancellationToken cancellationToken)
    {
        var request = await repository.GetByIdAsync(id, cancellationToken);
        if (request is null) return null;
        if (!string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only pending return requests can be approved or rejected.");

        request.Status = nextStatus;
        auditRepository.Add(adminId, nextStatus == "Approved" ? "APPROVE_RETURN" : "REJECT_RETURN", "ReturnRequest", id, request.Reason);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetDtoByIdAsync(id, cancellationToken);
    }
}
