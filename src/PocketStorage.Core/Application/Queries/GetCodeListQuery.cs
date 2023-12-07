using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Core.Application.Models;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Queries;

public class GetCodeListQuery : IRequest<GetCodeListQueryResult>
{
    public int? CodeListId { get; set; }
}

public class GetCodeListQueryHandler(DataContext context) : IRequestHandler<GetCodeListQuery, GetCodeListQueryResult>
{
    private static readonly Func<DataContext, int, Task<CodeListResource?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, int codelistId) =>
        context.CodeLists
            .AsNoTracking()
            .Where(codeList => codeList.CodeListId == codelistId)
            .Select(codeList => new CodeListResource
            {
                CodeListId = codeList.CodeListId,
                Name = codeList.Name,
                CodeListItems = codeList.CodeListItems != null ? codeList.CodeListItems.Select(codeListItem => new CodeListItemResource { CodeListId = codeListItem.CodeListId, CodeListItemId = codeListItem.CodeListItemId, Value = codeListItem.Value }) : null
            })
            .SingleOrDefault());

    public async Task<GetCodeListQueryResult> Handle(GetCodeListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!request.CodeListId.HasValue || request.CodeListId.Value < 1)
            {
                return new GetCodeListQueryResult(Fail, new RequestError(Fail, "Invalid request, please provide the code list ID.", new BadRequestException()));
            }

            CodeListResource? codeList = await CompiledQuery(context, request.CodeListId.Value);
            if (codeList == null)
            {
                return new GetCodeListQueryResult(EntityNotFound, new RequestError(EntityNotFound, "", new EntityNotFoundException(request.CodeListId.Value.ToString(), nameof(CodeList))));
            }

            return new GetCodeListQueryResult(codeList);
        }
        catch (OperationCanceledException exception)
        {
            return new GetCodeListQueryResult(Cancelled, new RequestError(Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new GetCodeListQueryResult(Error, new RequestError(Error, "Request interrupted by server.", exception));
        }
    }
}

public class GetCodeListQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public GetCodeListQueryResult(CodeListResource? result) : this(Success, null) => Result = result;

    public CodeListResource? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
