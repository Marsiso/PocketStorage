using FluentValidation;
using MediatR;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;

namespace PocketStorage.Application.Application.Validators;

public class DeletePersonalDataInputValidator : AbstractValidator<DeletePersonalDataInput>
{
    private readonly IMediator _mediator;

    public DeletePersonalDataInputValidator(IMediator mediator)
    {
        _mediator = mediator;

        RuleFor(entity => entity.Password)
            .NotEmpty()
            .WithMessage("Please enter your password.")
            .MustAsync(HasValidPassword)
            .WithMessage("Invalid password.");
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
