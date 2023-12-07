using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Core.Application.Models;
using PocketStorage.Core.Extensions;
using PocketStorage.Data;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Application.Queries;

public class GetUsersQuery(UserQueryString queryString) : IRequest<GetUsersQueryResult>
{
    public UserQueryString QueryString { get; set; } = queryString;
}

public class GetUsersQueryHandler(DataContext context) : IRequestHandler<GetUsersQuery, GetUsersQueryResult>
{
    private static readonly Func<DataContext, int, int, IAsyncEnumerable<UserResource>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, int skip, int take) =>
        context.Users
            .AsNoTracking()
            .Include(user => user.UserCreatedBy)
            .Include(user => user.UserUpdatedBy)
            .Skip(skip)
            .Take(take)
            .Select(user => new UserResource
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
                ProfilePhoto = user.ProfilePhoto,
                UserCreatedBy = user.UserCreatedBy != null
                    ? new UserResource
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
                    }
                    : null,
                UserUpdatedBy = user.UserUpdatedBy != null
                    ? new UserResource
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
                    }
                    : null
            }));

    private static readonly Func<DataContext, Task<int>> CompiledCountQuery = EF.CompileAsyncQuery((DataContext context) =>
        context.Users
            .AsNoTracking()
            .Count());

    public async Task<GetUsersQueryResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return new GetUsersQueryResult(new PagedList<UserResource>(
                await CompiledQuery(context, request.QueryString.RecordsOffset(), request.QueryString.RecordsToReturn()).ToListAsync(),
                await CompiledCountQuery(context),
                request.QueryString.PageNumber,
                request.QueryString.PageSize));
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
    public GetUsersQueryResult(PagedList<UserResource>? result) : this(Success, null) => Result = result;
    public PagedList<UserResource>? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
