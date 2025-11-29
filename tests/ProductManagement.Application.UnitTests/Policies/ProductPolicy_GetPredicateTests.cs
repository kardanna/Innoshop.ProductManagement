using FluentAssertions;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Policies;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.UnitTests.Policies;

public class ProductPolicy_GetPredicateTests
{
    private readonly IProductPolicy _policy;

    public ProductPolicy_GetPredicateTests()
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
    public async Task ProductPolicy_ShouldReturnPredicateOnGetIsProductRetrievalAllowedPredicate_WhenNotProvidedWithGuid()
    {
        //Arrange
        Guid? requesterId = null;

        //Act
        var predicate = _policy.GetIsProductRetrievalAllowedPredicate(requesterId);

        //Assert
        predicate.Should().BeOfType<Func<Product, bool>>();
    }

    [Fact]
    public async Task ProductPolicy_IsProductRetrievalAllowedPredicateShouldReturnFalse_WhenProductOwnerIsDeactivatedAndRequesterIsNotProductOwner()
    {
        //Arrange
        Guid? requesterId = Guid.CreateVersion7();
        owner.IsDeactivated = true;

        //Act
        var predicate = _policy.GetIsProductRetrievalAllowedPredicate(requesterId);

        //Assert
        predicate(product).Should().BeFalse();
    }

    [Fact]
    public async Task ProductPolicy_IsProductRetrievalAllowedPredicateShouldReturnTrue_WhenProductOwnerIsDeactivatedAndRequesterIsProductOwner()
    {
        //Arrange
        Guid? requesterId = Guid.CreateVersion7();
        owner.IsDeactivated = true;

        //Act
        var predicate = _policy.GetIsProductRetrievalAllowedPredicate(owner.Id);

        //Assert
        predicate(product).Should().BeTrue();
    }

    [Fact]
    public async Task ProductPolicy_IsProductRetrievalAllowedPredicateShouldReturnTrue_WhenProductOwnerIsNotDeactivated()
    {
        //Arrange
        Guid? requesterId = Guid.CreateVersion7();
        owner.IsDeactivated = false;

        //Act
        var predicate = _policy.GetIsProductRetrievalAllowedPredicate(requesterId);

        //Assert
        predicate(product).Should().BeTrue();
    }
}
