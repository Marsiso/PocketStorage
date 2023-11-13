using FluentValidation;
using MediatR;
using PocketStorage.Application.Extensions;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;

namespace PocketStorage.Application.Application.Validators;

public class RegisterInputValidator : AbstractValidator<RegisterInput>
{
    private readonly IMediator _mediator;

    public RegisterInputValidator(IMediator mediator)
    {
        _mediator = mediator;

        RuleFor(entity => entity.GivenName)
            .NotEmpty()
            .WithMessage("Please enter your given name.")
            .MaximumLength(256)
            .WithMessage("Your given name must be at most 256 characters long.");

        RuleFor(entity => entity.FamilyName)
            .NotEmpty()
            .WithMessage("Please enter your family name.")
            .MaximumLength(256)
            .WithMessage("Your family name must be at most 256 characters long.");

        RuleFor(entity => entity.Email)
            .NotEmpty()
            .WithMessage("Please enter your email address.")
            .EmailAddress()
            .WithMessage("Your email address doesn't have valid format.")
            .MustAsync(UserDoesNotExist)
            .WithMessage("Your email address is already taken. Please choose another one.")
            .MaximumLength(256)
            .WithMessage("Your email address must be at most 256 characters long.");

        RuleFor(entity => entity.Password)
            .NotEmpty()
            .WithMessage("Please enter your password.")
            .MinimumLength(10)
            .WithMessage("Your password length must be at least 10.")
            .HasUpperCaseCharacter()
            .WithMessage("Your password must contain at least one uppercase letter.")
            .HasLowerCaseCharacter()
            .WithMessage("Your password must contain at least one lowercase letter.")
            .HasNumericCharacter()
            .WithMessage("Your password must contain at least one number.")
            .HasSpecialCharacter()
            .WithMessage("Your password must contain at least one special character.");

        RuleFor(entity => entity.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Please repeat your password.")
            .Equal(entity => entity.Password)
            .WithMessage("Passwords must match.");
    }

    private async Task<bool> UserDoesNotExist(string? email, CancellationToken cancellationToken)
    {
        GetUserQueryResult result = await _mediator.Send(new GetUserQuery(), cancellationToken);
        if (result.Status == GetUserQueryResultStatus.Fail)
        {
            return true;
        }

        return false;
    }
}
