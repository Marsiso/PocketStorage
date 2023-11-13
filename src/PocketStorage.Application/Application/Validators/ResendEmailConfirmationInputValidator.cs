using FluentValidation;
using MediatR;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;

namespace PocketStorage.Application.Application.Validators;

public class ResendEmailConfirmationInputValidator : AbstractValidator<ResendEmailConfirmationInput>
{
    private readonly IMediator _mediator;

    public ResendEmailConfirmationInputValidator(IMediator mediator)
    {
        _mediator = mediator;

        RuleFor(entity => entity.Email)
            .NotEmpty()
            .WithMessage("Please enter your email address.")
            .EmailAddress()
            .WithMessage("Your email address doesn't have valid format.")
            .MustAsync(UserExists)
            .WithMessage("Your email address doesn't exist.");
    }

    private async Task<bool> UserExists(string? email, CancellationToken cancellationToken)
    {
        GetUserQueryResult result = await _mediator.Send(new GetUserQuery(), cancellationToken);
        if (result.Status == GetUserQueryResultStatus.Success)
        {
            return true;
        }

        return false;
    }
}
