using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Commands;

public record SignUpCommand(string? UserName, string? GivenName, string? FamilyName, string? Email, string? Phone, string? Password, string? Culture, string? ProfilePhoto) : IRequest<SignUpCommandResult>;

public class SignUpCommandHandler(UserManager<User> userManager, IValidator<SignUpCommand> validator, IMapper mapper) : IRequestHandler<SignUpCommand, SignUpCommandResult>
{
    public async Task<SignUpCommandResult> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Dictionary<string, string[]> errors = (await validator.ValidateAsync(new ValidationContext<SignUpCommand>(request), cancellationToken)).DistinctErrorsByProperty();
            if (errors.Count > 0)
            {
                return new SignUpCommandResult(Fail, new RequestError(Fail, "The submitted form contains invalid information.", new EntityValidationException(nameof(User), request.Email, null, errors)));
            }

            User user = mapper.Map<User>(request);
            IdentityResult result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return new SignUpCommandResult(Error, new RequestError(Error, null, new Exception(Join(" ", result.Errors))));
            }

            result = await userManager.AddPasswordAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new SignUpCommandResult(Error, new RequestError(Error, null, new Exception(Join(" ", result.Errors))));
            }

            return new SignUpCommandResult(user);
        }
        catch (OperationCanceledException exception)
        {
            return new SignUpCommandResult(Cancelled, new RequestError(Error, "Request interrupted by client.", exception));
        }
        catch (Exception exception)
        {
            return new SignUpCommandResult(Error, new RequestError(Error, "Request interrupted by server.", exception));
        }
    }
}

public class SignUpCommandResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public SignUpCommandResult(User? user) : this(Success, null) => User = user;

    public User? User { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
