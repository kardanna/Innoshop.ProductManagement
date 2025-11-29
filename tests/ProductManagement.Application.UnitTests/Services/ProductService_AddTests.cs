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

public class ProductService_AddTests
{
    private readonly IProductRepository _productRepositoryMock;
    private readonly IProductPolicy _productPolicyMock;

    
    private readonly IProductService _service;

    public ProductService_AddTests()
    {
        _productRepositoryMock = Substitute.For<IProductRepository>();
        _productPolicyMock = Substitute.For<IProductPolicy>();

        _service = new ProductService(
            _productRepositoryMock,
            _productPolicyMock
        );
    }

    private static readonly ProductOwner owner = new()
    {
        IsDeactivated = false,
        IsDeleted = false
    };

    private static readonly AddProductContext addProductContext = new(
        owner,
        "name",
        "descritpiton",
        "measuremnentUnit"
    );

    [Fact]
    public async Task ProductService_ShouldReturnErrorOnAdd_WhenPolicyDenies()
    {
        //Arrange
        var error = DomainErrors.ProductOwner.Deactivated;
        _productPolicyMock.IsCreationAllowedAsync(addProductContext).Returns(error);

        //Act
        var result = await _service.AddAsync(addProductContext);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task ProductService_ShouldReturnErrorOnAdd_WhenMeasurementUnitIsUnknown()
    {
        //Arrange
        var context = addProductContext with { MeasurementUnit = "???" };
        _productPolicyMock.IsCreationAllowedAsync(context).Returns(PolicyResult.Success);
        
        //Act
        var result = await _service.AddAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.MeasurementUnit.UnknownUnit);
    }

    [Fact]
    public async Task ProductService_ShouldReturnProductOnAdd_WhenAllRulesApply()
    {
        //Arrange
        var unit = MeasurementUnit.Meter;
        var context = addProductContext with { MeasurementUnit = unit.Name };
        _productPolicyMock.IsCreationAllowedAsync(context).Returns(PolicyResult.Success);

        //Act
        var result = await _service.AddAsync(context);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(context.Name);
        result.Value.Description.Should().Be(context.Description);
        result.Value.MeasurementUnitId.Should().Be(unit.Id);
        result.Value.Owner.Should().Be(owner);
        _productRepositoryMock.Received(1)
            .Add(Arg.Is<Product>(p => p.Name == context.Name 
                && p.Description == context.Description
                && p.MeasurementUnitId == unit.Id 
                && p.Owner == owner));
    }
}
