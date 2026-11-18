using Controller.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Controller.DTO.SFTPAction;

public record class SFTPActionDistributeRequest(
    [ModelBinder(typeof(JsonModelBinder<List<SFTPActionDistributeLocation>>))]
    List<SFTPActionDistributeLocation> Locations,
    List<IFormFile> Files
);

public record class SFTPActionDistributeLocation(
    int SftpConnectionId,
    string RemotePath
);

public record SFTPActionDistributeLocationRequest(int SftpConnectionId, string RemotePath);