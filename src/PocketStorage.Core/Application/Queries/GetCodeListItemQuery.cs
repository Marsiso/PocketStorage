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

public class GetCodeListItemQuery : IRequest<GetCodeListItemQueryResult>
{
    public int? CodeListItemId { get; set; }
}

public class GetCodeListItemQueryHandler(DataContext context) : IRequestHandler<GetCodeListItemQuery, GetCodeListItemQueryResult>
{
    private static readonly Func<DataContext, int, Task<CodeListItemResource?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, int id) =>
        context.CodeListItems
            .AsNoTracking()
            .Where(item => item.CodeListItemId == id)
            .Select(item => new CodeListItemResource { CodeListId = item.CodeListId, CodeListItemId = item.CodeListItemId, Value = item.Value })
            .SingleOrDefault());

    public async Task<GetCodeListItemQueryResult> Handle(GetCodeListItemQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!request.CodeListItemId.HasValue || request.CodeListItemId.Value < 1)
            {
                return new GetCodeListItemQueryResult(Fail, new RequestError(Fail, "Invalid request, please provide the code list item ID.", new BadRequestException()));
            }

            CodeListItemResource? item = await CompiledQuery(context, request.CodeListItemId.Value);
            if (item == null)
            {
                return new GetCodeListItemQueryResult(EntityNotFound, new RequestError(EntityNotFound, "The code list item with the given ID could not be found.", new EntityNotFoundException(request.CodeListItemId.Value.ToString(), nameof(CodeListItem))));
            }

            return new GetCodeListItemQueryResult(item);
        }
        catch (OperationCanceledException exception)
        {
            return new GetCodeListItemQueryResult(Cancelled, new RequestError(Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new GetCodeListItemQueryResult(Error, new RequestError(Error, "Request interrupted by server.", exception));
        }
    }
}

public class GetCodeListItemQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public GetCodeListItemQueryResult(CodeListItemResource? result) : this(Success, null) => Result = result;

    public CodeListItemResource? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
