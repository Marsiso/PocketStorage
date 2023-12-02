using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Core.Application.Models;
using PocketStorage.Core.Extensions;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Queries;

public class GetUsersQuery : IRequest<GetUsersQueryResult>
{
}

public class GetUsersQueryHandler(DataContext context) : IRequestHandler<GetUsersQuery, GetUsersQueryResult>
{
    private static readonly Func<DataContext, IAsyncEnumerable<User>> CompiledQuery = EF.CompileAsyncQuery((DataContext context) => context.Users.AsNoTracking()
        .Include(user => user.UserCreatedBy)
        .Include(user => user.UserUpdatedBy));

    public async Task<GetUsersQueryResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            List<User> users = await CompiledQuery(context).ToListAsync();
            List<UserDetails> details = new();

            foreach (User user in users)
            {
                UserDetails detail = new()
                {
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    UserName = user.UserName,
                    GivenName = user.GivenName,
                    MiddleName = null,
                    FamilyName = user.FamilyName,
                    Culture = user.Culture,
                    ProfilePhoto = user.ProfilePhoto
                };

                if (user.UserCreatedBy != null)
                {
                    detail.UserCreatedBy = new UserDetails
                    {
                        Email = user.UserCreatedBy.Email,
                        EmailConfirmed = user.UserCreatedBy.EmailConfirmed,
                        PhoneNumber = user.UserCreatedBy.PhoneNumber,
                        PhoneNumberConfirmed = user.UserCreatedBy.PhoneNumberConfirmed,
                        UserName = user.UserCreatedBy.UserName,
                        GivenName = user.UserCreatedBy.GivenName,
                        MiddleName = null,
                        FamilyName = user.UserCreatedBy.FamilyName,
                        Culture = user.UserCreatedBy.Culture,
                        ProfilePhoto = user.UserCreatedBy.ProfilePhoto
                    };
                }

                if (user.UserUpdatedBy != null)
                {
                    detail.UserUpdatedBy = new UserDetails
                    {
                        Email = user.UserUpdatedBy.Email,
                        EmailConfirmed = user.UserUpdatedBy.EmailConfirmed,
                        PhoneNumber = user.UserUpdatedBy.PhoneNumber,
                        PhoneNumberConfirmed = user.UserUpdatedBy.PhoneNumberConfirmed,
                        UserName = user.UserUpdatedBy.UserName,
                        GivenName = user.UserUpdatedBy.GivenName,
                        MiddleName = null,
                        FamilyName = user.UserUpdatedBy.FamilyName,
                        Culture = user.UserUpdatedBy.Culture,
                        ProfilePhoto = user.UserUpdatedBy.ProfilePhoto
                    };
                }

                details.Add(detail);
            }

            return new GetUsersQueryResult(details.ToList());
        }
        catch (OperationCanceledException exception)
        {
            return new GetUsersQueryResult(Cancelled, new RequestError(Cancelled, "Request interrupted by the client.", exception));
        }
        catch (Exception exception)
        {
            return new GetUsersQueryResult(Error, new RequestError(Error, "Request interrupted by the server.", exception));
        }
    }
}

public class GetUsersQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public GetUsersQueryResult(List<UserDetails>? result) : this(Success, null) => Result = result;

    public List<UserDetails>? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
