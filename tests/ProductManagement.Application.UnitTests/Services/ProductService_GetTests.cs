using FluentAssertions;
using NSubstitute;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.UnitTests.Services;

public class ProductService_GetTests
{
    private readonly IProductRepository _productRepositoryMock;
    private readonly IProductPolicy _productPolicyMock;

    
    private readonly IProductService _service;

    public ProductService_GetTests()
    {
        _productRepositoryMock = Substitute.For<IProductRepository>();
        _productPolicyMock = Substitute.For<IProductPolicy>();

        _service = new ProductService(
            _productRepositoryMock,
            _productPolicyMock
        );
    }

    private static readonly Product product = new();

    [Fact]
    public async Task ProductService_ShouldReturnErrorOnGet_WhenProductIsNotFound()
    {
        //Arrange
        var productId = Guid.CreateVersion7();
        Guid? requesterId = Guid.CreateVersion7();
        _productRepositoryMock.GetAsync(productId).Returns((Product)null!);

        //Act
        var result = await _service.GetAsync(productId, requesterId);

        //Assert
        result.Error.Should().Be(DomainErrors.Product.NotFound);
    }

    [Fact]
    public async Task ProductService_ShouldReturnErrorOnGet_WhenPolicyDenies()
    {
        //Arrange
        var productId = Guid.CreateVersion7();
        Guid? requesterId = Guid.CreateVersion7();
        _productRepositoryMock.GetAsync(productId).Returns(product);
        var error = DomainErrors.Authentication.Unauthorized;
        _productPolicyMock.IsRetrievalAllowedAsync(product, requesterId).Returns(error);

        //Act
        var result = await _service.GetAsync(productId, requesterId);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task ProductService_ShouldReturnProductOnGet_WhenPolicyAllows()
    {
        //Arrange
        var productId = Guid.CreateVersion7();
        Guid? requesterId = Guid.CreateVersion7();
        _productRepositoryMock.GetAsync(productId).Returns(product);
        _productPolicyMock.IsRetrievalAllowedAsync(product, requesterId).Returns(PolicyResult.Success);

        //Act
        var result = await _service.GetAsync(productId, requesterId);

        //Assert
        result.Value.Should().Be(product);
    }
}
