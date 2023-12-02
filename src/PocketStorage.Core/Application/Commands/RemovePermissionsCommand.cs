using MediatR;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Data;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Commands;

public record RemovePermissionsCommand(string? Role, Permission Permissions) : IRequest<RemovePermissionsCommandResult>;

public class RemovePermissionsCommandHandler(DataContext context, ISender sender) : IRequestHandler<RemovePermissionsCommand, RemovePermissionsCommandResult>
{
    public async Task<RemovePermissionsCommandResult> Handle(RemovePermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            GetRoleQueryResult result = await sender.Send(new GetRoleQuery(request.Role), cancellationToken);
            if (result is not { Status: Success, Result: not null })
            {
                return new RemovePermissionsCommandResult(result.Status, result.Error);
            }

            result.Result.Permissions &= ~request.Permissions;
            context.Roles.Update(result.Result);

            await context.SaveChangesAsync(cancellationToken);
            return new RemovePermissionsCommandResult(true);
        }
        catch (OperationCanceledException exception)
        {
            return new RemovePermissionsCommandResult(Cancelled, new RequestError(Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new RemovePermissionsCommandResult(Error, new RequestError(Error, "Request interrupted by server.", exception));
        }
    }
}

public class RemovePermissionsCommandResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public RemovePermissionsCommandResult(bool result) : this(Success, null) => Result = result;

    public bool Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
