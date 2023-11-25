using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.Core.Extensions;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;

namespace PocketStorage.Core.Application.Queries;

public class VerifyPermissionsQuery(Permission permissions) : IRequest<ApiCallResponse<bool>>, IEmailRequest
{
    public Permission Permissions { get; set; } = permissions;
    public string? Email { get; set; }
}

public class VerifyPermissionsQueryHandler(DataContext context, UserManager<User> userManager, ISender sender) : IRequestHandler<VerifyPermissionsQuery, ApiCallResponse<bool>>
{
    public static readonly Func<DataContext, IList<string>, IAsyncEnumerable<Role>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, IList<string> roles) =>
        context.Roles.AsNoTracking().Where(role => roles.Contains(role.Name)));

    public async Task<ApiCallResponse<bool>> Handle(VerifyPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            ApiCallResponse<User> result = await sender.Send(new GetUserQuery(), cancellationToken);
            if (result is { Status: RequestStatus.EntityNotFound or RequestStatus.Fail or RequestStatus.Cancelled or RequestStatus.Error })
            {
                return new ApiCallResponse<bool>(result.Status, false, result.Error);
            }

            List<Role> roles = await CompiledQuery(context, await userManager.GetRolesAsync(result.Result)).ToListAsync();
            return new ApiCallResponse<bool>(RequestStatus.Success, roles.Any(role => (role.Permissions & request.Permissions) != 0), null);
        }
        catch (OperationCanceledException exception)
        {
            return new ApiCallResponse<bool>(RequestStatus.Cancelled, false, new ApiCallError(RequestStatus.Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new ApiCallResponse<bool>(RequestStatus.Error, false, new ApiCallError(RequestStatus.Cancelled, "Request interrupted by server.", exception));
        }
    }
}
