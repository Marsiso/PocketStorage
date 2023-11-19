using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public class GetRoleQuery(string? name) : IRequest<ApiCallResponse<Role>>
{
    public string? Name { get; set; } = name;
}

public class GetRoleQueryHandler(DataContext context) : IRequestHandler<GetRoleQuery, ApiCallResponse<Role>>
{
    private static readonly Func<DataContext, string, Task<Role?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string name) =>
        context.Roles.AsNoTracking().SingleOrDefault(role => role.Name == name));

    public async Task<ApiCallResponse<Role>> Handle(GetRoleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (IsNullOrWhiteSpace(request.Name))
            {
                return new ApiCallResponse<Role>(RequestStatus.Fail, null, new ApiCallError(RequestStatus.Fail, "Invalid request, please provide the name of the role.", new BadRequestException()));
            }

            Role? role = await CompiledQuery(context, request.Name);
            if (role == null)
            {
                return new ApiCallResponse<Role>(RequestStatus.EntityNotFound, null, new ApiCallError(RequestStatus.EntityNotFound, "The role with the given name could not be found.", new EntityNotFoundException(request.Name, nameof(Role))));
            }

            return new ApiCallResponse<Role>(RequestStatus.Success, role, null);
        }
        catch (OperationCanceledException exception)
        {
            return new ApiCallResponse<Role>(RequestStatus.Cancelled, null, new ApiCallError(RequestStatus.Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new ApiCallResponse<Role>(RequestStatus.Error, null, new ApiCallError(RequestStatus.Cancelled, "Request interrupted by server.", exception));
        }
    }
}
