using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public class VerifyEmailExistsQuery(string? email) : IRequest<VerifyEmailExistsQueryResult>
{
    public string? Email { get; set; } = email;
}

public class VerifyEmailExistsQueryHandler(DataContext context) : IRequestHandler<VerifyEmailExistsQuery, VerifyEmailExistsQueryResult>
{
    private static readonly Func<DataContext, string, Task<bool>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string email) =>
        context.Users.AsNoTracking().Any(user => user.Email == email));

    public async Task<VerifyEmailExistsQueryResult> Handle(VerifyEmailExistsQuery request, CancellationToken cancellationToken)
    {
        if (IsNullOrWhiteSpace(request.Email))
        {
            return new VerifyEmailExistsQueryResult(RequestStatus.Fail, new RequestError(RequestStatus.EntityNotFound, null, new BadRequestException()));
        }

        return new VerifyEmailExistsQueryResult(await CompiledQuery(context, request.Email));
    }
}

public class VerifyEmailExistsQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public VerifyEmailExistsQueryResult(bool result) : this(RequestStatus.Success, null) => Result = result;

    public bool Result { get; set; }
    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
