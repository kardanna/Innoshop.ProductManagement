using FluentAssertions;
using NSubstitute;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.UnitTests.Services;

public class ProductOwnerService_DeactivateTests
{
    private readonly IProductOwnerRepository _productOwnerRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly IProductOwnerService _service;

    public ProductOwnerService_DeactivateTests()
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
    public async Task ProductOwnerService_ShouldReturnErrorOnDeactivate_WhenOwnerIsDeleted()
    {
        //Arrange
        var ownerId = owner.Id;
        owner.IsDeleted = true;
        _productOwnerRepositoryMock.GetAsync(ownerId).Returns(owner);

        //Act
        var result = await _service.DeactivateProductOwnerAsync(ownerId);

        //Assert
        result.Error.Should().Be(DomainErrors.ProductOwner.Deleted);
    }

    [Fact]
    public async Task ProductOwnerService_ShouldReturnSuccessAndSaveChangesOnDeactivate_WhenOwnerWasNotDeactivatedBefore()
    {
        //Arrange
        var ownerId = owner.Id;
        owner.IsDeleted = false;
        owner.IsDeactivated = false;
        _productOwnerRepositoryMock.GetAsync(ownerId).Returns(owner);

        //Act
        var result = await _service.DeactivateProductOwnerAsync(ownerId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        owner.IsDeactivated.Should().BeTrue();
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ProductOwnerService_ShouldReturnSuccessAndDontSaveChangesOnDeactivate_WhenOwnerHasAlreadyBeenDeactivated()
    {
        //Arrange
        var ownerId = owner.Id;
        owner.IsDeleted = false;
        owner.IsDeactivated = true;
        _productOwnerRepositoryMock.GetAsync(ownerId).Returns(owner);

        //Act
        var result = await _service.DeactivateProductOwnerAsync(ownerId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        owner.IsDeactivated.Should().BeTrue();
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }
}