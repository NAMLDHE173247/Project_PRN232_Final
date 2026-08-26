using EbayClone.API.DTOs.Reviews;

namespace EbayClone.API.Services;

public interface IAdminReviewService
{
    Task<PagedReviewResultDto> GetReviewsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<AdminReviewDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminReviewDto?> HideAsync(int id, int adminId, string reason, CancellationToken cancellationToken = default);
    Task<AdminReviewDto?> RestoreAsync(int id, int adminId, CancellationToken cancellationToken = default);
}
