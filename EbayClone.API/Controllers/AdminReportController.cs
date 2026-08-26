using EbayClone.API.DTOs.Reports;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/reports")]
public sealed class AdminReportController(AdminReportService reportService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<AdminReportDto>> GetSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            return BadRequest(new { message = "From date cannot be after To date." });
        return Ok(await reportService.GetAsync(from, to, cancellationToken));
    }
}
