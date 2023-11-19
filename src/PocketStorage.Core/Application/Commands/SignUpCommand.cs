using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;

namespace PocketStorage.Core.Application.Commands;

public record SignUpCommand(string? UserName, string? GivenName, string? FamilyName, string? Email, string? Phone, string? Password, string? Culture, string? ProfilePhoto) : IRequest<ApiCallResponse<User>>;

public class SignUpCommandHandler(UserManager<User> userManager, IValidator<SignUpCommand> validator, IMapper mapper) : IRequestHandler<SignUpCommand, ApiCallResponse<User>>
{
    public async Task<ApiCallResponse<User>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Dictionary<string, string[]> errors = (await validator.ValidateAsync(new ValidationContext<SignUpCommand>(request), cancellationToken)).DistinctErrorsByProperty();
            if (errors.Count > 0)
            {
                return new ApiCallResponse<User>(RequestStatus.ValidationFailure, null, new ApiCallError(RequestStatus.ValidationFailure, "The submitted form contains invalid information.", new EntityValidationException(nameof(User), request.Email, null, errors)));
            }

            User user = mapper.Map<User>(request);
            IdentityResult result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return new ApiCallResponse<User>(RequestStatus.Error, null, new ApiCallError(RequestStatus.Error, null, new Exception(Join(" ", result.Errors))));
            }

            result = await userManager.AddPasswordAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new ApiCallResponse<User>(RequestStatus.Error, null, new ApiCallError(RequestStatus.Error, null, new Exception(Join(" ", result.Errors))));
            }

            return new ApiCallResponse<User>(RequestStatus.Success, user, null);
        }
        catch (OperationCanceledException exception)
        {
            return new ApiCallResponse<User>(RequestStatus.Cancelled, null, new ApiCallError(RequestStatus.Error, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new ApiCallResponse<User>(RequestStatus.Error, null, new ApiCallError(RequestStatus.Error, "Request interrupted by server.", exception));
        }
    }
}
