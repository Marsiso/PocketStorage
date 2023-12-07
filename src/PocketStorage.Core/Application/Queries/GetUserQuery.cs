using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public class GetUserQuery : IRequest<GetUserQueryResult>, IEmailRequest
{
    public string? Email { get; set; }
}

public class GetUserQueryHandler(DataContext context) : IRequestHandler<GetUserQuery, GetUserQueryResult>
{
    public static readonly Func<DataContext, string, Task<User?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string email) =>
        context.Users
            .AsNoTracking()
            .SingleOrDefault(user => user.Email == email));

    public async Task<GetUserQueryResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (IsNullOrWhiteSpace(request.Email))
            {
                return new GetUserQueryResult(RequestStatus.Fail, new RequestError(RequestStatus.EntityNotFound, "The user must be signed in to call the method.", new BadRequestException()));
            }

            User? user = await CompiledQuery(context, request.Email);
            if (user == null)
            {
                return new GetUserQueryResult(RequestStatus.EntityNotFound, new RequestError(RequestStatus.EntityNotFound, "The user with the given email address could not be found.", new EntityNotFoundException(request.Email, nameof(User))));
            }

            return new GetUserQueryResult(user);
        }
        catch (OperationCanceledException exception)
        {
            return new GetUserQueryResult(RequestStatus.Cancelled, new RequestError(RequestStatus.Cancelled, "Request interrupted by the client.", exception));
        }
        catch (Exception exception)
        {
            return new GetUserQueryResult(RequestStatus.Cancelled, new RequestError(RequestStatus.Error, "Request interrupted by the server.", exception));
        }
    }
}

public class GetUserQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public GetUserQueryResult(User? result) : this(RequestStatus.Success, null) => Result = result;

    public User? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
