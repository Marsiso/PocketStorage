using System.Net;
using MediatR;
using Microsoft.AspNetCore.Identity;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Core.Application.Queries.VerifyPasswordQueryResultStatus;

namespace PocketStorage.Core.Application.Queries;

public class VerifyPasswordQuery : IRequest<VerifyPasswordQueryResult>
{
    public VerifyPasswordQuery(string? email, string? password)
    {
        Email = email;
        Password = password;
    }

    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class VerifyPasswordQueryHandler : IRequestHandler<VerifyPasswordQuery, VerifyPasswordQueryResult>
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;

    public VerifyPasswordQueryHandler(DataContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<VerifyPasswordQueryResult> Handle(VerifyPasswordQuery request, CancellationToken cancellationToken)
    {
        if (IsNullOrWhiteSpace(request.Email))
        {
            return new VerifyPasswordQueryResult(new ApiCallError());
        }

        if (IsNullOrWhiteSpace(request.Password))
        {
            return new VerifyPasswordQueryResult(new ApiCallError());
        }

        User? user = await GetUserQueryHandler.CompiledQuery(_context, request.Email);
        if (user == null)
        {
            return new VerifyPasswordQueryResult(UserNotFound, new ApiCallError(HttpStatusCode.NotFound, new EntityNotFoundException(request.Email, nameof(User))));
        }

        return new VerifyPasswordQueryResult(Success, await _userManager.CheckPasswordAsync(user, request.Password));
    }
}

public class VerifyPasswordQueryResult : IRequestResult
{
    public VerifyPasswordQueryResult(VerifyPasswordQueryResultStatus status, bool result)
    {
        Status = status;
        Result = result;
    }

    public VerifyPasswordQueryResult(VerifyPasswordQueryResultStatus status, ApiCallError? error)
    {
        Status = status;
        Error = error;
    }

    public VerifyPasswordQueryResult(ApiCallError? error)
    {
        Status = Fail;
        Error = error;
    }

    public VerifyPasswordQueryResultStatus Status { get; set; }
    public bool Result { get; set; }
    public ApiCallError? Error { get; set; }
}

public enum VerifyPasswordQueryResultStatus
{
    Success,
    UserNotFound,
    Fail,
    OperationCancelled,
    InternalServerError
}
