using System;
using System.Collections.Generic;

namespace EbayClone.API.Models;

public partial class Review
{
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public int? ReviewerId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string ModerationStatus { get; set; } = "Visible";
    public string? ModerationReason { get; set; }
    public int? ModeratedBy { get; set; }
    public DateTime? ModeratedAtUtc { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? Reviewer { get; set; }
}
