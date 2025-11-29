using FluentAssertions;
using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using ProductManagement.Application.Policies;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.UnitTests.Policies;

public class ProductPolicy_DeleteTests
{
    private readonly IProductPolicy _policy;

    public ProductPolicy_DeleteTests()
    {
        _policy = new ProductPolicy();
    }

    private static readonly ProductOwner owner = new()
    {
        Id = Guid.CreateVersion7(),
        IsDeactivated = false,
        IsDeleted = false
    };

    private static readonly Product product = new()
    {
        Id = Guid.CreateVersion7(),
        Name = "product",
        Description = "description",
        MeasurementUnit = MeasurementUnit.Meter,
        Owner = owner,
        OwnerId = owner.Id
    };

    
    [Fact]
    public async Task ProductPolicy_ShouldDenyDelete_WhenRequesterIsNotProductOwner()
    {
        //Arrange
        var requesterId = Guid.CreateVersion7();

        //Act
        var result = await _policy.IsDeleteAllowedAsync(product, requesterId);

        //Assert
        result.Error.Should().Be(DomainErrors.Authentication.Unauthorized);
    }

    [Fact]
    public async Task ProductPolicy_ShouldDenyDelete_WhenProductOwnerIsDeactivated()
    {
        //Arrange
        var requesterId = owner.Id;
        owner.IsDeactivated = true;

        //Act
        var result = await _policy.IsDeleteAllowedAsync(product, requesterId);

        //Assert
        result.Error.Should().Be(DomainErrors.ProductOwner.Deactivated);
    }

    [Fact]
    public async Task ProductPolicy_ShouldDenyDelete_WhenProductOwnerIsDeleted()
    {
        //Arrange
        var requesterId = owner.Id;
        owner.IsDeactivated = false;
        owner.IsDeleted = true;

        //Act
        var result = await _policy.IsDeleteAllowedAsync(product, requesterId);

        //Assert
        result.Error.Should().Be(DomainErrors.ProductOwner.Deleted);
    }

    [Fact]
    public async Task ProductPolicy_ShouldAllowDelete_WhenAllRulesApply()
    {
        //Arrange
        var requesterId = owner.Id;
        owner.IsDeactivated = false;
        owner.IsDeleted = false;

        //Act
        var result = await _policy.IsDeleteAllowedAsync(product, requesterId);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}
