using FluentValidation;
using PocketStorage.Core.Application.Commands;

namespace PocketStorage.Application.Application.Validators;

public class CreateUserCommandValidator : AbstractValidator<SignUpCommand>
{
    public CreateUserCommandValidator()
    {
    }
}
