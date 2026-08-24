using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

[ApiController]
public sealed class ApiHealthController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<ApiHealthController> logger) : ControllerBase
{
    [HttpGet("/health/api")]
    public async Task<IActionResult> Check(CancellationToken cancellationToken)
    {
        var baseUrl = configuration["ApiBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "Unhealthy", reason = "ApiBaseUrl is not configured." });

        try
        {
            var client = httpClientFactory.CreateClient("api-health");
            using var response = await client.GetAsync("health", cancellationToken);
            if (response.IsSuccessStatusCode)
                return Ok(new { status = "Healthy", dependency = "EbayClone.API", apiStatus = (int)response.StatusCode });

            logger.LogWarning("API health check returned HTTP {StatusCode}.", (int)response.StatusCode);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "Unhealthy", dependency = "EbayClone.API", apiStatus = (int)response.StatusCode });
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(exception, "API health check failed.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "Unhealthy", dependency = "EbayClone.API" });
        }
    }
}
