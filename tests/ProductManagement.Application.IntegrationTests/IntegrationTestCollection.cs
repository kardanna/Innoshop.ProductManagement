namespace ProductManagement.Application.IntegrationTests;


[CollectionDefinition(IntegrationTestCollection.CollectionName)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
    public const string CollectionName = "ProductManagement.Application.IntegrationTests";
}