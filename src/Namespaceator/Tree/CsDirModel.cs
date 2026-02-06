using Namespaceator.Models;

namespace Namespaceator.Tree;

public class CsDirModel
{
    public required string DirPathFull { get; set; }
    public required string DirName { get; set; }
    public CsDirModel? ParentDir { get; set; }
    public required List<CsDirModel> SubDirs { get; set; }
    public required List<CsProjModel> CsProjs { get; set; }
    public required List<CsFileModel> CsFiles { get; set; }

    public void Mutable_Fill(HashSet<string>? excludeDirs = null)
    {
        excludeDirs ??= ["bin", "obj", ".git", ".vs"];

        var dirInfo = new DirectoryInfo(DirPathFull);

        var csProjFiles = dirInfo.GetFiles("*.csproj").OrderBy(f => f.Name).ToList();
        foreach (var csProjFile in csProjFiles)
            CsProjs.Add(new() { FileNameFull = csProjFile.Name, ParentDir = this });

        var csFiles = dirInfo.GetFiles("*.cs").OrderBy(f => f.Name).ToList();
        foreach (var csFile in csFiles)
            CsFiles.Add(new() { FileNameFull = csFile.Name, ParentDir = this });

        var subDirs = dirInfo
            .GetDirectories()
            .Where(d => excludeDirs.Contains(d.Name) is false)
            .OrderBy(d => d.Name)
            .ToList();
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

    public PrintLines GetTreePrintLines(int indentLevel = 0)
    {
        var indentChar = ' ';
        var indent = new string(indentChar, indentLevel * 2);

        var result = new PrintLines { Lines = [new() { Text = $"{indent}{DirName}", Color = ConsoleColor.Cyan }] };

        foreach (var subDir in SubDirs)
            result.Add(subDir.GetTreePrintLines(indentLevel + 1));

        foreach (var csProj in CsProjs)
            result.Lines.Add(new() { Text = $"{indent}{indentChar}{csProj.FileNameFull}", Color = ConsoleColor.Red });
        foreach (var csFile in CsFiles)
            result.Lines.Add(new() { Text = $"{indent}{indentChar}{csFile.FileNameFull}", Color = ConsoleColor.Green });

        return result;
    }
}
