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

public class GetCodeListsQuery(CodeListQueryString queryString) : IRequest<GetCodeListsQueryResult>
{
    public CodeListQueryString QueryString { get; set; } = queryString;
}

public class GetCodeListsQueryHandler(DataContext context) : IRequestHandler<GetCodeListsQuery, GetCodeListsQueryResult>
{
    private static readonly Func<DataContext, int, int, IAsyncEnumerable<CodeListResource>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, int skip, int take) =>
        context.CodeLists
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .Select(codeList => new CodeListResource { CodeListId = codeList.CodeListId, Name = codeList.Name }));

    private static readonly Func<DataContext, Task<int>> CompiledCountQuery = EF.CompileAsyncQuery((DataContext context) =>
        context.CodeLists
            .AsNoTracking()
            .Count());

    public async Task<GetCodeListsQueryResult> Handle(GetCodeListsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return new GetCodeListsQueryResult(new PagedList<CodeListResource>(
                await CompiledQuery(context, request.QueryString.RecordsOffset(), request.QueryString.RecordsToReturn()).ToListAsync(),
                await CompiledCountQuery(context),
                request.QueryString.PageNumber,
                request.QueryString.PageSize
            ));
        }
        catch (OperationCanceledException exception)
        {
            return new GetCodeListsQueryResult(Cancelled, new RequestError(Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new GetCodeListsQueryResult(Error, new RequestError(Error, "Request interrupted by server.", exception));
        }
    }
}

public class GetCodeListsQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public GetCodeListsQueryResult(PagedList<CodeListResource>? result) : this(Success, null) => Result = result;

    public PagedList<CodeListResource>? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
