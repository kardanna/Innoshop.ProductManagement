using MediatR;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}