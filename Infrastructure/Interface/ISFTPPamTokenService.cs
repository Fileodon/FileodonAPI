using Infrastructure.Models;

namespace Infrastructure.Interface;

public interface ISFTPIntermediaryTokenService
{
    string CreateToken(SFTPConnection destinationConnection, Guid jobId);
}
