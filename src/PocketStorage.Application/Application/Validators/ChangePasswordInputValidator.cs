using FluentValidation;
using MediatR;
using PocketStorage.Application.Extensions;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;

namespace PocketStorage.Application.Application.Validators;

public class ChangePasswordInputValidator : AbstractValidator<ChangePasswordInput>
{
    private readonly IMediator _mediator;

    public ChangePasswordInputValidator(IMediator mediator)
    {
        _mediator = mediator;

        RuleFor(entity => entity.OldPassword)
            .NotEmpty()
            .WithMessage("Please enter your old password.")
            .MustAsync(HasValidPassword)
            .WithMessage("Your old password doesn't match.");

        RuleFor(entity => entity.NewPassword)
            .NotEmpty()
            .WithMessage("Please enter your new password.")
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
            .WithMessage("Please enter your confirmation password.")
            .Equal(entity => entity.NewPassword)
            .WithMessage("Passwords must match.");
    }

    private async Task<bool> HasValidPassword(string? password, CancellationToken cancellationToken)
    {
        GetUserQueryResult userResult = await _mediator.Send(new GetUserQuery(), cancellationToken);
        if (userResult.Status != GetUserQueryResultStatus.Success)
        {
            return false;
        }

        VerifyPasswordQueryResult result = await _mediator.Send(new VerifyPasswordQuery(userResult.Result.Email, password), cancellationToken);
        return result is { Status: VerifyPasswordQueryResultStatus.Success, Result: true };
    }
}
