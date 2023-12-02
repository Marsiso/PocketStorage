using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Core.Extensions;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Queries;

public class FindUserRolesQuery(string? userId) : IRequest<FindUserRolesQueryResult>
{
    public string? UserId { get; set; } = userId;
}

public class FindUserRolesQueryHandler(DataContext context, ISender sender) : IRequestHandler<FindUserRolesQuery, FindUserRolesQueryResult>
{
    public static readonly Func<DataContext, string, IAsyncEnumerable<Role>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string userId) => context.UserRoles.AsNoTracking()
        .Where(userRole => userRole.UserId == userId)
        .Join(context.Roles, userRole => userRole.RoleId, role => role.Id, (userRole, role) => role));

    public async Task<FindUserRolesQueryResult> Handle(FindUserRolesQuery request, CancellationToken cancellationToken)
    {
        if (IsNullOrWhiteSpace(request.UserId))
        {
            return new FindUserRolesQueryResult(Unauthorized, new RequestError());
        }

        List<Role> roles = await CompiledQuery(context, request.UserId).ToListAsync();
        if (roles.Count > 0)
        {
            return new FindUserRolesQueryResult(roles);
        }

        return new FindUserRolesQueryResult(EntityNotFound, new RequestError(EntityNotFound, "The user with the given ID doesn't have any roles assigned.", new EntityNotFoundException(request.UserId, nameof(User))));
    }
}

public class FindUserRolesQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public FindUserRolesQueryResult(IEnumerable<Role>? result) : this(Success, null) => Result = result;

    public IEnumerable<Role>? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
