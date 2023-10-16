using CommunityToolkit.Diagnostics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Exceptions;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public record GetRoleQuery(string? Id) : IRequest<GetRoleQueryResult>;

public class GetRoleQueryHandler : IRequestHandler<GetRoleQuery, GetRoleQueryResult>
{
    public static readonly Func<DataContext, string, Task<Role?>> Query = EF.CompileAsyncQuery((DataContext databaseContext, string id) =>
        databaseContext.Roles.AsNoTracking()
            .SingleOrDefault(entity => entity.Id == id));

    private readonly DataContext _databaseContext;
    private readonly ILogger<GetRoleQueryHandler> _logger;

    public GetRoleQueryHandler(DataContext databaseContext, ILogger<GetRoleQueryHandler> logger)
    {
        _databaseContext = databaseContext;
        _logger = logger;
    }

    public async Task<GetRoleQueryResult> Handle(GetRoleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Role? originalRole = default;

            if (!IsNullOrWhiteSpace(request.Id))
            {
                originalRole = await Query(_databaseContext, request.Id);
            }

            if (originalRole is not null)
            {
                return new GetRoleQueryResult(GetRoleQueryResultType.RoleFound, originalRole, default);
            }

            EntityNotFoundException exception = new(request.Id, nameof(Role));

            return new GetRoleQueryResult(GetRoleQueryResultType.RoleNotFound, default, exception);
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogError(exception.ToString());

            return new GetRoleQueryResult(GetRoleQueryResultType.OperationCancelled, default, exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.ToString());

            return new GetRoleQueryResult(GetRoleQueryResultType.InternalServerError, default, exception);
        }
    }
}

public record GetRoleQueryResult(GetRoleQueryResultType ResultType, Role? Result, Exception? Exception)
{
    public Role GetResult()
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

public enum GetRoleQueryResultType
{
    RoleFound,
    RoleNotFound,
    OperationCancelled,
    InternalServerError
}
