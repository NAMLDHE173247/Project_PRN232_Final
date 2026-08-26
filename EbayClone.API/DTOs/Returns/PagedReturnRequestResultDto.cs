namespace EbayClone.API.DTOs.Returns;

public record PagedReturnRequestResultDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<ReturnRequestDto> Items);
