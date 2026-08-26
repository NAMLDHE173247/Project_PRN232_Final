using EbayClone.API.DTOs.Products;
using EbayClone.API.Models;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AdminProductService(IProductRepository productRepository, IAdminAuditRepository auditRepository) : IAdminProductService
{
    public async Task<PagedProductResultDto<AdminProductDto>> GetProductsAsync(
        string? search, int? sellerId, string? status, string? sort, string? direction,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await productRepository.GetPageAsync(search, sellerId, status, sort, direction, page, pageSize, cancellationToken);
        return new(page, pageSize, result.Total, result.Items.Select(Map).ToList());
    }

    public Task<AdminProductDto?> HideAsync(int id, int adminId, string reason, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, "Active", "Hidden", "HIDE_PRODUCT", reason.Trim(), cancellationToken);

    public Task<AdminProductDto?> RestoreAsync(int id, int adminId, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, "Hidden", "Active", "RESTORE_PRODUCT", null, cancellationToken);

    private async Task<AdminProductDto?> ChangeStatusAsync(int id, int adminId, string expected, string next, string action, string? reason, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null) return null;
        if (product.ModerationStatus != expected) throw new InvalidOperationException($"Only {expected} products can perform this transition.");
        product.ModerationStatus = next;
        product.ModerationReason = reason;
        product.ModeratedBy = adminId;
        product.ModeratedAtUtc = DateTime.UtcNow;
        auditRepository.Add(adminId, action, "Product", id, reason);
        await productRepository.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<AdminProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : Map(product);
    }

    private static AdminProductDto Map(Product product) =>
        new(product.Id, product.Name, product.Price, product.SellerId, product.ModerationStatus, product.ModerationReason, product.ModeratedBy, product.ModeratedAtUtc);
}
