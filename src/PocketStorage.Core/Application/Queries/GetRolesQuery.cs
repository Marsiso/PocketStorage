using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Core.Application.Models;
using PocketStorage.Core.Extensions;
using PocketStorage.Data;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Queries;

public class GetRolesQuery(RoleQueryString queryString) : IRequest<GetRolesQueryResult>
{
    public RoleQueryString QueryString { get; set; } = queryString;
}

public class GetRolesQueryHandler(DataContext context) : IRequestHandler<GetRolesQuery, GetRolesQueryResult>
{
    private static readonly Func<DataContext, int, int, IAsyncEnumerable<RoleResource>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, int skip, int take) =>
        context.Roles
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .Select(role => new RoleResource { Name = role.Name, Permissions = role.Permissions }));

    private static readonly Func<DataContext, Task<int>> CompiledCountQuery = EF.CompileAsyncQuery((DataContext context) =>
        context.Roles
            .AsNoTracking()
            .Count());

    public async Task<GetRolesQueryResult> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return new GetRolesQueryResult(new PagedList<RoleResource>(
                await CompiledQuery(context, request.QueryString.RecordsOffset(), request.QueryString.RecordsToReturn()).ToListAsync(),
                await CompiledCountQuery(context),
                request.QueryString.PageNumber,
                request.QueryString.PageSize));
        }
        catch (OperationCanceledException exception)
        {
            return new GetRolesQueryResult(Cancelled, new RequestError(Cancelled, "Request interrupted by the client.", exception));
        }
        catch (Exception exception)
        {
            return new GetRolesQueryResult(Error, new RequestError(Error, "Request interrupted by the server.", exception));
        }
    }
}

public class GetRolesQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public GetRolesQueryResult(PagedList<RoleResource>? result) : this(Success, null) => Result = result;
    public PagedList<RoleResource>? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
