using FluentValidation;
using MediatR;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;

namespace PocketStorage.Application.Application.Validators;

public class LoginInputValidator : AbstractValidator<LoginInput>
{
    private readonly IMediator _mediator;

    public LoginInputValidator(IMediator mediator)
    {
        _mediator = mediator;

        RuleFor(entity => entity.Email)
            .NotEmpty()
            .WithMessage("Please enter your email address.")
            .EmailAddress()
            .WithMessage("Your email address doesn't have valid format.")
            .MustAsync(HasValidEmail)
            .WithMessage("Your email address or password is invalid.")
            .MustAsync((input, _, cancellationToken) => HasValidPassword(input.Email, input.Password, cancellationToken))
            .WithMessage("Your email address or password is invalid.");

        RuleFor(entity => entity.Password)
            .NotEmpty()
            .WithMessage("Please enter your password.");
    }

    private async Task<bool> HasValidEmail(string? email, CancellationToken cancellationToken)
    {
        VerifyEmailExistsQueryResult result = await _mediator.Send(new VerifyEmailExistsQuery(email), cancellationToken);
        return result is { Status: VerifyEmailExistsQueryResultStatus.Success, Result: true };
    }

    private async Task<bool> HasValidPassword(string? email, string? password, CancellationToken cancellationToken)
    {
        VerifyPasswordQueryResult result = await _mediator.Send(new VerifyPasswordQuery(email, password), cancellationToken);
        return result is { Status: VerifyPasswordQueryResultStatus.Success, Result: true };
    }
}
