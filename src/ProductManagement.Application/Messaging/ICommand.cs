using MediatR;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.Messaging;

public interface ICommand<T> : IRequest<Result<T>>
{
}

public interface ICommand : IRequest<Result>
{
}