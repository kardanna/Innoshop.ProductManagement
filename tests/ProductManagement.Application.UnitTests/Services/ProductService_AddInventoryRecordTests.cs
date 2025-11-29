using FluentAssertions;
using NSubstitute;
using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.UnitTests.Services;

public class ProductService_AddInventoryRecordTests
{
    private readonly IProductRepository _productRepositoryMock;
    private readonly IProductPolicy _productPolicyMock;

    
    private readonly IProductService _service;

    public ProductService_AddInventoryRecordTests()
    {
        _productRepositoryMock = Substitute.For<IProductRepository>();
        _productPolicyMock = Substitute.For<IProductPolicy>();

        _service = new ProductService(
            _productRepositoryMock,
            _productPolicyMock
        );
    }

    private static readonly Product product = new()
    {
        Id = Guid.CreateVersion7()
    };

    private static readonly AddProductInventoryContext addProductInventoryContext = new(
        product.Id,
        10m,
        20m,
        Guid.CreateVersion7()
    );

    [Fact]
    public async Task ProductService_ShouldReturnErrorOnAddInventoryRecord_WhenProductNotFound()
    {
        //Arrange
        var context = addProductInventoryContext;
        _productRepositoryMock.GetAsync(context.ProductId).Returns((Product)null!);

        //Act
        var result = await _service.AddInventoryRecordAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.Product.NotFound);
    }

    [Fact]
    public async Task ProductService_ShouldReturnErrorOnAddInventoryRecord_WhenPolicyDenies()
    {
        //Arrange
        var context = addProductInventoryContext;
        _productRepositoryMock.GetAsync(context.ProductId).Returns(product);
        _productPolicyMock.IsRetrievalAllowedAsync(product, context.RequesterId).Returns(PolicyResult.Success);
        var error = DomainErrors.ProductOwner.Deactivated;
        _productPolicyMock.IsInventoryRecordCreationAllowedAsync(product, context).Returns(error);

        //Act
        var result = await _service.AddInventoryRecordAsync(context);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task ProductService_ShouldReturnProductOnAddInventoryRecord_WhenPolicyAllows()
    {
        //Arrange
        var context = addProductInventoryContext;
        _productRepositoryMock.GetAsync(context.ProductId).Returns(product);
        _productPolicyMock.IsRetrievalAllowedAsync(product, context.RequesterId).Returns(PolicyResult.Success);
        _productPolicyMock.IsInventoryRecordCreationAllowedAsync(product, context).Returns(PolicyResult.Success);

        //Act
        var result = await _service.AddInventoryRecordAsync(context);

        //Assert
        result.Value.InventoryRecords.Should()
            .Contain(r => r.ProductId == product.Id
                && r.UnitPrice == context.Price
                && r.Quantity == context.Quantity);
    }
}
