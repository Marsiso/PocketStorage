using System.Net;
using MediatR;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;
using static PocketStorage.Core.Application.Commands.AssignPermissionsCommandResultStatus;

namespace PocketStorage.Core.Application.Commands;

public record AssignPermissionsCommand(string? Role, Permission Permissions) : IRequest<AssignPermissionsCommandResult>;

public class AssignPermissionsCommandHandler : IRequestHandler<AssignPermissionsCommand, AssignPermissionsCommandResult>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;

    public AssignPermissionsCommandHandler(DataContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<AssignPermissionsCommandResult> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            GetRoleQueryResult result = await _mediator.Send(new GetRoleQuery(request.Role), cancellationToken);

            switch (result.Status)
            {
                case GetRoleQueryStatus.Fail:
                    return new AssignPermissionsCommandResult(Fail, result.Error);

                case GetRoleQueryStatus.OperationCancelled:
                    return new AssignPermissionsCommandResult(OperationCancelled, result.Error);

                case GetRoleQueryStatus.InternalServerError:
                    return new AssignPermissionsCommandResult(InternalServerError, result.Error);

                case GetRoleQueryStatus.Success:
                    Role role = result.Result;

                    role.Permissions |= request.Permissions;

                    _context.Roles.Update(role);

                    await _context.SaveChangesAsync(cancellationToken);

                    return new AssignPermissionsCommandResult(Success, new ApiCallError(HttpStatusCode.InternalServerError, new ArgumentOutOfRangeException(nameof(result), $"Unhandled parameter '{nameof(GetRoleQueryStatus)}' value is out of range.")));

                default:
                    return new AssignPermissionsCommandResult(InternalServerError, result.Error);
            }
        }
        catch (OperationCanceledException exception)
        {
            return new AssignPermissionsCommandResult(OperationCancelled, new ApiCallError(499, exception));
        }
        catch (Exception exception)
        {
            return new AssignPermissionsCommandResult(InternalServerError, new ApiCallError(HttpStatusCode.InternalServerError, exception));
        }
    }
}

public class AssignPermissionsCommandResult : IRequestResult
{
    public AssignPermissionsCommandResult(AssignPermissionsCommandResultStatus status, Role? result)
    {
        Status = status;
        Result = result;
    }

    public AssignPermissionsCommandResult(AssignPermissionsCommandResultStatus status, ApiCallError? error)
    {
        Status = Fail;
        Error = error;
    }

    public AssignPermissionsCommandResult(ApiCallError? error)
    {
        Status = Fail;
        Error = error;
    }

    public AssignPermissionsCommandResultStatus Status { get; set; }
    public Role? Result { get; set; }
    public ApiCallError? Error { get; set; }
}

public enum AssignPermissionsCommandResultStatus
{
    Success,
    Fail,
    OperationCancelled,
    InternalServerError
}
