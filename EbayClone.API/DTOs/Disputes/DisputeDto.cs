namespace EbayClone.API.DTOs.Disputes;

public record DisputeDto(
    int Id,
    int? OrderId,
    int? RaisedBy,
    string? RaisedByName,
    string? Description,
    string? Status,
    string? Resolution,
    int? AssignedTo,
    string? AssignedToName,
    DateTime? AssignedAtUtc,
    DateTime? ReviewStartedAtUtc,
    int? ResolvedBy,
    DateTime? ResolvedAtUtc);
