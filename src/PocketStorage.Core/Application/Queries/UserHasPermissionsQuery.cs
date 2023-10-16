using CommunityToolkit.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;

namespace PocketStorage.Core.Application.Queries;

public record UserHasPermissionsQuery(string? Id, Permission Permissions) : IRequest<UserHasPermissionsQueryResult>;

public class UserHasPermissionsQueryHandler : IRequestHandler<UserHasPermissionsQuery, UserHasPermissionsQueryResult>
{
    public static readonly Func<DataContext, IList<string>, IAsyncEnumerable<Role>> Query = EF.CompileAsyncQuery((DataContext databaseContext, IList<string> roleNames) =>
        databaseContext.Roles.AsNoTracking()
            .Where(entity => roleNames.Contains(entity.Name)));

    private readonly DataContext _databaseContext;
    private readonly ILogger<UserHasPermissionsQueryHandler> _logger;
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;

    public UserHasPermissionsQueryHandler(DataContext databaseContext, UserManager<User> userManager, IMediator mediator, ILogger<UserHasPermissionsQueryHandler> logger)
    {
        _databaseContext = databaseContext;
        _userManager = userManager;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<UserHasPermissionsQueryResult> Handle(UserHasPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            GetUserQuery query = new(request.Id);
            GetUserQueryResult queryResult = await _mediator.Send(query, cancellationToken);

            if (queryResult.ResultType == GetUserQueryResultType.UserNotFound)
            {
                return new UserHasPermissionsQueryResult(UserHasPermissionsQueryResultType.NotAuthorized, default, queryResult.Exception);
            }

            if (queryResult.ResultType == GetUserQueryResultType.OperationCancelled)
            {
                return new UserHasPermissionsQueryResult(UserHasPermissionsQueryResultType.OperationCancelled, default, queryResult.Exception);
            }

            if (queryResult.ResultType != GetUserQueryResultType.UserFound)
            {
                return new UserHasPermissionsQueryResult(UserHasPermissionsQueryResultType.InternalServerError, default, queryResult.Exception);
            }

            User originalUser = queryResult.GetResult();

            IList<string> roleNames = await _userManager.GetRolesAsync(originalUser);

            List<Role> roles = new();
            await foreach (Role role in Query(_databaseContext, roleNames).WithCancellation(cancellationToken))
            {
                roles.Add(role);
            }

            bool authorized = roles.Any(entity => (entity.Permissions & request.Permissions) > 0);
            if (authorized)
            {
                return new UserHasPermissionsQueryResult(UserHasPermissionsQueryResultType.Authorized, originalUser, default);
            }

            return new UserHasPermissionsQueryResult(UserHasPermissionsQueryResultType.NotAuthorized, originalUser, default);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.ToString());

            return new UserHasPermissionsQueryResult(UserHasPermissionsQueryResultType.InternalServerError, default, exception);
        }
    }
}

public record UserHasPermissionsQueryResult(UserHasPermissionsQueryResultType ResultType, User? Result, Exception? Exception)
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

public enum UserHasPermissionsQueryResultType
{
    Authorized,
    NotAuthorized,
    OperationCancelled,
    InternalServerError
}
