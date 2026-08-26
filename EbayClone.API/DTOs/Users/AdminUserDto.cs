namespace EbayClone.API.DTOs.Users;

public record AdminUserDto(
    int Id,
    string Email,
    string FullName,
    string Role,
    string ModerationStatus,
    string? ModerationReason,
    int? ModeratedBy,
    DateTime? ModeratedAtUtc);
