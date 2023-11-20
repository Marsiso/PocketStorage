using MediatR;
using Microsoft.AspNetCore.Identity;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;

namespace PocketStorage.Core.Application.Queries;

public class VerifyPasswordQuery(string? email, string? password) : IRequest<ApiCallResponse<bool>>
{
    public string? Email { get; set; } = email;
    public string? Password { get; set; } = password;
}

public class VerifyPasswordQueryHandler(DataContext context, UserManager<User> userManager) : IRequestHandler<VerifyPasswordQuery, ApiCallResponse<bool>>
{
    public async Task<ApiCallResponse<bool>> Handle(VerifyPasswordQuery request, CancellationToken cancellationToken)
    {
        if (IsNullOrWhiteSpace(request.Email))
        {
            return new ApiCallResponse<bool>(RequestStatus.Fail, false, new ApiCallError(RequestStatus.Fail, "Invalid request, please provide the account's email address.", new BadRequestException()));
        }

        if (IsNullOrWhiteSpace(request.Password))
        {
            return new ApiCallResponse<bool>(RequestStatus.Fail, false, new ApiCallError(RequestStatus.Fail, "Invalid request, please provide the account's password.", new BadRequestException()));
        }

        User? user = await GetUserQueryHandler.CompiledQuery(context, request.Email);
        if (user == null)
        {
            return new ApiCallResponse<bool>(RequestStatus.EntityNotFound, false, new ApiCallError(RequestStatus.EntityNotFound, "The user with the given email address could not be found.", new EntityNotFoundException(request.Email, nameof(User))));
        }

        return new ApiCallResponse<bool>(RequestStatus.Success, await userManager.CheckPasswordAsync(user, request.Password), null);
    }
}
