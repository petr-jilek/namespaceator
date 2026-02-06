using System.Threading.Tasks;
using Namespaceator.Models;

namespace Namespaceator.Tree;

public class CsFileModel
{
    public required string FileNameFull { get; set; }
    public required CsDirModel ParentDir { get; set; }

    public List<string> TargetNamespaces
    {
        get
        {
            var namespaces = new List<string>();

            var dir = ParentDir;
            while (true)
            {
                var csProj = dir.CsProjs.FirstOrDefault();
                if (csProj is not null)
                {
                    namespaces.Add(csProj.ProjNamespace);
                    break;
                }

                namespaces.Add(dir.DirName);

                dir = dir.ParentDir;
                if (dir is null)
                    break;
            }

            if (dir is null)
                throw new InvalidOperationException($"{FileNameFull} is not inside any .csproj!");

            namespaces.Reverse();

            return namespaces;
        }
    }

    public string TargetNamespace => string.Join('.', TargetNamespaces);

    public string PathFull => Path.Combine(ParentDir.DirPathFull, FileNameFull);

    public async Task<List<NamespaceChange>> UpdateNamespacesAsync()
    {
        var text = await File.ReadAllTextAsync(PathFull);

        var namespaceChanges = new List<NamespaceChange>();

        return namespaceChanges;
    }

    public async Task UpdateUsingsAsync(List<NamespaceChange> namespaceChanges)
    {
        var text = await File.ReadAllTextAsync(PathFull);
    }
}
