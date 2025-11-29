using FluentAssertions;
using NSubstitute;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.UnitTests.Services;

public class ProductOwnerService_GetOrCreateTests
{
    private readonly IProductOwnerRepository _productOwnerRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly IProductOwnerService _service;

    public ProductOwnerService_GetOrCreateTests()
    {
        _productOwnerRepositoryMock = Substitute.For<IProductOwnerRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _service = new ProductOwnerService(
            _productOwnerRepositoryMock,
            _unitOfWorkMock
        );
    }

    private static readonly ProductOwner owner = new()
    {
        Id = Guid.CreateVersion7()
    };

    [Fact]
    public async Task ProductOwnerService_ShouldReturnExistingOwnerOnGetOrCreate_WhenOwnerWithIdIsFound()
    {
        //Arrange
        var ownerId = owner.Id;
        _productOwnerRepositoryMock.GetAsync(ownerId).Returns(owner);

        //Act
        var result = await _service.GetOrCreateProductOwnerAsync(ownerId);

        //Assert
        result.Value.Id.Should().Be(owner.Id);
    }

    [Fact]
    public async Task ProductOwnerService_ShouldReturnNewOwnerOnGetOrCreate_WhenOwnerWithIdIsNotFound()
    {
        //Arrange
        var ownerId = owner.Id;
        _productOwnerRepositoryMock.GetAsync(ownerId).Returns((ProductOwner)null!);

        //Act
        var result = await _service.GetOrCreateProductOwnerAsync(ownerId);

        //Assert
        _productOwnerRepositoryMock.Received(1).Add(Arg.Is<ProductOwner>(po => po.Id == ownerId));
        result.Value.Id.Should().Be(owner.Id);
        result.Value.IsDeactivated.Should().BeFalse();
        result.Value.IsDeleted.Should().BeFalse();
    }
}