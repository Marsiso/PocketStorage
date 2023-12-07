using FluentValidation;
using MediatR;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Enums;

namespace PocketStorage.Application.Application.Validators;

public class ForgotPasswordInputValidator : AbstractValidator<ForgotPasswordInput>
{
    private readonly IMediator _mediator;

    public ForgotPasswordInputValidator(IMediator mediator)
    {
        _mediator = mediator;

        RuleFor(entity => entity.Email)
            .NotEmpty()
            .WithMessage("Please enter your email address.")
            .EmailAddress()
            .WithMessage("Your email address doesn't have valid format.")
            .MustAsync(UserExists)
            .WithMessage("Your email address isn't associated with any account.");
    }

    private async Task<bool> UserExists(string? email, CancellationToken cancellationToken)
    {
        GetUserQueryResult userResult = await _mediator.Send(new GetUserQuery(), cancellationToken);
        return userResult is { Status: RequestStatus.Success, Result: not null };
    }
}
