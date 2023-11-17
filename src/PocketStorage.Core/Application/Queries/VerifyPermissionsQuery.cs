using System.Net;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Core.Extensions;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;
using static PocketStorage.Core.Application.Queries.VerifyPermissionsQueryResultStatus;

namespace PocketStorage.Core.Application.Queries;

public class VerifyPermissionsQuery : IRequest<VerifyPermissionsQueryResult>, IEmailRequest
{
    public VerifyPermissionsQuery(Permission permissions) => Permissions = permissions;

    public Permission Permissions { get; set; }

    public string? Email { get; set; }
}

public class VerifyPermissionsQueryHandler : IRequestHandler<VerifyPermissionsQuery, VerifyPermissionsQueryResult>
{
    public static readonly Func<DataContext, IList<string>, IAsyncEnumerable<Role>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, IList<string> roles) =>
        context.Roles.AsNoTracking().Where(role => roles.Contains(role.Name)));

    private readonly DataContext _context;
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;

    public VerifyPermissionsQueryHandler(DataContext context, UserManager<User> userManager, IMediator mediator)
    {
        _context = context;
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<VerifyPermissionsQueryResult> Handle(VerifyPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            GetUserQueryResult result = await _mediator.Send(new GetUserQuery(), cancellationToken);
            if (result.Status != GetUserQueryResultStatus.Success)
            {
                return result.Status switch
                {
                    GetUserQueryResultStatus.Fail => new VerifyPermissionsQueryResult(result.Error),
                    GetUserQueryResultStatus.OperationCancelled => new VerifyPermissionsQueryResult(OperationCancelled, result.Error),
                    GetUserQueryResultStatus.InternalServerError => new VerifyPermissionsQueryResult(InternalServerError, result.Error)
                };
            }

            User user = result.GetResult();
            List<Role> roles = await CompiledQuery(_context, await _userManager.GetRolesAsync(user)).ToListAsync();

            bool authorized = roles.Any(role => (role.Permissions & request.Permissions) != 0);
            return new VerifyPermissionsQueryResult(Success, authorized);
        }
        catch (OperationCanceledException exception)
        {
            return new VerifyPermissionsQueryResult(OperationCancelled, new ApiCallError(499, exception));
        }
        catch (Exception exception)
        {
            return new VerifyPermissionsQueryResult(InternalServerError, new ApiCallError(HttpStatusCode.InternalServerError, exception));
        }
    }
}

public class VerifyPermissionsQueryResult
{
    public VerifyPermissionsQueryResult(VerifyPermissionsQueryResultStatus status, bool result)
    {
        Status = status;
        Result = result;
    }

    public VerifyPermissionsQueryResult(VerifyPermissionsQueryResultStatus status, ApiCallError? error)
    {
        Status = status;
        Error = error;
    }

    public VerifyPermissionsQueryResult(ApiCallError? error)
    {
        Status = Fail;
        Error = error;
    }

    public VerifyPermissionsQueryResultStatus Status { get; init; }
    public bool Result { get; set; }
    public ApiCallError? Error { get; set; }
}

public enum VerifyPermissionsQueryResultStatus
{
    Success,
    Fail,
    OperationCancelled,
    InternalServerError
}
