using EbayClone.API.DTOs.Returns;

namespace EbayClone.API.Services;

public interface IAdminReturnRequestService
{
    Task<PagedReturnRequestResultDto> GetPageAsync(
        string? status, int? userId, int? orderId, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ReturnRequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ReturnRequestDto?> ApproveAsync(int id, int adminId, CancellationToken cancellationToken = default);
    Task<ReturnRequestDto?> RejectAsync(int id, int adminId, CancellationToken cancellationToken = default);
}
