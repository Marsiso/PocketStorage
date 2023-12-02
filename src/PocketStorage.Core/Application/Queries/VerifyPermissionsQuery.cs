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
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Queries;

public class VerifyPermissionsQuery(Permission permissions) : IRequest<VerifyPermissionsQueryResult>, IEmailRequest
{
    public Permission Permissions { get; set; } = permissions;
    public string? Email { get; set; }
}

public class VerifyPermissionsQueryHandler(DataContext context, UserManager<User> userManager, ISender sender) : IRequestHandler<VerifyPermissionsQuery, VerifyPermissionsQueryResult>
{
    public static readonly Func<DataContext, IList<string>, IAsyncEnumerable<Role>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, IList<string> roles) =>
        context.Roles.AsNoTracking().Where(role => roles.Contains(role.Name)));

    public async Task<VerifyPermissionsQueryResult> Handle(VerifyPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            GetUserQueryResult result = await sender.Send(new GetUserQuery(), cancellationToken);
            if (result is not { Status: Success, Result: not null })
            {
                return new VerifyPermissionsQueryResult(result.Status, result.Error);
            }

            List<Role> roles = await CompiledQuery(context, await userManager.GetRolesAsync(result.Result)).ToListAsync();
            return new VerifyPermissionsQueryResult(roles.Any(role => (role.Permissions & request.Permissions) != 0));
        }
        catch (OperationCanceledException exception)
        {
            return new VerifyPermissionsQueryResult(Cancelled, new RequestError(Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new VerifyPermissionsQueryResult(Error, new RequestError(Cancelled, "Request interrupted by server.", exception));
        }
    }
}

public class VerifyPermissionsQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public VerifyPermissionsQueryResult(bool result) : this(Success, null) => Result = result;

    public bool Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
