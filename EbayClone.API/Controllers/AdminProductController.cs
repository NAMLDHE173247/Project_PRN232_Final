using EbayClone.API.DTOs.Products;
using EbayClone.API.DTOs.Moderation;
using System.Security.Claims;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/products")]
public class AdminProductController(IAdminProductService productService) : ControllerBase
{
    [HttpGet]
    public Task<PagedProductResultDto<AdminProductDto>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] int? sellerId,
        [FromQuery] string? status,
        [FromQuery] string? sort,
        [FromQuery] string? direction,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        productService.GetProductsAsync(search, sellerId, status, sort, direction, page, pageSize, cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPut("{id:int}/hide")]
    public Task<ActionResult<AdminProductDto>> Hide(int id, ModerationReasonRequestDto request, CancellationToken cancellationToken) =>
        ExecuteTransition(() => productService.HideAsync(id, GetAdminId(), request.Reason, cancellationToken));

    [HttpPut("{id:int}/restore")]
    public Task<ActionResult<AdminProductDto>> Restore(int id, CancellationToken cancellationToken) =>
        ExecuteTransition(() => productService.RestoreAsync(id, GetAdminId(), cancellationToken));

    private static async Task<ActionResult<AdminProductDto>> ExecuteTransition(Func<Task<AdminProductDto?>> transition)
    {
        try { var product = await transition(); return product is null ? new NotFoundResult() : new OkObjectResult(product); }
        catch (InvalidOperationException exception) { return new ConflictObjectResult(new { message = exception.Message }); }
        catch (DbUpdateConcurrencyException) { return new ConflictObjectResult(new { message = "The product state changed before this action completed." }); }
    }

    private int GetAdminId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

}
