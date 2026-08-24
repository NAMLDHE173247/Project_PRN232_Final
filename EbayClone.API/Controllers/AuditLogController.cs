using EbayClone.API.DTOs.Audit;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/audit-logs")]
public class AuditLogController(IAuditService auditService) : ControllerBase
{
    [HttpGet]
    public Task<PagedAuditResultDto> GetPage(
        [FromQuery] string? search,
        [FromQuery] string? action,
        [FromQuery] string? resource,
        [FromQuery] int? actorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? sort,
        [FromQuery] string? direction,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return auditService.GetPageAsync(search, action, resource, actorId, from, to, sort, direction, page, pageSize, cancellationToken);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string? search,
        [FromQuery] string? action,
        [FromQuery] string? resource,
        [FromQuery] int? actorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? sort,
        [FromQuery] string? direction,
        CancellationToken cancellationToken)
    {
        var content = await auditService.ExportAsync(search, action, resource, actorId, from, to, sort, direction, cancellationToken);
        return File(content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx");
    }
}
