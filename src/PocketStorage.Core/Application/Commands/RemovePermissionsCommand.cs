using CommunityToolkit.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;

namespace PocketStorage.Core.Application.Commands;

public record RemovePermissionsCommand(string? RoleName, Permission Permissions) : IRequest<RemovePermissionsCommandResult>;

public class RemovePermissionsCommandHandler : IRequestHandler<RemovePermissionsCommand, RemovePermissionsCommandResult>
{
    private readonly DataContext _databaseContext;
    private readonly ILogger<RemovePermissionsCommandHandler> _logger;
    private readonly IMediator _mediator;

    public RemovePermissionsCommandHandler(DataContext databaseContext, IMediator mediator, ILogger<RemovePermissionsCommandHandler> logger)
    {
        _databaseContext = databaseContext;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<RemovePermissionsCommandResult> Handle(RemovePermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            GetRoleWithNameQuery query = new(request.RoleName);
            GetRoleWithNameQueryResult queryResult = await _mediator.Send(query, cancellationToken);

            if (queryResult.ResultType == GetRoleWithNameQueryResultType.RoleNotFound)
            {
                return new RemovePermissionsCommandResult(RemovePermissionsCommandResultType.PermissionsNotRemoved, default, queryResult.Exception);
            }

            if (queryResult.ResultType == GetRoleWithNameQueryResultType.OperationCancelled)
            {
                return new RemovePermissionsCommandResult(RemovePermissionsCommandResultType.OperationCancelled, default, queryResult.Exception);
            }

            if (queryResult.ResultType != GetRoleWithNameQueryResultType.RoleFound)
            {
                return new RemovePermissionsCommandResult(RemovePermissionsCommandResultType.InternalServerError, default, queryResult.Exception);
            }

            Role originalRole = queryResult.GetResult();

            Permission permission = ~request.Permissions;

            originalRole.Permissions &= permission;

            _databaseContext.Roles.Update(originalRole);

            await _databaseContext.SaveChangesAsync(cancellationToken);

            return new RemovePermissionsCommandResult(RemovePermissionsCommandResultType.PermissionsRemoved, originalRole, default);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.ToString());

            return new RemovePermissionsCommandResult(RemovePermissionsCommandResultType.InternalServerError, default, exception);
        }
    }
}

public record RemovePermissionsCommandResult(RemovePermissionsCommandResultType ResultType, Role? Result, Exception? Exception)
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

public enum RemovePermissionsCommandResultType
{
    PermissionsRemoved,
    PermissionsNotRemoved,
    OperationCancelled,
    InternalServerError
}
