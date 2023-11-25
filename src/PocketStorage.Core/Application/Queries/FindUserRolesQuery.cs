using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Core.Extensions;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public class FindUserRolesQuery(string? userId) : IRequest<ApiCallResponse<IEnumerable<Role>>>
{
    public string? UserId { get; set; } = userId;
}

public class FindUserRolesQueryHandler(DataContext context, ISender sender) : IRequestHandler<FindUserRolesQuery, ApiCallResponse<IEnumerable<Role>>>
{
    public static readonly Func<DataContext, string, IAsyncEnumerable<Role>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string userId) => context.UserRoles.AsNoTracking()
        .Where(userRole => userRole.UserId == userId)
        .Join(context.Roles, userRole => userRole.RoleId, role => role.Id, (userRole, role) => role));

    public async Task<ApiCallResponse<IEnumerable<Role>>> Handle(FindUserRolesQuery request, CancellationToken cancellationToken)
    {
        if (IsNullOrWhiteSpace(request.UserId))
        {
            return new ApiCallResponse<IEnumerable<Role>>(RequestStatus.Unauthorized, null, new ApiCallError());
        }

        List<Role> roles = await CompiledQuery(context, request.UserId).ToListAsync();
        if (roles.Count > 0)
        {
            return new ApiCallResponse<IEnumerable<Role>>(RequestStatus.Success, roles, null);
        }

        return new ApiCallResponse<IEnumerable<Role>>(RequestStatus.EntityNotFound, roles, new ApiCallError(RequestStatus.EntityNotFound, "The user with the given ID doesn't have any roles assigned.", new EntityNotFoundException(request.UserId, nameof(User))));
    }
}
