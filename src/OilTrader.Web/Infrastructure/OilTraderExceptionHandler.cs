using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OilTrader.Web.Infrastructure;

public class OilTraderExceptionHandler : IExceptionHandler
{
    private readonly ILogger<OilTraderExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public OilTraderExceptionHandler(
        ILogger<OilTraderExceptionHandler> logger,
        IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        _logger.LogError(ex, "Unhandled exception");

        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = _env.IsDevelopment() ? ex.Message : null,
            },
            ct);

        return true;
    }
}
