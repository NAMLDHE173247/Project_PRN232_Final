using System.ComponentModel.DataAnnotations;

namespace EbayClone.API.DTOs.Disputes;

public sealed class ResolveDisputeRequestDto
{
    [Required, MinLength(10), MaxLength(2000)]
    public string Resolution { get; set; } = string.Empty;
}
