using Infrastructure.Models;

namespace Infrastructure.Interface;

public interface ISFTPPamTokenService
{
    string CreateToken(SFTPConnection connection, Guid jobId);
}
