using CommunityToolkit.Diagnostics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Exceptions;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public record GetRoleWithNameQuery(string? Name) : IRequest<GetRoleWithNameQueryResult>;

public class GetRoleWithNameQueryHandler : IRequestHandler<GetRoleWithNameQuery, GetRoleWithNameQueryResult>
{
    public static readonly Func<DataContext, string, Task<Role?>> Query = EF.CompileAsyncQuery((DataContext databaseContext, string name) =>
        databaseContext.Roles.AsNoTracking()
            .SingleOrDefault(entity => entity.Name == name));

    private readonly DataContext _databaseContext;
    private readonly ILogger<GetRoleWithNameQueryHandler> _logger;

    public GetRoleWithNameQueryHandler(DataContext databaseContext, ILogger<GetRoleWithNameQueryHandler> logger)
    {
        _databaseContext = databaseContext;
        _logger = logger;
    }

    public async Task<GetRoleWithNameQueryResult> Handle(GetRoleWithNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Role? originalRole = default;

            if (!IsNullOrWhiteSpace(request.Name))
            {
                originalRole = await Query(_databaseContext, request.Name);
            }

            if (originalRole is not null)
            {
                return new GetRoleWithNameQueryResult(GetRoleWithNameQueryResultType.RoleFound, originalRole, default);
            }

            EntityNotFoundException exception = new(request.Name, nameof(Role));

            return new GetRoleWithNameQueryResult(GetRoleWithNameQueryResultType.RoleNotFound, default, exception);
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogError(exception.ToString());

            return new GetRoleWithNameQueryResult(GetRoleWithNameQueryResultType.OperationCancelled, default, exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.ToString());

            return new GetRoleWithNameQueryResult(GetRoleWithNameQueryResultType.InternalServerError, default, exception);
        }
    }
}

public record GetRoleWithNameQueryResult(GetRoleWithNameQueryResultType ResultType, Role? Result, Exception? Exception)
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

public enum GetRoleWithNameQueryResultType
{
    RoleFound,
    RoleNotFound,
    OperationCancelled,
    InternalServerError
}
