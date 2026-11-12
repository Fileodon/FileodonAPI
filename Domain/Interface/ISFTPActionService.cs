using Domain.DTO;
using Infrastructure.Models;

namespace Domain.Interface;

public interface ISFTPActionService
{
    public Task<bool> FileExistsAsync(SFTPConnection connection, string remoteFilePath);
    public Task<bool> UploadFileAsync(SFTPConnection connection, Stream fileStream, string remoteFilePath);
}
