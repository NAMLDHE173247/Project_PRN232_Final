using System.ComponentModel.DataAnnotations;

namespace EbayClone.API.DTOs.Disputes;

public class AssignDisputeRequestDto
{
    [Range(1, int.MaxValue)]
    public int AdminUserId { get; set; }
}
