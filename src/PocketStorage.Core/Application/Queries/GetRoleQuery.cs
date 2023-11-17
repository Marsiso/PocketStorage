using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Core.Application.Queries.GetRoleQueryStatus;

namespace PocketStorage.Core.Application.Queries;

public class GetRoleQuery : IRequest<GetRoleQueryResult>
{
    public GetRoleQuery(string? name) => Name = name;

    public string? Name { get; set; }
}

public class GetRoleQueryHandler : IRequestHandler<GetRoleQuery, GetRoleQueryResult>
{
    private static readonly Func<DataContext, string, Task<Role?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string name) =>
        context.Roles.AsNoTracking().SingleOrDefault(role => role.Name == name));

    private readonly DataContext _context;

    public GetRoleQueryHandler(DataContext context) => _context = context;

    public async Task<GetRoleQueryResult> Handle(GetRoleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (IsNullOrWhiteSpace(request.Name))
            {
                return new GetRoleQueryResult(new ApiCallError(new EntityNotFoundException(request.Name, nameof(Role))));
            }

            Role? role = await CompiledQuery(_context, request.Name);
            if (role != null)
            {
                return new GetRoleQueryResult(role);
            }

            return new GetRoleQueryResult(new ApiCallError(HttpStatusCode.NotFound, new EntityNotFoundException(request.Name, nameof(Role))));
        }
        catch (OperationCanceledException exception)
        {
            return new GetRoleQueryResult(OperationCancelled, new ApiCallError(499, exception));
        }
        catch (Exception exception)
        {
            return new GetRoleQueryResult(OperationCancelled, new ApiCallError(HttpStatusCode.InternalServerError, exception));
        }
    }
}

public class GetRoleQueryResult : IRequestResult
{
    public GetRoleQueryResult(Role? result)
    {
        Status = Success;
        Result = result;
    }

    public GetRoleQueryResult(GetRoleQueryStatus status, ApiCallError? error)
    {
        Status = status;
        Error = error;
    }

    public GetRoleQueryResult(ApiCallError? error)
    {
        Status = Fail;
        Error = error;
    }

    public GetRoleQueryStatus Status { get; set; }
    public Role? Result { get; set; }
    public ApiCallError? Error { get; set; }
}

public enum GetRoleQueryStatus
{
    Success,
    Fail,
    OperationCancelled,
    InternalServerError
}
