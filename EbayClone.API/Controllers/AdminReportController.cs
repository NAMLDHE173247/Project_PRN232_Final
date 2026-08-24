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
    public Task<AdminReportDto> GetSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken) => reportService.GetAsync(from, to, cancellationToken);
}
