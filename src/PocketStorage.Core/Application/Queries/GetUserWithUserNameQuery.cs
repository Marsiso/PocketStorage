using CommunityToolkit.Diagnostics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Exceptions;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public record GetUserWithUserNameQuery(string? UserName) : IRequest<GetUserWithUserNameQueryResult>;

public class GetUserQueryWithUserNameHandler : IRequestHandler<GetUserWithUserNameQuery, GetUserWithUserNameQueryResult>
{
    public static readonly Func<DataContext, string, Task<User?>> Query = EF.CompileAsyncQuery((DataContext databaseContext, string userName) =>
        databaseContext.Users.AsNoTracking()
            .SingleOrDefault(entity => entity.UserName == userName));

    private readonly DataContext _databaseContext;
    private readonly ILogger<GetUserQueryWithUserNameHandler> _logger;

    public GetUserQueryWithUserNameHandler(DataContext databaseContext, ILogger<GetUserQueryWithUserNameHandler> logger)
    {
        _databaseContext = databaseContext;
        _logger = logger;
    }

    public async Task<GetUserWithUserNameQueryResult> Handle(GetUserWithUserNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            User? originalUser = default;

            if (!IsNullOrWhiteSpace(request.UserName))
            {
                originalUser = await Query(_databaseContext, request.UserName);
            }

            if (originalUser is not null)
            {
                return new GetUserWithUserNameQueryResult(GetUserWithUserNameQueryResultType.UserFound, originalUser, default);
            }

            EntityNotFoundException exception = new(request.UserName, nameof(User));

            return new GetUserWithUserNameQueryResult(GetUserWithUserNameQueryResultType.UserNotFound, default, exception);
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogError(exception.ToString());

            return new GetUserWithUserNameQueryResult(GetUserWithUserNameQueryResultType.OperationCancelled, default, exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.ToString());

            return new GetUserWithUserNameQueryResult(GetUserWithUserNameQueryResultType.InternalServerError, default, exception);
        }
    }
}

public record GetUserWithUserNameQueryResult(GetUserWithUserNameQueryResultType ResultType, User? Result, Exception? Exception)
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

public enum GetUserWithUserNameQueryResultType
{
    UserFound,
    UserNotFound,
    OperationCancelled,
    InternalServerError
}
