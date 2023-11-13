using System.Net;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;

namespace PocketStorage.Core.Application.Commands;

public record SignUpCommand(string? UserName, string? GivenName, string? FamilyName, string? Email, string? Phone, string? Password, string? Culture, string? ProfilePhoto) : IRequest<SignUpCommandResult>;

public class SignUpCommandHandler : IRequestHandler<SignUpCommand, SignUpCommandResult>
{
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<SignUpCommand> _validator;

    public SignUpCommandHandler(UserManager<User> userManager, IValidator<SignUpCommand> validator, IMapper mapper)
    {
        _userManager = userManager;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<SignUpCommandResult> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Dictionary<string, string[]> errors = (await _validator.ValidateAsync(new ValidationContext<SignUpCommand>(request), cancellationToken)).DistinctErrorsByProperty();
            if (errors.Count > 0)
            {
                return new SignUpCommandResult(SignUpCommandResultStatus.Fail, new ApiCallError(new EntityValidationException(nameof(User), request.Email, null, errors)));
            }

            User user = _mapper.Map<User>(request);
            IdentityResult result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return new SignUpCommandResult(SignUpCommandResultStatus.InternalServerError, new ApiCallError(HttpStatusCode.InternalServerError, null));
            }

            result = await _userManager.AddPasswordAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new SignUpCommandResult(SignUpCommandResultStatus.InternalServerError, new ApiCallError(HttpStatusCode.InternalServerError, null));
            }

            return new SignUpCommandResult(SignUpCommandResultStatus.Success, user);
        }
        catch (OperationCanceledException exception)
        {
            return new SignUpCommandResult(SignUpCommandResultStatus.OperationCancelled, new ApiCallError(499, exception));
        }
        catch (Exception exception)
        {
            return new SignUpCommandResult(SignUpCommandResultStatus.InternalServerError, new ApiCallError(HttpStatusCode.InternalServerError, exception));
        }
    }
}

public class SignUpCommandResult
{
    public SignUpCommandResult(SignUpCommandResultStatus status, User? result)
    {
        Status = status;
        Result = result;
    }

    public SignUpCommandResult(SignUpCommandResultStatus status, ApiCallError? error)
    {
        Status = status;
        Error = error;
    }

    public SignUpCommandResult(ApiCallError? error) => Error = error;

    public SignUpCommandResultStatus Status { get; set; }
    public User? Result { get; set; }
    public ApiCallError? Error { get; set; }
}

public enum SignUpCommandResultStatus
{
    Success,
    Fail,
    OperationCancelled,
    InternalServerError
}
