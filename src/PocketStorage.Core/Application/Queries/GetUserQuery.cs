using CommunityToolkit.Diagnostics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Exceptions;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public record GetUserQuery(string? Id) : IRequest<GetUserQueryResult>;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, GetUserQueryResult>
{
    public static readonly Func<DataContext, string, Task<User?>> Query = EF.CompileAsyncQuery((DataContext databaseContext, string id) =>
        databaseContext.Users.AsNoTracking()
            .SingleOrDefault(entity => entity.Id == id));

    private readonly DataContext _databaseContext;
    private readonly ILogger<GetUserQueryHandler> _logger;

    public GetUserQueryHandler(DataContext databaseContext, ILogger<GetUserQueryHandler> logger)
    {
        _databaseContext = databaseContext;
        _logger = logger;
    }

    public async Task<GetUserQueryResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            User? originalUser = default;

            if (!IsNullOrWhiteSpace(request.Id))
            {
                originalUser = await Query(_databaseContext, request.Id);
            }

            if (originalUser is not null)
            {
                return new GetUserQueryResult(GetUserQueryResultType.UserFound, originalUser, default);
            }

            EntityNotFoundException exception = new(request.Id, nameof(User));

            return new GetUserQueryResult(GetUserQueryResultType.UserNotFound, default, exception);
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogError(exception.ToString());

            return new GetUserQueryResult(GetUserQueryResultType.OperationCancelled, default, exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.ToString());

            return new GetUserQueryResult(GetUserQueryResultType.InternalServerError, default, exception);
        }
    }
}

public record GetUserQueryResult(GetUserQueryResultType ResultType, User? Result, Exception? Exception)
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

public enum GetUserQueryResultType
{
    UserFound,
    UserNotFound,
    OperationCancelled,
    InternalServerError
}
