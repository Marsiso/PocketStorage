using System.Net;
using CommunityToolkit.Diagnostics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Core.Application.Queries.GetUserQueryResultStatus;

namespace PocketStorage.Core.Application.Queries;

public class GetUserQuery : IRequest<GetUserQueryResult>, IEmailRequest
{
    public string? Email { get; set; }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, GetUserQueryResult>
{
    public static readonly Func<DataContext, string, Task<User?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string email) =>
        context.Users.AsNoTracking().SingleOrDefault(entity => entity.Email == email));

    private readonly DataContext _context;

    public GetUserQueryHandler(DataContext context) => _context = context;

    public async Task<GetUserQueryResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (IsNullOrWhiteSpace(request.Email))
            {
                return new GetUserQueryResult(new ApiCallError(new EntityNotFoundException(request.Email, nameof(User))));
            }

            User? user = await CompiledQuery(_context, request.Email);
            if (user != null)
            {
                return new GetUserQueryResult(Success, user);
            }

            return new GetUserQueryResult(new ApiCallError(HttpStatusCode.NotFound, new EntityNotFoundException(request.Email, nameof(User))));
        }
        catch (OperationCanceledException exception)
        {
            return new GetUserQueryResult(new ApiCallError(499, exception));
        }
        catch (Exception exception)
        {
            return new GetUserQueryResult(new ApiCallError(HttpStatusCode.InternalServerError, exception));
        }
    }
}

public class GetUserQueryResult : IRequestResult
{
    public GetUserQueryResult(GetUserQueryResultStatus status, User? result)
    {
        Status = status;
        Result = result;
    }

    public GetUserQueryResult(ApiCallError? error)
    {
        Status = Fail;
        Error = error;
    }

    public GetUserQueryResultStatus Status { get; set; }
    public User? Result { get; set; }
    public ApiCallError? Error { get; set; }

    public User GetResult()
    {
        Guard.IsNotNull(Result);
        return Result;
    }

    public ApiCallError GetError()
    {
        Guard.IsNotNull(Error);
        return Error;
    }
}

public enum GetUserQueryResultStatus
{
    Success,
    Fail,
    OperationCancelled,
    InternalServerError
}
