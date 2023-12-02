using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;

namespace PocketStorage.Core.Authentication.Queries;

public class GetUserInfoQuery : IRequest<GetUserInfoQueryResult>
{
}

public class GetUserInfoQueryHandler(ISender sender, IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetUserInfoQuery, GetUserInfoQueryResult>
{
    public async Task<GetUserInfoQueryResult> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext is not { User.Identity.IsAuthenticated: true })
        {
            return new GetUserInfoQueryResult(UserInfo.Anonymous);
        }

        GetUserQueryResult result = await sender.Send(new GetUserQuery(), cancellationToken);
        if (result is not { Status: RequestStatus.Success, Result: not null })
        {
            return new GetUserInfoQueryResult(result.Status, result.Error);
        }

        UserInfo userinfo = new() { IsAuthenticated = true };
        if (httpContextAccessor.HttpContext.User.Identity is ClaimsIdentity identity)
        {
            userinfo.NameClaimType = identity.NameClaimType;
            userinfo.RoleClaimType = identity.RoleClaimType;
        }
        else
        {
            userinfo.NameClaimType = ClaimTypes.Name;
            userinfo.RoleClaimType = ClaimTypes.Role;
        }

        if (!httpContextAccessor.HttpContext.User.Claims.Any())
        {
            return new GetUserInfoQueryResult(userinfo);
        }

        userinfo.Claims = new List<ClaimValue>();
        foreach (Claim claim in httpContextAccessor.HttpContext.User.Claims)
        {
            if (claim.Type == OpenIddictConstants.Claims.UpdatedAt)
            {
                continue;
            }

            userinfo.Claims.Add(new ClaimValue(claim.Type, claim.Value));
        }

        return new GetUserInfoQueryResult(userinfo);
    }
}

public class GetUserInfoQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public GetUserInfoQueryResult(UserInfo? result) : this(RequestStatus.Success, null) => Result = result;

    public UserInfo? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
