using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OilTrader.Contracts;
using OilTrader.Contracts.TickManagement;

namespace OilTrader.Tests.Integration.Infrastructure;

public class FaultInjectingWebApplicationFactory : OilTraderWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(ITickService));
            services.Remove(descriptor);

            var mock = new Mock<ITickService>();
            mock.Setup(s => s.ProcessAsync(It.IsAny<Tick>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("integration-test fault"));
            services.AddSingleton(mock.Object);
        });
    }
}
