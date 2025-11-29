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

public class ProductService_UpdateTests
{
    private readonly IProductRepository _productRepositoryMock;
    private readonly IProductPolicy _productPolicyMock;

    
    private readonly IProductService _service;

    public ProductService_UpdateTests()
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

    private static readonly UpdateProductContext updateProductInventoryContext = new(
        product.Id,
        "name",
        "discovery",
        "measurementUnit",
        Guid.CreateVersion7()
    );

    [Fact]
    public async Task ProductService_ShouldReturnErrorOnUpdate_WhenMeasurementUnitIsUnknown()
    {
        //Arrange
        var context = updateProductInventoryContext with { MeasurementUnit = "???" };

        //Act
        var result = await _service.UpdateAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.MeasurementUnit.UnknownUnit);
    }

    [Fact]
    public async Task ProductService_ShouldReturnErrorOnUpdate_WhenProductIsNotFound()
    {
        //Arrange
        var unit = MeasurementUnit.Meter;
        var context = updateProductInventoryContext with { MeasurementUnit = unit.Name };
        _productRepositoryMock.GetAsync(context.ProductId).Returns((Product)null!);

        //Act
        var result = await _service.UpdateAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.Product.NotFound);
    }

    [Fact]
    public async Task ProductService_ShouldReturnErrorOnUpdate_WhenPolicyDenies()
    {
        //Arrange
        var unit = MeasurementUnit.Meter;
        var context = updateProductInventoryContext with { MeasurementUnit = unit.Name };
        _productRepositoryMock.GetAsync(context.ProductId).Returns(product);
        _productPolicyMock.IsRetrievalAllowedAsync(product, context.RequesterId).Returns(PolicyResult.Success);
        var error = DomainErrors.ProductOwner.Deactivated;
        _productPolicyMock.IsUpdateAllowedAsync(product, context).Returns(error);

        //Act
        var result = await _service.UpdateAsync(context);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task ProductService_ShouldReturnProductOnUpdate_WhenPolicyAllows()
    {
        //Arrange
        var unit = MeasurementUnit.Meter;
        var context = updateProductInventoryContext with { MeasurementUnit = unit.Name };
        _productRepositoryMock.GetAsync(context.ProductId).Returns(product);
        _productPolicyMock.IsRetrievalAllowedAsync(product, context.RequesterId).Returns(PolicyResult.Success);
        _productPolicyMock.IsUpdateAllowedAsync(product, context).Returns(PolicyResult.Success);

        //Act
        var result = await _service.UpdateAsync(context);

        //Assert
        result.Value.Name.Should().Be(context.Name);
        result.Value.Description.Should().Be(context.Description);
        result.Value.MeasurementUnitId.Should().Be(unit.Id);
    }
}