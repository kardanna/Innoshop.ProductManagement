using FluentAssertions;
using NSubstitute;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.Services;
using ProductManagement.Application.UseCases.Products.GetAll;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.UnitTests.Services;

public class ProductService_GetAllTests
{
    private readonly IProductRepository _productRepositoryMock;
    private readonly IProductPolicy _productPolicyMock;

    
    private readonly IProductService _service;

    public ProductService_GetAllTests()
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
    public async Task ProductService_ShouldReturnEmptyOnGetAll_WhenNoProductsReturnedByRepository()
    {
        //Arrange
        Guid? requesterId = Guid.CreateVersion7();
        _productRepositoryMock.GetAllAsync(Arg.Any<GetAllProductsQueryParameters?>()).Returns(Enumerable.Empty<Product>());
        _productPolicyMock.GetIsProductRetrievalAllowedPredicate(requesterId).Returns(p => true);

        //Act
        var result = await _service.GetAllAsync((GetAllProductsQueryParameters)null!, requesterId);

        //Assert
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ProductService_ShouldReturnEmptyOnGetAll_WhenAllProductsAreFilteredByPredicate()
    {
        //Arrange
        Guid? requesterId = Guid.CreateVersion7();
        _productRepositoryMock.GetAllAsync(Arg.Any<GetAllProductsQueryParameters?>()).Returns([ product, product, product ]);
        _productPolicyMock.GetIsProductRetrievalAllowedPredicate(requesterId).Returns(p => false);

        //Act
        var result = await _service.GetAllAsync((GetAllProductsQueryParameters)null!, requesterId);

        //Assert
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ProductService_ShouldReturnCollectionOnGetAll_WhenNotAllProductsAreFilteredOut()
    {
        //Arrange
        Guid requesterId = Guid.CreateVersion7();
        List<Product> collection = [ product, product, product ];
        _productRepositoryMock.GetAllAsync(Arg.Any<GetAllProductsQueryParameters?>()).Returns(collection);
        _productPolicyMock.GetIsProductRetrievalAllowedPredicate(requesterId).Returns(p => true);

        //Act
        var result = await _service.GetAllAsync((GetAllProductsQueryParameters)null!, requesterId);

        //Assert
        result.Value.Should().NotBeEmpty();
        result.Value.Should().BeSubsetOf(collection);
    }
}
