using Asp.Versioning;
using Microsoft.OpenApi;
using OilTrader.Contracts;
using OilTrader.Contracts.Messaging;
using OilTrader.Contracts.TickManagement;
using OilTrader.Domain.Messaging;
using OilTrader.Domain.TickManagement;
using OilTrader.Web.Infrastructure;
using Serilog;
using Serilog.Formatting.Json;
using System.Reflection;

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
builder.Services.AddSingleton<ITickRepository, QueueTickRepository>();
builder.Services.AddSingleton<IMessagePublisher, InMemoryMessagePublisher>();
builder.Services.AddSingleton<ITimeframeAggregator, TimeframeAggregator>();
builder.Services.AddSingleton<ITickService, TickService>();

builder.Services
    .AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "OilTrader API", Version = "v1" });
    // Include XML comments if GenerateDocumentationFile is enabled
    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
        o.IncludeXmlComments(xmlPath);
});

builder.Services.AddExceptionHandler<OilTraderExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "OilTrader API v1"));

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
