namespace Infrastructure.Models.Enums;

public enum SFTPActionType
{
    Upload = 1,
    Download = 2,
    Delete = 3,
    Rename = 4,
    ListFiles = 5,
    CreateDirectory = 6,
    CheckExists = 7,
    Distribute = 8,
}
