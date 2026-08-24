using EbayClone.API.DTOs.Products;
using EbayClone.API.Models;

namespace EbayClone.API.Services;

public interface IAdminProductService
{
    Task<PagedProductResultDto<AdminProductDto>> GetProductsAsync(
        string? search,
        int? sellerId,
        ProductStatus? status,
        string? sort,
        string? direction,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<AdminProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminProductDto?> HideAsync(int id, int adminId, CancellationToken cancellationToken = default);
    Task<AdminProductDto?> UnhideAsync(int id, int adminId, CancellationToken cancellationToken = default);
}
