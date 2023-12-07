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

public class GetCodeListItemsQuery(CodeListItemQueryString queryString) : IRequest<GetCodeListItemsQueryResult>
{
    public CodeListItemQueryString QueryString { get; set; } = queryString;
}

public class GetCodeListItemsQueryHandler(DataContext context) : IRequestHandler<GetCodeListItemsQuery, GetCodeListItemsQueryResult>
{
    private static readonly Func<DataContext, int, int, IAsyncEnumerable<CodeListItemResource>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, int skip, int take) =>
        context.CodeListItems
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .Select(codeListItem => new CodeListItemResource { CodeListId = codeListItem.CodeListId, CodeListItemId = codeListItem.CodeListItemId, Value = codeListItem.Value }));

    private static readonly Func<DataContext, Task<int>> CompiledCountQuery = EF.CompileAsyncQuery((DataContext context) =>
        context.CodeListItems
            .AsNoTracking()
            .Count());

    public async Task<GetCodeListItemsQueryResult> Handle(GetCodeListItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return new GetCodeListItemsQueryResult(new PagedList<CodeListItemResource>(
                await CompiledQuery(context, request.QueryString.RecordsOffset(), request.QueryString.RecordsToReturn()).ToListAsync(),
                await CompiledCountQuery(context),
                request.QueryString.PageNumber,
                request.QueryString.PageSize));
        }
        catch (OperationCanceledException exception)
        {
            return new GetCodeListItemsQueryResult(Cancelled, new RequestError(Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new GetCodeListItemsQueryResult(Error, new RequestError(Error, "Request interrupted by server.", exception));
        }
    }
}

public class GetCodeListItemsQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public GetCodeListItemsQueryResult(PagedList<CodeListItemResource>? result) : this(Success, null) => Result = result;

    public PagedList<CodeListItemResource>? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
