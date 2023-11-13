using System.Net;
using MediatR;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;
using static PocketStorage.Core.Application.Commands.RemovePermissionsCommandResultStatus;

namespace PocketStorage.Core.Application.Commands;

public record RemovePermissionsCommand(string? Role, Permission Permissions) : IRequest<RemovePermissionsCommandResult>;

public class RemovePermissionsCommandHandler : IRequestHandler<RemovePermissionsCommand, RemovePermissionsCommandResult>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;

    public RemovePermissionsCommandHandler(DataContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<RemovePermissionsCommandResult> Handle(RemovePermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            GetRoleQueryResult result = await _mediator.Send(new GetRoleQuery(request.Role), cancellationToken);
            switch (result.Status)
            {
                case GetRoleQueryStatus.Fail:
                    return new RemovePermissionsCommandResult(result.Error);

                case GetRoleQueryStatus.OperationCancelled:
                    return new RemovePermissionsCommandResult(OperationCancelled, result.Error);

                case GetRoleQueryStatus.InternalServerError:
                    return new RemovePermissionsCommandResult(InternalServerError, result.Error);

                case GetRoleQueryStatus.Success:
                    Role role = result.Result;

                    role.Permissions &= ~request.Permissions;

                    _context.Roles.Update(role);

                    await _context.SaveChangesAsync(cancellationToken);

                    return new RemovePermissionsCommandResult(Success, role);

                default:
                    return new RemovePermissionsCommandResult(InternalServerError, new ApiCallError(HttpStatusCode.InternalServerError, new ArgumentOutOfRangeException(nameof(result), $"Unhandled parameter '{nameof(GetRoleQueryStatus)}' value is out of range.")));
            }
        }
        catch (OperationCanceledException exception)
        {
            return new RemovePermissionsCommandResult(new ApiCallError(499, exception));
        }
        catch (Exception exception)
        {
            return new RemovePermissionsCommandResult(new ApiCallError(HttpStatusCode.InternalServerError, exception));
        }
    }
}

public class RemovePermissionsCommandResult : IRequestResult
{
    public RemovePermissionsCommandResult(RemovePermissionsCommandResultStatus status, Role? result)
    {
        Status = status;
        Result = result;
    }

    public RemovePermissionsCommandResult(RemovePermissionsCommandResultStatus status, ApiCallError? error)
    {
        Status = status;
        Error = error;
    }

    public RemovePermissionsCommandResult(ApiCallError? error)
    {
        Status = Fail;
        Error = error;
    }

    public RemovePermissionsCommandResultStatus Status { get; set; }
    public Role? Result { get; set; }
    public ApiCallError? Error { get; set; }
}

public enum RemovePermissionsCommandResultStatus
{
    Success,
    Fail,
    OperationCancelled,
    InternalServerError
}
