using FluentValidation;
using PocketStorage.Domain.Application.DataTransferObjects;

namespace PocketStorage.Application.Application.Validators;

public class LoginWithTwoFactorAuthInput : AbstractValidator<LoginWithRecoveryCodeInput>
{
    public LoginWithTwoFactorAuthInput() =>
        RuleFor(entity => entity.RecoveryCode)
            .NotEmpty()
            .WithMessage("Please enter your recovery code.");
}
