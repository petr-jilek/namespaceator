namespace Namespaceator.Models;

public class CsDirModel
{
    public required string DirPathFull { get; set; }
    public required string DirName { get; set; }
    public CsDirModel? ParentDir { get; set; }
    public required List<CsDirModel> SubDirs { get; set; }
    public required List<CsProjModel> CsProjs { get; set; }
    public required List<CsFileModel> CsFiles { get; set; }

    public void Mutable_Fill()
    {
        var dirInfo = new DirectoryInfo(DirPathFull);

        var csProjFiles = dirInfo.GetFiles("*.csproj");
        foreach (var csProjFile in csProjFiles)
            CsProjs.Add(new() { FileNameFull = csProjFile.Name, ParentDir = this });

        var csFiles = dirInfo.GetFiles("*.cs");
        foreach (var csFile in csFiles)
            CsFiles.Add(new() { FileNameFull = csFile.Name, ParentDir = this });

        var subDirs = dirInfo.GetDirectories();
        foreach (var subDir in subDirs)
            SubDirs.Add(
                new CsDirModel
                {
                    DirPathFull = subDir.FullName,
                    DirName = subDir.Name,
                    ParentDir = this,
                    SubDirs = [],
                    CsFiles = [],
                    CsProjs = [],
                }
            );

        foreach (var subDirModel in SubDirs)
            subDirModel.Mutable_Fill();
    }
}
