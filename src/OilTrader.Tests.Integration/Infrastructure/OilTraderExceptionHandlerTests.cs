using System.Net;

namespace OilTrader.Tests.Integration.Infrastructure;

[Collection(IntegrationTestCollection.FaultInjection)]
public class OilTraderExceptionHandlerTests
{
    private readonly FaultInjectingWebApplicationFactory _factory;

    public OilTraderExceptionHandlerTests(FaultInjectingWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostTickAsync_UnhandledServiceFault_Returns500WithProblemJson()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostTickAsync(
            TickTestPayloads.ValidJson($"TEST-{Guid.NewGuid():N}".ToUpperInvariant()));

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task PostTickAsync_DevelopmentEnvironment_IncludesExceptionDetail()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostTickAsync(
            TickTestPayloads.ValidJson($"TEST-{Guid.NewGuid():N}".ToUpperInvariant()));

        // Assert
        var detail = await response.ReadDetailAsync();
        Assert.Equal("integration-test fault", detail);
    }
}
