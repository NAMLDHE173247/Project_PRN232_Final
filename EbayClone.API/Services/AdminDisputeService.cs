using EbayClone.API.DTOs.Disputes;
using EbayClone.API.Models;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AdminDisputeService(IDisputeRepository disputeRepository, IAdminAuditRepository auditRepository)
    : IAdminDisputeService
{
    public async Task<PagedDisputeResultDto> GetDisputesAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await disputeRepository.GetPageAsync(status, page, pageSize, cancellationToken);
        return new PagedDisputeResultDto(page, pageSize, result.Total, result.Items);
    }

    public Task<DisputeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        disputeRepository.GetDtoByIdAsync(id, cancellationToken);

    public async Task<DisputeDto?> AssignAsync(int id, int adminId, int assigneeId, CancellationToken cancellationToken = default)
    {
        if (!await disputeRepository.IsAdminAsync(assigneeId, cancellationToken)) throw new ArgumentException("Assignee must be an Admin user.");
        var dispute = await disputeRepository.GetByIdAsync(id, cancellationToken);
        if (dispute is null) return null;
        if (dispute.Status != "Open") throw new InvalidOperationException("Only Open disputes can be assigned.");
        dispute.Status = "Assigned";
        dispute.AssignedTo = assigneeId;
        dispute.AssignedAtUtc = DateTime.UtcNow;
        auditRepository.Add(adminId, "ASSIGN_DISPUTE", "Dispute", id, $"Assigned to Admin #{assigneeId}");
        await disputeRepository.SaveChangesAsync(cancellationToken);
        return await disputeRepository.GetDtoByIdAsync(id, cancellationToken);
    }

    public async Task<DisputeDto?> StartReviewAsync(int id, int adminId, CancellationToken cancellationToken = default)
    {
        var dispute = await disputeRepository.GetByIdAsync(id, cancellationToken);
        if (dispute is null) return null;
        if (dispute.Status != "Assigned") throw new InvalidOperationException("Only Assigned disputes can enter review.");
        if (dispute.AssignedTo != adminId) throw new InvalidOperationException("Only the assigned Admin can start review.");
        dispute.Status = "InReview";
        dispute.ReviewStartedAtUtc = DateTime.UtcNow;
        auditRepository.Add(adminId, "START_DISPUTE_REVIEW", "Dispute", id);
        await disputeRepository.SaveChangesAsync(cancellationToken);
        return await disputeRepository.GetDtoByIdAsync(id, cancellationToken);
    }

    public Task<DisputeDto?> ResolveAsync(
        int id,
        int adminId,
        string resolution,
        CancellationToken cancellationToken = default) =>
        FinishAsync(id, adminId, resolution, DisputeStatus.Open, DisputeStatus.Resolved, "RESOLVE_DISPUTE", cancellationToken);

    public Task<DisputeDto?> RejectAsync(
        int id,
        int adminId,
        string resolution,
        CancellationToken cancellationToken = default) =>
        FinishAsync(id, adminId, resolution, DisputeStatus.Open, DisputeStatus.Rejected, "REJECT_DISPUTE", cancellationToken);

    private async Task<DisputeDto?> FinishAsync(
        int id,
        int adminId,
        string resolution,
        DisputeStatus expectedStatus,
        DisputeStatus nextStatus,
        string auditAction,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resolution))
            throw new InvalidOperationException("Resolution is required.");
        if (resolution.Trim().Length > 2000)
            throw new InvalidOperationException("Resolution cannot exceed 2000 characters.");

        var dispute = await disputeRepository.GetByIdAsync(id, cancellationToken);
        if (dispute is null) return null;
        if (dispute.Status is not ("Open" or "Assigned" or "InReview"))
            throw new InvalidOperationException("Only active disputes can be changed by this action.");
        if (dispute.AssignedTo.HasValue && dispute.AssignedTo != adminId)
            throw new InvalidOperationException("Only the assigned Admin can finish this dispute.");

        dispute.Status = nextStatus.ToString();
        dispute.Resolution = resolution.Trim();
        dispute.ResolvedBy = adminId;
        dispute.ResolvedAtUtc = DateTime.UtcNow;
        auditRepository.Add(adminId, auditAction, "Dispute", id, dispute.Resolution);
        await disputeRepository.SaveChangesAsync(cancellationToken);
        return await disputeRepository.GetDtoByIdAsync(id, cancellationToken);
    }

}
