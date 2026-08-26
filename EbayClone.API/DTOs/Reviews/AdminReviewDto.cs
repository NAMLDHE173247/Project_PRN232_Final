namespace EbayClone.API.DTOs.Reviews;

public record AdminReviewDto(
    int Id,
    int? ProductId,
    int? ReviewerId,
    int? Rating,
    string? Comment,
    DateTime? CreatedAt,
    string ModerationStatus,
    string? ModerationReason,
    int? ModeratedBy,
    DateTime? ModeratedAtUtc);
