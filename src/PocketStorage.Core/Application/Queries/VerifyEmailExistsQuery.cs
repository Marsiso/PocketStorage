using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public class VerifyEmailExistsQuery(string? email) : IRequest<ApiCallResponse<bool>>
{
    public string? Email { get; set; } = email;
}

public class VerifyEmailExistsQueryHandler(DataContext context) : IRequestHandler<VerifyEmailExistsQuery, ApiCallResponse<bool>>
{
    private static readonly Func<DataContext, string, Task<bool>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string email) =>
        context.Users.AsNoTracking().Any(user => user.Email == email));

    public async Task<ApiCallResponse<bool>> Handle(VerifyEmailExistsQuery request, CancellationToken cancellationToken)
    {
        if (IsNullOrWhiteSpace(request.Email))
        {
            return new ApiCallResponse<bool>(RequestStatus.Fail, false, new ApiCallError(RequestStatus.EntityNotFound, null, new BadRequestException()));
        }

        if (await CompiledQuery(context, request.Email))
        {
            return new ApiCallResponse<bool>(RequestStatus.Success, await CompiledQuery(context, request.Email), null);
        }

        return new ApiCallResponse<bool>(RequestStatus.EntityNotFound, await CompiledQuery(context, request.Email), new ApiCallError(RequestStatus.EntityNotFound, "The user with the given email address could not be found.", new EntityNotFoundException(request.Email, nameof(User))));
    }
}
