using ProductManagement.Domain.Shared;

namespace ProductManagement.Domain.Errors;

public static class DomainErrors
{
    public static class Product
    {
        public static readonly Error NotFound = new(
            "Product.NotFound",
            "Product not found."
        );
    }

    public static class MeasurementUnit
    {
        public static readonly Error UnknownUnit = new(
            "MeasurementUnit.UnknownUnit",
            "Unknown measurment unit specified."
        );
    }

    public static class ProductOwner
    {
        public static readonly Error Deactivated = new(
            "ProductOwner.Deactivated",
            "Product owner is deactivated."
        );

        public static readonly Error Deleted = new(
            "ProductOwner.Deleted",
            "Product owner has been deleted."
        );
    }

    public static class Authentication
    {
        public static readonly Error InvalidSubjectClaim = new(
            "Authentication.InvalidSubjectClaim",
            "Token has no subject claim or claim value is invalid."
        );

        public static readonly Error InvalidJwtIdClaim = new(
            "Authentication.InvalidJwtIdClaim",
            "Token has no token ID claim or claim value is invalid."
        );
    }
}