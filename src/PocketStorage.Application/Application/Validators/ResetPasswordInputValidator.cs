using FluentValidation;
using PocketStorage.Domain.Application.DataTransferObjects;

namespace PocketStorage.Application.Application.Validators;

public class ResetPasswordInputValidator : AbstractValidator<ResetPasswordInput>
{
    public ResetPasswordInputValidator()
    {
    }
}
