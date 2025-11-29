using FluentAssertions;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using ProductManagement.Application.Policies;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.UnitTests.Policies;

public class ProductPolicy_RetrievalTests
{
    private readonly IProductPolicy _policy;

    public ProductPolicy_RetrievalTests()
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
    public async Task ProductPolicy_ShouldDenyRetrieval_WhenProductOwnerIsDeactivatedAndRequesterIsNotSpecified()
    {
        //Arrange
        owner.IsDeactivated = true;

        //Act
        var result = await _policy.IsRetrievalAllowedAsync(product, null);

        //Assert
        result.Error.Should().Be(DomainErrors.Product.NotFound);
    }

    [Fact]
    public async Task ProductPolicy_ShouldDenyRetrieval_WhenProductOwnerIsDeactivatedAndRequesterIsNotProductOwner()
    {
        //Arrange
        owner.IsDeactivated = true;

        //Act
        var result = await _policy.IsRetrievalAllowedAsync(product, Guid.CreateVersion7());

        //Assert
        result.Error.Should().Be(DomainErrors.Product.NotFound);
    }

    [Fact]
    public async Task ProductPolicy_ShouldAllowRetrieval_WhenProductOwnerIsDeactivatedAndRequesterIsProductOwner()
    {
        //Arrange
        owner.IsDeactivated = true;

        //Act
        var result = await _policy.IsRetrievalAllowedAsync(product, owner.Id);

        //Assert
        result.Should().Be(PolicyResult.Success);
    }
}
