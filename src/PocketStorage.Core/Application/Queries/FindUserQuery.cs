using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public class FindUserQuery(string? email) : IRequest<ApiCallResponse<User>>
{
    public string? Email { get; set; } = email;
}

public class FindUserQueryHandler(DataContext context) : IRequestHandler<FindUserQuery, ApiCallResponse<User>>
{
    public static readonly Func<DataContext, string, Task<User?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string email) =>
        context.Users.AsNoTracking().SingleOrDefault(user => user.Email == email));

    public async Task<ApiCallResponse<User>> Handle(FindUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (IsNullOrWhiteSpace(request.Email))
            {
                return new ApiCallResponse<User>(RequestStatus.Fail, null, new ApiCallError(RequestStatus.EntityNotFound, "The user must be signed in to call the method.", new BadRequestException()));
            }

            User? user = await CompiledQuery(context, request.Email);
            if (user == null)
            {
                return new ApiCallResponse<User>(RequestStatus.EntityNotFound, null, new ApiCallError(RequestStatus.EntityNotFound, "The user with the given email address could not be found.", new EntityNotFoundException(request.Email, nameof(User))));
            }

            return new ApiCallResponse<User>(RequestStatus.Success, user, null);
        }
        catch (OperationCanceledException exception)
        {
            return new ApiCallResponse<User>(RequestStatus.Cancelled, null, new ApiCallError(RequestStatus.Cancelled, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new ApiCallResponse<User>(RequestStatus.Cancelled, null, new ApiCallError(RequestStatus.Error, "Request interrupted by server.", exception));
        }
    }
}
