using FluentAssertions;
using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using ProductManagement.Application.Policies;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.UnitTests.Policies;

public class ProductPolicy_InventoryRecordCreationTests
{
    private readonly IProductPolicy _policy;

    public ProductPolicy_InventoryRecordCreationTests()
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

    private static readonly AddProductInventoryContext addInventoryContext = new(
        product.Id,
        10m,
        10m,
        Guid.CreateVersion7()
    );

    
    [Fact]
    public async Task ProductPolicy_ShouldDenyInventoryRecordCreation_WhenRequesterIsNotProductOwner()
    {
        //Arrange
        var context = addInventoryContext with { RequesterId = Guid.CreateVersion7() };

        //Act
        var result = await _policy.IsInventoryRecordCreationAllowedAsync(product, context);

        //Assert
        result.Error.Should().Be(DomainErrors.Authentication.Unauthorized);
    }

    [Fact]
    public async Task ProductPolicy_ShouldDenyInventoryRecordCreation_WhenProductOwnerIsDeactivated()
    {
        //Arrange
        var context = addInventoryContext with { RequesterId = owner.Id };
        owner.IsDeactivated = true;

        //Act
        var result = await _policy.IsInventoryRecordCreationAllowedAsync(product, context);

        //Assert
        result.Error.Should().Be(DomainErrors.ProductOwner.Deactivated);
    }

    [Fact]
    public async Task ProductPolicy_ShouldDenyInventoryRecordCreation_WhenProductOwnerIsDeleted()
    {
        //Arrange
        var context = addInventoryContext with { RequesterId = owner.Id };
        owner.IsDeactivated = false;
        owner.IsDeleted = true;

        //Act
        var result = await _policy.IsInventoryRecordCreationAllowedAsync(product, context);

        //Assert
        result.Error.Should().Be(DomainErrors.ProductOwner.Deleted);
    }

    [Fact]
    public async Task ProductPolicy_ShouldAllowInventoryRecordCreation_WhenAllRulesApply()
    {
        //Arrange
        var context = addInventoryContext with { RequesterId = owner.Id };
        owner.IsDeactivated = false;
        owner.IsDeleted = false;

        //Act
        var result = await _policy.IsInventoryRecordCreationAllowedAsync(product, context);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}
