namespace BallastLaneBoard.WebApi.IntegrationTests.Infrastructure;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection
    : ICollectionFixture<PostgresFixture>,
      ICollectionFixture<KeycloakFixture>,
      ICollectionFixture<ApiFixture>;
