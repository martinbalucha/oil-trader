namespace OilTrader.Tests.Integration.Infrastructure;

public static class IntegrationTestCollection
{
    public const string HappyPath = "Integration";
    public const string FaultInjection = "Integration.FaultInjection";
}

[CollectionDefinition(IntegrationTestCollection.HappyPath)]
public class HappyPathIntegrationCollection : ICollectionFixture<OilTraderWebApplicationFactory>;

[CollectionDefinition(IntegrationTestCollection.FaultInjection)]
public class FaultInjectionIntegrationCollection : ICollectionFixture<FaultInjectingWebApplicationFactory>;
