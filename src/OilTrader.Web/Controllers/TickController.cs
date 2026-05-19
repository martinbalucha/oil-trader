using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OilTrader.Contracts.TickManagement;
using OilTrader.Web.Models;

namespace OilTrader.Web.Controllers;

/// <summary>
/// API Controller for Tick operations
/// </summary>
[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/tick")]
public class TickController : ControllerBase
{
    private readonly ITickService _tickService;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="tickService">Tick service</param>
    public TickController(ITickService tickService)
    {
        _tickService = tickService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "The tick is processed successfully")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest, Description = "Tick is invalid")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, Description = "Internal server error")]
    public async Task<IActionResult> PostTickAsync([FromBody] TickRequest request, CancellationToken ct)
    {
        var tick = request.ToDomain();

        await _tickService.ProcessAsync(tick, ct);

        return NoContent();
    }
}
