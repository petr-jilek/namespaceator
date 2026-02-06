namespace Namespaceator.Tree;

public class CsProjModel
{
    public required string FileNameFull { get; set; }
    public required CsDirModel ParentDir { get; set; }

    public string ProjNamespace => Path.GetFileNameWithoutExtension(FileNameFull);
}
