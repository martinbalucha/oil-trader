using Serilog;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

var listenUrls = builder.Configuration.GetSection("Server").GetValue<string>("Urls")
    ?? throw new InvalidOperationException(
        "Missing configuration key Server:Urls; see appsettings.json.");
builder.WebHost.UseUrls(listenUrls);

var rollingLogPath = Path.Combine(builder.Environment.ContentRootPath, "logs", "oiltrader-.json");

builder.Host.UseSerilog((context, _, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter())
        .WriteTo.File(new JsonFormatter(), rollingLogPath, rollingInterval: RollingInterval.Day);
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapControllers();

try
{
    Log.Information("OilTrader.Web starting");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
