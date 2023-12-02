using MediatR;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Data;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Commands;

public record AssignPermissionsCommand(string? Role, Permission Permissions) : IRequest<AssignPermissionsCommandResult>;

public class AssignPermissionsCommandHandler(DataContext context, ISender sender) : IRequestHandler<AssignPermissionsCommand, AssignPermissionsCommandResult>
{
    public async Task<AssignPermissionsCommandResult> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            GetRoleQueryResult result = await sender.Send(new GetRoleQuery(request.Role), cancellationToken);
            if (result is not { Status: Success, Result: not null })
            {
                return new AssignPermissionsCommandResult(result.Status, result.Error);
            }

            result.Result.Permissions |= request.Permissions;
            context.Roles.Update(result.Result);

            await context.SaveChangesAsync(cancellationToken);
            return new AssignPermissionsCommandResult(true);
        }
        catch (OperationCanceledException exception)
        {
            return new AssignPermissionsCommandResult(Cancelled, new RequestError(Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new AssignPermissionsCommandResult(Error, new RequestError(Error, "Request interrupted by server.", exception));
        }
    }
}

public class AssignPermissionsCommandResult(RequestStatus status, RequestError error) : IRequestResult
{
    public AssignPermissionsCommandResult(bool result) : this(Success, null) => Result = result;

    public bool Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
