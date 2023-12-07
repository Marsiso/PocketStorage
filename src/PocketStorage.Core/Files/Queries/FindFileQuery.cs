using MediatR;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Core.Files.Models;
using PocketStorage.Data;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Models;
using static System.String;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.Core.Files.Queries;

public class FindFileQuery(int? folderId, string? email) : IRequest<FindFileQueryResult>
{
    public int? FolderId { get; set; } = folderId;
    public string? Email { get; set; } = email;
}

public class FindFileQueryHandler(DataContext context) : IRequestHandler<FindFileQuery, FindFileQueryResult>
{
    private static readonly Func<DataContext, string, int, Task<FileResource?>> CompiledQuery = EF.CompileAsyncQuery((DataContext context, string email, int folderId) =>
        context.Files
            .AsNoTracking()
            .Where(file => file.FolderId == folderId && file.Folder.User.Email == email)
            .Include(file => file.Folder)
            .Select(file => new FileResource
            {
                Id = file.Id,
                SafeName = file.SafeName,
                Extension = file.Extension,
                MimeType = file.MimeType,
                Size = file.Size,
                Folder = file.Folder != null ? new FolderResource { Id = file.Folder.Id, Name = file.Folder.Name, TotalCount = file.Folder.TotalCount, TotalSize = file.Folder.TotalSize } : null
            })
            .SingleOrDefault());

    public async Task<FindFileQueryResult> Handle(FindFileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!request.FolderId.HasValue || request.FolderId.Value < 1)
            {
                return new FindFileQueryResult(Fail, new RequestError(Fail, "Invalid request, please provide the folder ID.", new BadRequestException()));
            }

            if (IsNullOrWhiteSpace(request.Email))
            {
                return new FindFileQueryResult(Fail, new RequestError(EntityNotFound, "The user must be signed in to call the method.", new BadRequestException()));
            }

            FileResource? detail = await CompiledQuery(context, request.Email, request.FolderId.Value);
            if (detail == null)
            {
                return new FindFileQueryResult(
                    EntityNotFound,
                    new RequestError(EntityNotFound, "The file could not be found.", new EntityNotFoundException($"{request.Email},{(request.FolderId.HasValue ? request.FolderId.Value.ToString() : Empty)}", nameof(File))));
            }

            return new FindFileQueryResult(detail);
        }
        catch (OperationCanceledException exception)
        {
            return new FindFileQueryResult(Cancelled, new RequestError(Error, "Request interrupted by the client.", exception));
        }
        catch (Exception exception)
        {
            return new FindFileQueryResult(Error, new RequestError(Error, "Request interrupted by the server.", exception));
        }
    }
}

public class FindFileQueryResult(RequestStatus status, RequestError? error) : IRequestResult
{
    public FindFileQueryResult(FileResource? result) : this(Success, null) => Result = result;

    public FileResource? Result { get; set; }

    public RequestStatus Status { get; set; } = status;
    public RequestError? Error { get; set; } = error;
}
