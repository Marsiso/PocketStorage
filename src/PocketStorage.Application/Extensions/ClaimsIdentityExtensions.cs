using System.Collections.Immutable;
using System.Globalization;
using System.Security.Claims;
using CommunityToolkit.Diagnostics;
using MediatR;
using OpenIddict.Abstractions;
using PocketStorage.BFF.Authorization.Constants;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.BFF.Authorization.Helpers;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Enums;
using static System.String;
using static OpenIddict.Abstractions.OpenIddictConstants.Claims;

namespace PocketStorage.Application.Extensions;

public static class ClaimsIdentityExtensions
{
    public static async Task<ClaimsIdentity> SetClaims(this ClaimsIdentity identity, string? email, ISender sender, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(sender);
        Guard.IsNotNull(identity);

        FindUserQueryResult userResult = await sender.Send(new FindUserQuery(email), cancellationToken);
        if (userResult is not { Status: RequestStatus.Success, Result: not null })
        {
            return identity;
        }

        identity.SetClaim(Subject, userResult.Result.Id);
        identity.SetClaim(Email, userResult.Result.Email);
        identity.SetClaim(EmailVerified, userResult.Result.EmailConfirmed);

        if (!IsNullOrWhiteSpace(userResult.Result.PhoneNumber))
        {
            identity.SetClaim(PhoneNumber, userResult.Result.PhoneNumber);
            identity.SetClaim(PhoneNumberVerified, userResult.Result.PhoneNumberConfirmed);
        }

        identity.SetClaim(Name, userResult.Result.Email);
        identity.SetClaim(Username, userResult.Result.UserName);
        identity.SetClaim(GivenName, userResult.Result.GivenName);
        identity.SetClaim(MiddleName, Empty);
        identity.SetClaim(FamilyName, userResult.Result.FamilyName);
        identity.SetClaim(Locale, userResult.Result.Culture);
        identity.SetClaim(Zoneinfo, TimeZoneInfo.Local.DisplayName);
        identity.SetClaim(UpdatedAt, userResult.Result.DateUpdated.ToString(new CultureInfo(userResult.Result.Culture)));

        FindUserRolesQueryResult rolesResult = await sender.Send(new FindUserRolesQuery(userResult.Result.Id), cancellationToken);
        if (rolesResult is not { Status: RequestStatus.Success, Result: not null })
        {
            return identity;
        }

        Permission permissions = rolesResult.Result.Select(role => role.Permissions).Aggregate(Permission.None, (accumulator, permission) => accumulator | permission);
        identity.SetClaim(PermitConstants.Claims.Permissions, PolicyNameHelpers.GetPolicyNameFor(permissions));

        identity.SetClaims(Role, rolesResult.Result.Select(role => role.Name).ToImmutableArray());

        return identity;
    }
}
