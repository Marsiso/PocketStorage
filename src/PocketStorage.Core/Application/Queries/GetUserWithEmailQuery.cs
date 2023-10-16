using CommunityToolkit.Diagnostics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Exceptions;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public record GetUserWithEmailQuery(string? Email) : IRequest<GetUserWithEmailQueryResult>;

public class GetUserQueryWithEmailHandler : IRequestHandler<GetUserWithEmailQuery, GetUserWithEmailQueryResult>
{
    public static readonly Func<DataContext, string, Task<User?>> Query = EF.CompileAsyncQuery((DataContext databaseContext, string email) =>
        databaseContext.Users.AsNoTracking()
            .SingleOrDefault(entity => entity.Email == email));

    private readonly DataContext _databaseContext;
    private readonly ILogger<GetUserQueryWithEmailHandler> _logger;

    public GetUserQueryWithEmailHandler(DataContext databaseContext, ILogger<GetUserQueryWithEmailHandler> logger)
    {
        _databaseContext = databaseContext;
        _logger = logger;
    }

    public async Task<GetUserWithEmailQueryResult> Handle(GetUserWithEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            User? originalUser = default;

            if (!IsNullOrWhiteSpace(request.Email))
            {
                originalUser = await Query(_databaseContext, request.Email);
            }

            if (originalUser is not null)
            {
                return new GetUserWithEmailQueryResult(GetUserWithEmailQueryResultType.UserFound, originalUser, default);
            }

            EntityNotFoundException exception = new(request.Email, nameof(User));

            return new GetUserWithEmailQueryResult(GetUserWithEmailQueryResultType.UserNotFound, default, exception);
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogError(exception.ToString());

            return new GetUserWithEmailQueryResult(GetUserWithEmailQueryResultType.OperationCancelled, default, exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.ToString());

            return new GetUserWithEmailQueryResult(GetUserWithEmailQueryResultType.InternalServerError, default, exception);
        }
    }
}

public record GetUserWithEmailQueryResult(GetUserWithEmailQueryResultType ResultType, User? Result, Exception? Exception)
{
    public User GetResult()
    {
        Guard.IsNotNull(Result);

        return Result;
    }

    public Exception GetException()
    {
        Guard.IsNotNull(Exception);

        return Exception;
    }
}

public enum GetUserWithEmailQueryResultType
{
    UserFound,
    UserNotFound,
    OperationCancelled,
    InternalServerError
}
