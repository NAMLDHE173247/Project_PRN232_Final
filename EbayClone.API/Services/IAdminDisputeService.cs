using EbayClone.API.DTOs.Disputes;

namespace EbayClone.API.Services;

public interface IAdminDisputeService
{
    Task<PagedDisputeResultDto> GetDisputesAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<DisputeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DisputeDto?> AssignAsync(int id, int adminId, int assigneeId, CancellationToken cancellationToken = default);
    Task<DisputeDto?> StartReviewAsync(int id, int adminId, CancellationToken cancellationToken = default);
    Task<DisputeDto?> ResolveAsync(
        int id,
        int adminId,
        string resolution,
        CancellationToken cancellationToken = default);
    Task<DisputeDto?> RejectAsync(
        int id,
        int adminId,
        string resolution,
        CancellationToken cancellationToken = default);
}
