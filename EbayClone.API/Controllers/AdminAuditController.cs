using EbayClone.API.DTOs.Audit;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/audit")]
public class AdminAuditController(IAdminAuditService service) : ControllerBase
{
    [HttpGet]
    public async Task<PagedAdminAuditLogDto> GetPage(
        [FromQuery] string? action, [FromQuery] string? resourceType,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await service.GetPageAsync(action, resourceType, page, pageSize, cancellationToken);
    }
}
