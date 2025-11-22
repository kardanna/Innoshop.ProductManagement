using MediatR;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.Messaging;

public interface IQuery<T> : IRequest<Result<T>>
{
}