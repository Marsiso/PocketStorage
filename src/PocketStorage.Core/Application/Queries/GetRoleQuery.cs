using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Queries;

public class GetRoleQuery(string? name) : IRequest<GetRoleQueryResult>
{
    public string? Name { get; set; } = name;
}

public class GetRoleQueryHandler(DataContext context) : IRequestHandler<GetRoleQuery, GetRoleQueryResult>
{
    private static readonly Func<DataContext, string, Task<Role?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string name) =>
        context.Roles.AsNoTracking().SingleOrDefault(role => role.Name == name));

    public async Task<GetRoleQueryResult> Handle(GetRoleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (IsNullOrWhiteSpace(request.Name))
            {
                return new GetRoleQueryResult(Fail, new RequestError(Fail, "Invalid request, please provide the name of the role.", new BadRequestException()));
            }

            Role? role = await CompiledQuery(context, request.Name);
            if (role == null)
            {
                return new GetRoleQueryResult(EntityNotFound, new RequestError(EntityNotFound, "The role with the given name could not be found.", new EntityNotFoundException(request.Name, nameof(Role))));
            }

            return new GetRoleQueryResult(role);
        }
        catch (OperationCanceledException exception)
        {
            return new GetRoleQueryResult(Cancelled, new RequestError(Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new GetRoleQueryResult(Error, new RequestError(Cancelled, "Request interrupted by server.", exception));
        }
    }
}

public class GetRoleQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public GetRoleQueryResult(Role? result) : this(Success, null) => Result = result;

    public Role? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
