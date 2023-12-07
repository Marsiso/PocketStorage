using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Queries;

public class FindUserQuery(string? email) : IRequest<FindUserQueryResult>
{
    public string? Email { get; set; } = email;
}

public class FindUserQueryHandler(DataContext context) : IRequestHandler<FindUserQuery, FindUserQueryResult>
{
    public static readonly Func<DataContext, string, Task<User?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string email) =>
        context.Users
            .AsNoTracking()
            .SingleOrDefault(user => user.Email == email));

    public async Task<FindUserQueryResult> Handle(FindUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (IsNullOrWhiteSpace(request.Email))
            {
                return new FindUserQueryResult(Fail, new RequestError(EntityNotFound, "The user must be signed in to call the method.", new BadRequestException()));
            }

            User? user = await CompiledQuery(context, request.Email);
            if (user == null)
            {
                return new FindUserQueryResult(EntityNotFound, new RequestError(EntityNotFound, "The user with the given email address could not be found.", new EntityNotFoundException(request.Email, nameof(User))));
            }

            return new FindUserQueryResult(user);
        }
        catch (OperationCanceledException exception)
        {
            return new FindUserQueryResult(Cancelled, new RequestError(Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new FindUserQueryResult(Error, new RequestError(Error, "Request interrupted by server.", exception));
        }
    }
}

public class FindUserQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public FindUserQueryResult(User? result) : this(Success, null) => Result = result;

    public User? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
