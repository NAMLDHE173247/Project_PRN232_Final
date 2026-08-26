using EbayClone.API.DTOs.Products;

namespace EbayClone.API.Services;

public interface IAdminProductService
{
    Task<PagedProductResultDto<AdminProductDto>> GetProductsAsync(
        string? search,
        int? sellerId,
        string? status,
        string? sort,
        string? direction,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<AdminProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminProductDto?> HideAsync(int id, int adminId, string reason, CancellationToken cancellationToken = default);
    Task<AdminProductDto?> RestoreAsync(int id, int adminId, CancellationToken cancellationToken = default);
}
