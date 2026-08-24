using EbayClone.API.DTOs.Products;
using EbayClone.API.Models;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AdminProductService(IProductRepository productRepository, IAuditRepository auditRepository) : IAdminProductService
{
    public async Task<PagedProductResultDto<AdminProductDto>> GetProductsAsync(
        string? search,
        int? sellerId,
        ProductStatus? status,
        string? sort,
        string? direction,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await productRepository.GetPageAsync(search, sellerId, status, sort, direction, page, pageSize, cancellationToken);
        return new PagedProductResultDto<AdminProductDto>(page, pageSize, result.Total, result.Items.Select(Map).ToList());
    }

    public async Task<AdminProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : Map(product);
    }

    public Task<AdminProductDto?> HideAsync(int id, int adminId, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, ProductStatus.Active, ProductStatus.Hidden, "HIDE_PRODUCT", cancellationToken);

    public Task<AdminProductDto?> UnhideAsync(int id, int adminId, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, ProductStatus.Hidden, ProductStatus.Active, "UNHIDE_PRODUCT", cancellationToken);

    private async Task<AdminProductDto?> ChangeStatusAsync(
        int id,
        int adminId,
        ProductStatus expectedStatus,
        ProductStatus nextStatus,
        string auditAction,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null) return null;
        if (product.Status != expectedStatus)
            throw new InvalidOperationException($"Only {expectedStatus} products can be changed by this action.");

        product.Status = nextStatus;
        await productRepository.SaveChangesAsync(cancellationToken);
        await auditRepository.AddAsync(new AuditLog
        {
            ActorId = adminId,
            Action = auditAction,
            Resource = "PRODUCT",
            ResourceId = product.Id,
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);
        return Map(product);
    }

    private static AdminProductDto Map(Product product) =>
        new(product.Id, product.Name, product.Price, product.SellerId, product.Status);
}
