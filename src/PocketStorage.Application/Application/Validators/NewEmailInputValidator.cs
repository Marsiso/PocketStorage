using FluentValidation;
using MediatR;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Enums;

namespace PocketStorage.Application.Application.Validators;

public class NewEmailInputValidator : AbstractValidator<NewEmailInput>
{
    private readonly IMediator _mediator;

    public NewEmailInputValidator(IMediator mediator)
    {
        _mediator = mediator;

        RuleFor(entity => entity.NewEmail)
            .NotEmpty()
            .WithMessage("Please enter your new email address.")
            .EmailAddress()
            .WithMessage("Your email address doesn't have valid format.")
            .MustAsync(UserDoesNotExist)
            .WithMessage("Your email address is already taken. Please choose another one.");
    }

    private async Task<bool> UserDoesNotExist(string? email, CancellationToken cancellationToken)
    {
        GetUserQueryResult result = await _mediator.Send(new GetUserQuery(), cancellationToken);
        return result is { Status: RequestStatus.EntityNotFound, Result: null };
    }
}
