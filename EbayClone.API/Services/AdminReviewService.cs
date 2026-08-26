using EbayClone.API.DTOs.Reviews;
using EbayClone.API.Models;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AdminReviewService(IReviewRepository reviewRepository, IAdminAuditRepository auditRepository) : IAdminReviewService
{
    public async Task<PagedReviewResultDto> GetReviewsAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await reviewRepository.GetPageAsync(page, pageSize, cancellationToken);
        return new(page, pageSize, result.Total, result.Items.Select(Map).ToList());
    }

    public Task<AdminReviewDto?> HideAsync(int id, int adminId, string reason, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, "Visible", "Hidden", "HIDE_REVIEW", reason.Trim(), cancellationToken);

    public Task<AdminReviewDto?> RestoreAsync(int id, int adminId, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, "Hidden", "Visible", "RESTORE_REVIEW", null, cancellationToken);

    private async Task<AdminReviewDto?> ChangeStatusAsync(int id, int adminId, string expected, string next, string action, string? reason, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(id, cancellationToken);
        if (review is null) return null;
        if (review.ModerationStatus != expected) throw new InvalidOperationException($"Only {expected} reviews can perform this transition.");
        review.ModerationStatus = next;
        review.ModerationReason = reason;
        review.ModeratedBy = adminId;
        review.ModeratedAtUtc = DateTime.UtcNow;
        auditRepository.Add(adminId, action, "Review", id, reason);
        await reviewRepository.SaveChangesAsync(cancellationToken);
        return Map(review);
    }

    public async Task<AdminReviewDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var review = await reviewRepository.GetByIdAsync(id, cancellationToken);
        return review is null ? null : Map(review);
    }

    private static AdminReviewDto Map(Review review) =>
        new(review.Id, review.ProductId, review.ReviewerId, review.Rating, review.Comment, review.CreatedAt, review.ModerationStatus, review.ModerationReason, review.ModeratedBy, review.ModeratedAtUtc);
}
