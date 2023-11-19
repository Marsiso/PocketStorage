using MediatR;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;

namespace PocketStorage.Core.Application.Commands;

public record AssignPermissionsCommand(string? Role, Permission Permissions) : IRequest<ApiCallResponse<bool>>;

public class AssignPermissionsCommandHandler(DataContext context, ISender sender) : IRequestHandler<AssignPermissionsCommand, ApiCallResponse<bool>>
{
    public async Task<ApiCallResponse<bool>> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            ApiCallResponse<Role> result = await sender.Send(new GetRoleQuery(request.Role), cancellationToken);
            if (result is not { Status: RequestStatus.Success })
            {
                return new ApiCallResponse<bool>(result.Status, false, result.Error);
            }

            result.Result.Permissions |= request.Permissions;
            context.Roles.Update(result.Result);

            await context.SaveChangesAsync(cancellationToken);
            return new ApiCallResponse<bool>(RequestStatus.Success, true, null);
        }
        catch (OperationCanceledException exception)
        {
            return new ApiCallResponse<bool>(RequestStatus.Cancelled, false, new ApiCallError(RequestStatus.Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new ApiCallResponse<bool>(RequestStatus.Error, false, new ApiCallError(RequestStatus.Error, "Request interrupted by server.", exception));
        }
    }
}
