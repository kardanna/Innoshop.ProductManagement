using FluentAssertions;
using NSubstitute;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.UnitTests.Services;

public class ProductOwnerService_DeleteTests
{
    private readonly IProductOwnerRepository _productOwnerRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly IProductOwnerService _service;

    public ProductOwnerService_DeleteTests()
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
    public async Task ProductOwnerService_ShouldReturnSuccessAndSaveChangesOnDelete_WhenOwnerIsNotDeleted()
    {
        //Arrange
        var ownerId = owner.Id;
        owner.IsDeleted = false;
        _productOwnerRepositoryMock.GetAsync(ownerId).Returns(owner);

        //Act
        var result = await _service.DeleteProductOwnerAsync(ownerId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        owner.IsDeleted.Should().BeTrue();
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ProductOwnerService_ShouldReturnSuccessAndDontSaveChangesOnDelete_WhenOwnerHasAlreadyBeenDeleted()
    {
        //Arrange
        var ownerId = owner.Id;
        owner.IsDeleted = true;
        _productOwnerRepositoryMock.GetAsync(ownerId).Returns(owner);

        //Act
        var result = await _service.DeleteProductOwnerAsync(ownerId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        owner.IsDeleted.Should().BeTrue();
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }
}