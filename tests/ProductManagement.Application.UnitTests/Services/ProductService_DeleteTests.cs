using FluentAssertions;
using NSubstitute;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.UnitTests.Services;

public class ProductService_DeleteTests
{
    private readonly IProductRepository _productRepositoryMock;
    private readonly IProductPolicy _productPolicyMock;

    
    private readonly IProductService _service;

    public ProductService_DeleteTests()
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
    public async Task ProductService_ShouldReturnErrorOnDelete_WhenProductIsNotFound()
    {
        //Arrange
        var productId = Guid.CreateVersion7();
        var requesterId = Guid.CreateVersion7();
        _productRepositoryMock.GetAsync(productId).Returns((Product)null!);

        //Act
        var result = await _service.DeleteAsync(productId, requesterId);

        //Assert
        result.Error.Should().Be(DomainErrors.Product.NotFound);
    }

    [Fact]
    public async Task ProductService_ShouldReturnErrorOnDelete_WhenPolicyDenies()
    {
        //Arrange
        var productId = Guid.CreateVersion7();
        var requesterId = Guid.CreateVersion7();
        _productRepositoryMock.GetAsync(productId).Returns(product);
        _productPolicyMock.IsRetrievalAllowedAsync(product, requesterId).Returns(PolicyResult.Success);
        var error = DomainErrors.ProductOwner.Deactivated;
        _productPolicyMock.IsDeleteAllowedAsync(product, requesterId).Returns(error);

        //Act
        var result = await _service.DeleteAsync(productId, requesterId);

        //Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task ProductService_ShouldReturnSuccessOnDelete_WhenPolicyAllows()
    {
        //Arrange
        var productId = Guid.CreateVersion7();
        var requesterId = Guid.CreateVersion7();
        _productRepositoryMock.GetAsync(productId).Returns(product);
        _productPolicyMock.IsRetrievalAllowedAsync(product, requesterId).Returns(PolicyResult.Success);
        _productPolicyMock.IsDeleteAllowedAsync(product, requesterId).Returns(PolicyResult.Success);

        //Act
        var result = await _service.DeleteAsync(productId, requesterId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        _productRepositoryMock.Received(1).Remove(product);
    }
}
