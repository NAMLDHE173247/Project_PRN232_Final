using EbayClone.API.DTOs.Returns;
using EbayClone.API.Models;

namespace EbayClone.API.Repositories;

public interface IReturnRequestRepository
{
    Task<(int Total, IReadOnlyList<ReturnRequestDto> Items)> GetPageAsync(
        string? status, int? userId, int? orderId, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ReturnRequestDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ReturnRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
