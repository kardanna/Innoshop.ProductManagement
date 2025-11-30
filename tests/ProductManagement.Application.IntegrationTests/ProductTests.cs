using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.UseCases.ProductInventory.Add;
using ProductManagement.Application.UseCases.Products.Add;
using ProductManagement.Application.UseCases.Products.Delete;
using ProductManagement.Application.UseCases.Products.Get;
using ProductManagement.Application.UseCases.Products.GetAll;
using ProductManagement.Application.UseCases.Products.Update;

namespace ProductManagement.Application.IntegrationTests;

[Collection(IntegrationTestCollection.CollectionName)]
public class ProductTests : BaseIntegrationTest
{
    public ProductTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Add_ShouldAdd_NewProductToDatabase()
    {
        //Arrange
        var addProductCommand = new AddProductCommand(
            "name",
            "description",
            "шт.",
            Guid.CreateVersion7()
        );

        //Act
        var response = await _sender.Send(addProductCommand);
        var createdProductId = response.Value.Id;
        var product = await _appContext.Products.FirstOrDefaultAsync(p => p.Id == createdProductId);

        //Assert
        product.Should().NotBeNull();
        product.Name.Should().Be(addProductCommand.Name);
        product.Description.Should().Be(addProductCommand.Description);
        product.MeasurementUnit.Name.Should().Be(addProductCommand.MeasurementUnit);
        product.OwnerId.Should().Be(addProductCommand.OwnerId);
        product.InventoryRecords.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_ShouldRetrieve_ProductFromTheDatabase()
    {
        //Arrange
        var addProductCommand = new AddProductCommand(
            "name",
            "description",
            "шт.",
            Guid.CreateVersion7()
        );
        var addProductCommandResponse = await _sender.Send(addProductCommand);
        var createdProductId = addProductCommandResponse.Value.Id;
        var getProductQuery = new GetProductQuery(
            createdProductId,
            null
        );

        //Act
        var getProductQueryResponse = await _sender.Send(getProductQuery);

        //Assert
        getProductQueryResponse.Value.Id.Should().Be(createdProductId);
        getProductQueryResponse.Value.Name.Should().Be(addProductCommand.Name);
        getProductQueryResponse.Value.Description.Should().Be(addProductCommand.Description);
        getProductQueryResponse.Value.MeasurementUnit.Should().Be(addProductCommand.MeasurementUnit);
        getProductQueryResponse.Value.OwnerId.Should().Be(addProductCommand.OwnerId);
    }

    [Fact]
    public async Task Update_ShouldUpdate_ProductInTheDatabase()
    {
        //Arrange
        var addProductCommand = new AddProductCommand(
            "name",
            "description",
            "шт.",
            Guid.CreateVersion7()
        );
        var addProductCommandResponse = await _sender.Send(addProductCommand);
        var createdProductId = addProductCommandResponse.Value.Id;
        var updateProductCommand = new UpdateProductCommand(
            createdProductId,
            "new name",
            "new description",
            "кг",
            addProductCommand.OwnerId
        );

        //Act
        await _sender.Send(updateProductCommand);
        var product = await _appContext.Products.FirstOrDefaultAsync(p => p.Id == createdProductId);

        //Assert
        product.Should().NotBeNull();
        product.Id.Should().Be(createdProductId);
        product.Name.Should().Be(updateProductCommand.Name);
        product.Description.Should().Be(updateProductCommand.Description);
        product.MeasurementUnit.Name.Should().Be(updateProductCommand.MeasurementUnit);
    }

    [Fact]
    public async Task GetAll_ShouldGetAll_ProductsInTheDatabase()
    {
        //Arrange
        var addFirstProductCommand = new AddProductCommand(
            "name",
            "description",
            "шт.",
            Guid.CreateVersion7()
        );
        var addFirstProductCommandResponse = await _sender.Send(addFirstProductCommand);
        var firstProductId = addFirstProductCommandResponse.Value.Id;
        var addSecondProductCommand = new AddProductCommand(
            "name",
            "description",
            "шт.",
            Guid.CreateVersion7()
        );
        var addSecondProductCommandResponse = await _sender.Send(addSecondProductCommand);
        var secondProductId = addSecondProductCommandResponse.Value.Id;

        var getAllProductsQuery = new GetAllProductsQuery();

        //Act
        var getAllProductsQueryResponse = await _sender.Send(getAllProductsQuery);

        //Assert
        getAllProductsQueryResponse.Value.Should().NotBeNullOrEmpty();
        getAllProductsQueryResponse.Value.Should().Contain(p => p.Id == firstProductId);
        getAllProductsQueryResponse.Value.Should().Contain(p => p.Id == secondProductId);
    }

    [Fact]
    public async Task Delete_ShouldDelete_TheProductFromTheDatabase()
    {
        //Arrange
        var addProductCommand = new AddProductCommand(
            "name",
            "description",
            "шт.",
            Guid.CreateVersion7()
        );
        var addProductCommandResponse = await _sender.Send(addProductCommand);
        var createdProductId = addProductCommandResponse.Value.Id;
        
        var deleteProductCommand = new DeleteProductCommand(
            createdProductId,
            addProductCommand.OwnerId
        );

        //Act
        await _sender.Send(deleteProductCommand);
        var products = await _appContext.Products.ToListAsync();

        //Assert
        products.Should().NotBeNullOrEmpty();
        products.Should().NotContain(p => p.Id == createdProductId);
    }

    [Fact]
    public async Task AddProductInventory_ShouldAddProductInventory_ToTheProductFromTheDatabase()
    {
        //Arrange
        var addProductCommand = new AddProductCommand(
            "name",
            "description",
            "шт.",
            Guid.CreateVersion7()
        );
        var addProductCommandResponse = await _sender.Send(addProductCommand);
        var createdProductId = addProductCommandResponse.Value.Id;
        
        var addProductInventoryCommand = new AddProductInventoryCommand(
            createdProductId,
            10m,
            20m,
            addProductCommand.OwnerId
        );

        //Act
        await _sender.Send(addProductInventoryCommand);
        var product = await _appContext.Products.FirstOrDefaultAsync(p => p.Id == createdProductId);

        //Assert
        product.Should().NotBeNull();
        product.InventoryRecords.Should().Contain(r => 
            r.UnitPrice == addProductInventoryCommand.Price
            && r.Quantity == addProductInventoryCommand.Quantity);
    }
}