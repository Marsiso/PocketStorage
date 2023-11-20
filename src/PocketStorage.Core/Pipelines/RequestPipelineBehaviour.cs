using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using PocketStorage.Domain.Contracts;

namespace PocketStorage.Core.Pipelines;

public class RequestPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : class
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestPipelineBehaviour(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ClaimsPrincipal? claimsPrincipal = _httpContextAccessor.HttpContext?.User;

        if (request is IEmailRequest requestWithEmail)
        {
            requestWithEmail.Email = claimsPrincipal?.FindFirst(ClaimTypes.Email)?.Value;
        }

        return await next();
    }
}
