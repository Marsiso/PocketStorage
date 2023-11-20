using FluentValidation;
using PocketStorage.Domain.Application.DataTransferObjects;

namespace PocketStorage.Application.Application.Validators;

public class EnableAuthenticatorInputValidator : AbstractValidator<EnableAuthenticatorInput>
{
    public EnableAuthenticatorInputValidator() =>
        RuleFor(entity => entity.Code)
            .NotEmpty()
            .WithMessage("Please enter your code.");
}
