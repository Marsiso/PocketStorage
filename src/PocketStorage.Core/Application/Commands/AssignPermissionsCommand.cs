using CommunityToolkit.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;

namespace PocketStorage.Core.Application.Commands;

public record AssignPermissionsCommand(string? RoleName, Permission Permissions) : IRequest<AssignPermissionsCommandResult>;

public class AssignPermissionsCommandHandler : IRequestHandler<AssignPermissionsCommand, AssignPermissionsCommandResult>
{
    private readonly DataContext _databaseContext;
    private readonly ILogger<AssignPermissionsCommandHandler> _logger;
    private readonly IMediator _mediator;

    public AssignPermissionsCommandHandler(DataContext databaseContext, IMediator mediator, ILogger<AssignPermissionsCommandHandler> logger)
    {
        _databaseContext = databaseContext;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<AssignPermissionsCommandResult> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            GetRoleWithNameQuery query = new(request.RoleName);
            GetRoleWithNameQueryResult queryResult = await _mediator.Send(query, cancellationToken);

            if (queryResult.ResultType == GetRoleWithNameQueryResultType.RoleNotFound)
            {
                return new AssignPermissionsCommandResult(AssignPermissionsCommandResultType.PermissionsNotAssigned, default, queryResult.Exception);
            }

            if (queryResult.ResultType == GetRoleWithNameQueryResultType.OperationCancelled)
            {
                return new AssignPermissionsCommandResult(AssignPermissionsCommandResultType.OperationCancelled, default, queryResult.Exception);
            }

            if (queryResult.ResultType != GetRoleWithNameQueryResultType.RoleFound)
            {
                return new AssignPermissionsCommandResult(AssignPermissionsCommandResultType.InternalServerError, default, queryResult.Exception);
            }

            Role originalRole = queryResult.GetResult();

            originalRole.Permissions |= request.Permissions;

            _databaseContext.Roles.Update(originalRole);

            await _databaseContext.SaveChangesAsync(cancellationToken);

            return new AssignPermissionsCommandResult(AssignPermissionsCommandResultType.PermissionsAssigned, originalRole, default);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.ToString());

            return new AssignPermissionsCommandResult(AssignPermissionsCommandResultType.InternalServerError, default, exception);
        }
    }
}

public record AssignPermissionsCommandResult(AssignPermissionsCommandResultType ResultType, Role? Result, Exception? Exception)
{
    public Role GetResult()
    {
        Guard.IsNotNull(Result);

        return Result;
    }

    public Exception GetException()
    {
        Guard.IsNotNull(Exception);

        return Exception;
    }
}

public enum AssignPermissionsCommandResultType
{
    PermissionsAssigned,
    PermissionsNotAssigned,
    OperationCancelled,
    InternalServerError
}
