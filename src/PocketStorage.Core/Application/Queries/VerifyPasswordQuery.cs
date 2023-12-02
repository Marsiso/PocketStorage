using MediatR;
using Microsoft.AspNetCore.Identity;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Queries;

public class VerifyPasswordQuery(string? email, string? password) : IRequest<VerifyPasswordQueryResult>
{
    public string? Email { get; set; } = email;
    public string? Password { get; set; } = password;
}

public class VerifyPasswordQueryHandler(DataContext context, UserManager<User> userManager) : IRequestHandler<VerifyPasswordQuery, VerifyPasswordQueryResult>
{
    public async Task<VerifyPasswordQueryResult> Handle(VerifyPasswordQuery request, CancellationToken cancellationToken)
    {
        if (IsNullOrWhiteSpace(request.Email))
        {
            return new VerifyPasswordQueryResult(Fail, new RequestError(Fail, "Invalid request, please provide the account's email address.", new BadRequestException()));
        }

        if (IsNullOrWhiteSpace(request.Password))
        {
            return new VerifyPasswordQueryResult(Fail, new RequestError(Fail, "Invalid request, please provide the account's password.", new BadRequestException()));
        }

        User? user = await GetUserQueryHandler.CompiledQuery(context, request.Email);
        if (user == null)
        {
            return new VerifyPasswordQueryResult(EntityNotFound, new RequestError(EntityNotFound, "The user with the given email address could not be found.", new EntityNotFoundException(request.Email, nameof(User))));
        }

        return new VerifyPasswordQueryResult(await userManager.CheckPasswordAsync(user, request.Password));
    }
}

public class VerifyPasswordQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public VerifyPasswordQueryResult(bool result) : this(Success, null) => Result = result;

    public bool Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
