using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Core.Files.Models;
using PocketStorage.Data;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Files.Models;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Files.Queries;

public class FindFolderQuery(string? userId, int? folderId) : IRequest<FindFolderQueryResult>
{
    public string? UserId { get; set; } = userId;
    public int? FolderId { get; set; } = folderId;
}

public class FindFolderQueryHandler(DataContext context) : IRequestHandler<FindFolderQuery, FindFolderQueryResult>
{
    public static readonly Func<DataContext, string, int, Task<FolderResource?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string userId, int id) => context.Folders
        .AsNoTracking()
        .Include(folder => folder.Parent)
        .Include(folder => folder.Files)
        .Include(folder => folder.Children)
        .Where(folder => folder.Id == id && folder.User.Id == userId)
        .Select(folder => new FolderResource
        {
            Id = folder.Id,
            Name = folder.Name,
            TotalCount = folder.TotalCount,
            TotalSize = folder.TotalSize,
            Files = folder.Files != null
                ? folder.Files.Select(file => new FileResource
                {
                    Id = file.Id,
                    SafeName = file.SafeName,
                    Extension = file.Extension,
                    MimeType = file.MimeType,
                    Size = file.Size
                }).ToList()
                : null,
            Parent = folder.Parent != null ? new FolderResource { Id = folder.Parent.Id, Name = folder.Parent.Name, TotalCount = folder.Parent.TotalCount, TotalSize = folder.Parent.TotalSize } : null,
            Children = folder.Children != null ? folder.Children.Select(child => new FolderResource { Id = folder.Id, Name = folder.Name, TotalCount = folder.TotalCount, TotalSize = folder.TotalSize }).ToList() : null
        })
        .SingleOrDefault());

    public async Task<FindFolderQueryResult> Handle(FindFolderQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (IsNullOrWhiteSpace(request.UserId))
            {
                return new FindFolderQueryResult(Fail, new RequestError(Fail, "Invalid request, please provide the user ID.", new BadRequestException()));
            }

            if (!request.FolderId.HasValue || request.FolderId.Value < 1)
            {
                return new FindFolderQueryResult(Fail, new RequestError(Fail, "Invalid request, please provide the folder ID.", new BadRequestException()));
            }

            FolderResource? summary = await CompiledQuery(context, request.UserId, request.FolderId.Value);
            if (summary == null)
            {
                return new FindFolderQueryResult(
                    EntityNotFound,
                    new RequestError(EntityNotFound, "The folder could not be found.", new EntityNotFoundException(request.FolderId.HasValue ? request.FolderId.Value.ToString() : Empty, nameof(Folder))));
            }

            return new FindFolderQueryResult(summary);
        }
        catch (OperationCanceledException exception)
        {
            return new FindFolderQueryResult(Cancelled, new RequestError(Error, "Request interrupted by the client.", exception));
        }
        catch (Exception exception)
        {
            return new FindFolderQueryResult(Error, new RequestError(Error, "Request interrupted by the server.", exception));
        }
    }
}

public class FindFolderQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public FindFolderQueryResult(FolderResource? result) : this(Success, null) => Result = result;

    public FolderResource? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
