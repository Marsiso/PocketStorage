using FluentValidation;
using PocketStorage.Domain.Application.DataTransferObjects;

namespace PocketStorage.Application.Application.Validators;

public class LoginWithRecoveryCodeInputValidator : AbstractValidator<LoginWithRecoveryCodeInput>
{
    public LoginWithRecoveryCodeInputValidator() =>
        RuleFor(entity => entity.RecoveryCode)
            .NotEmpty()
            .WithMessage("Please enter your recovery code.");
}
