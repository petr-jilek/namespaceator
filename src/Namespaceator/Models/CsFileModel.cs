namespace Namespaceator.Models;

public class CsFileModel
{
    public required string FileNameFull { get; set; }
    public required CsDirModel ParentDir { get; set; }
}
