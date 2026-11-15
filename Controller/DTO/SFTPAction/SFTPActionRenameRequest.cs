namespace Controller.DTO.SFTPAction;

public record class SFTPActionRenameRequest(
    string OldRemoteFilePath,
    string NewRemoteFilePath
);