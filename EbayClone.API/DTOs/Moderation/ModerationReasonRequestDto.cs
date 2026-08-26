using System.ComponentModel.DataAnnotations;

namespace EbayClone.API.DTOs.Moderation;

public class ModerationReasonRequestDto
{
    [Required, MinLength(3), MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
