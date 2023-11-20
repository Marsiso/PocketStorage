using FluentValidation;
using PocketStorage.Domain.Application.DataTransferObjects;

namespace PocketStorage.Application.Application.Validators;

public class PhoneInputValidator : AbstractValidator<PhoneInput>
{
    public PhoneInputValidator() =>
        RuleFor(entity => entity.PhoneNumber)
            .NotEmpty()
            .WithMessage("Please enter your phone number.");
}
