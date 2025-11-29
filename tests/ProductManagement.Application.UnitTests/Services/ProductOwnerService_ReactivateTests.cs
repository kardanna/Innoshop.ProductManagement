using FluentAssertions;
using NSubstitute;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.UnitTests.Services;

public class ProductOwnerService_ReactivateTests
{
    private readonly IProductOwnerRepository _productOwnerRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly IProductOwnerService _service;

    public ProductOwnerService_ReactivateTests()
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
    public async Task ProductOwnerService_ShouldReturnErrorOnReactivate_WhenOwnerIsDeleted()
    {
        //Arrange
        var ownerId = owner.Id;
        owner.IsDeleted = true;
        _productOwnerRepositoryMock.GetAsync(ownerId).Returns(owner);

        //Act
        var result = await _service.ReactivateProductOwnerAsync(ownerId);

        //Assert
        result.Error.Should().Be(DomainErrors.ProductOwner.Deleted);
    }

    [Fact]
    public async Task ProductOwnerService_ShouldReturnSuccessAndSaveChangesOnReactivate_WhenOwnerIsDeactivated()
    {
        //Arrange
        var ownerId = owner.Id;
        owner.IsDeleted = false;
        owner.IsDeactivated = true;
        _productOwnerRepositoryMock.GetAsync(ownerId).Returns(owner);

        //Act
        var result = await _service.ReactivateProductOwnerAsync(ownerId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        owner.IsDeactivated.Should().BeFalse();
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ProductOwnerService_ShouldReturnSuccessAndDontSaveChangesOnReactivate_WhenIsNotDeactivated()
    {
        //Arrange
        var ownerId = owner.Id;
        owner.IsDeleted = false;
        owner.IsDeactivated = false;
        _productOwnerRepositoryMock.GetAsync(ownerId).Returns(owner);

        //Act
        var result = await _service.ReactivateProductOwnerAsync(ownerId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        owner.IsDeactivated.Should().BeFalse();
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }
}