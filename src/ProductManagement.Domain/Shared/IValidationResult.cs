namespace ProductManagement.Domain.Shared;

public interface IValidationResult
{
    Error[] Errors { get; }
}