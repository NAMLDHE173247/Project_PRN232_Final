using System;
using System.Collections.Generic;

namespace EbayClone.API.Models;

public partial class Dispute
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? RaisedBy { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public string? Resolution { get; set; }

    public int? AssignedTo { get; set; }
    public DateTime? AssignedAtUtc { get; set; }
    public DateTime? ReviewStartedAtUtc { get; set; }
    public int? ResolvedBy { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }

    public virtual OrderTable? Order { get; set; }

    public virtual User? RaisedByNavigation { get; set; }
}
