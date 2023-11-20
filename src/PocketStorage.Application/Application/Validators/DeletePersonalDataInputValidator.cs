using FluentValidation;
using MediatR;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;

namespace PocketStorage.Application.Application.Validators;

public class DeletePersonalDataInputValidator : AbstractValidator<DeletePersonalDataInput>
{
    private readonly IMediator _mediator;

    public DeletePersonalDataInputValidator(IMediator mediator)
    {
        _mediator = mediator;

        RuleFor(entity => entity.Password)
            .NotEmpty()
            .WithMessage("Please enter your password.")
            .MustAsync(HasValidPassword)
            .WithMessage("Invalid password.");
    }

    private async Task<bool> HasValidPassword(string? password, CancellationToken cancellationToken)
    {
        ApiCallResponse<User> userResult = await _mediator.Send(new GetUserQuery(), cancellationToken);
        if (userResult.Status != RequestStatus.Success)
        {
            return false;
        }

        ApiCallResponse<bool> verificationResult = await _mediator.Send(new VerifyPasswordQuery(userResult.Result.Email, password), cancellationToken);
        return verificationResult is { Status: RequestStatus.Success, Result: true };
    }
}
