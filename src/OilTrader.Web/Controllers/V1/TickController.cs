using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OilTrader.Contracts.TickManagement;
using OilTrader.Web.Models;

namespace OilTrader.Web.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/tick")]
public class TickController : ControllerBase
{
    private readonly ITickService _tickService;

    public TickController(ITickService tickService) { _tickService = tickService; }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostTickAsync([FromBody] TickRequest request, CancellationToken ct)
    {
        var tick = request.ToDomain();
        await _tickService.ProcessAsync(tick, ct);
        return NoContent();
    }
}
