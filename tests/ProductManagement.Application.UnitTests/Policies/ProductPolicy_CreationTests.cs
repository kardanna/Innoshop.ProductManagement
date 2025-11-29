using FluentAssertions;
using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using ProductManagement.Application.Policies;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.UnitTests.Policies;

public class ProductPolicy_CreationTests
{
    private readonly IProductPolicy _policy;

    public ProductPolicy_CreationTests()
    {
        _policy = new ProductPolicy();
    }

    private static readonly ProductOwner owner = new()
    {
        IsDeactivated = false,
        IsDeleted = false
    };

    private static readonly AddProductContext context = new(
        owner,
        "name",
        "descritpiton",
        "measuremnentUnit"
    );

    [Fact]
    public async Task ProductPolicy_ShouldDenyCreation_WhenOwnerIsDeactivated()
    {
        //Arrange
        owner.IsDeactivated = true;

        //Act
        var result = await _policy.IsCreationAllowedAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.ProductOwner.Deactivated);
    }

    [Fact]
    public async Task ProductPolicy_ShouldDenyCreation_WhenOwnerIsDeleted()
    {
        //Arrange
        owner.IsDeactivated = false;
        owner.IsDeleted = true;

        //Act
        var result = await _policy.IsCreationAllowedAsync(context);

        //Assert
        result.Error.Should().Be(DomainErrors.ProductOwner.Deleted);
    }

    [Fact]
    public async Task ProductPolicy_ShouldAllowCreation_WhenAllRulesApply()
    {
        //Arrange
        owner.IsDeactivated = false;
        owner.IsDeleted = false;

        //Act
        var result = await _policy.IsCreationAllowedAsync(context);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}
