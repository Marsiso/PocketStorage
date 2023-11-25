using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;

namespace PocketStorage.Core.Authentication.Queries;

public class GetUserInfoQuery : IRequest<ApiCallResponse<UserInfo>>
{
}

public class GetUserInfoQueryHandler(ISender sender, IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetUserInfoQuery, ApiCallResponse<UserInfo>>
{
    public async Task<ApiCallResponse<UserInfo>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext is not { User.Identity.IsAuthenticated: true })
        {
            return new ApiCallResponse<UserInfo>(RequestStatus.Success, UserInfo.Anonymous, null);
        }

        ApiCallResponse<User> result = await sender.Send(new GetUserQuery(), cancellationToken);
        if (result is { Status: RequestStatus.EntityNotFound or RequestStatus.Fail or RequestStatus.Cancelled or RequestStatus.Error })
        {
            return new ApiCallResponse<UserInfo>(result.Status, null, result.Error);
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
            return new ApiCallResponse<UserInfo>(RequestStatus.Success, userinfo, null);
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

        return new ApiCallResponse<UserInfo>(RequestStatus.Success, userinfo, null);
    }
}
