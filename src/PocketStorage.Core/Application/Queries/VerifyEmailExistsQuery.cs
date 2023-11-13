using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Core.Application.Queries.VerifyEmailExistsQueryResultStatus;

namespace PocketStorage.Core.Application.Queries;

public class VerifyEmailExistsQuery : IRequest<VerifyEmailExistsQueryResult>
{
    public VerifyEmailExistsQuery(string? email) => Email = email;

    public string? Email { get; set; }
}

public class VerifyEmailExistsQueryHandler : IRequestHandler<VerifyEmailExistsQuery, VerifyEmailExistsQueryResult>
{
    private static readonly Func<DataContext, string, Task<bool>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string email) =>
        context.Users.AsNoTracking().Any(user => user.Email == email));

    private readonly DataContext _context;

    public VerifyEmailExistsQueryHandler(DataContext context) => _context = context;

    public async Task<VerifyEmailExistsQueryResult> Handle(VerifyEmailExistsQuery request, CancellationToken cancellationToken)
    {
        if (IsNullOrWhiteSpace(request.Email))
        {
            return new VerifyEmailExistsQueryResult(new ApiCallError());
        }

        return new VerifyEmailExistsQueryResult(Success, await CompiledQuery(_context, request.Email));
    }
}

public class VerifyEmailExistsQueryResult : IRequestResult
{
    public VerifyEmailExistsQueryResult(VerifyEmailExistsQueryResultStatus status, bool result)
    {
        Status = status;
        Result = result;
    }

    public VerifyEmailExistsQueryResult(ApiCallError? error)
    {
        Status = Fail;
        Error = error;
    }

    public VerifyEmailExistsQueryResultStatus Status { get; set; }
    public bool Result { get; set; }
    public ApiCallError? Error { get; set; }
}

public enum VerifyEmailExistsQueryResultStatus
{
    Success,
    Fail,
    OperationCancelled,
    InternalServerError
}
