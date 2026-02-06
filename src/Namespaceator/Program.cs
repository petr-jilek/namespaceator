using Namespaceator.Tree;

var path = args.Length > 0 ? args[0] : null;
var print = args.Contains("--print");

if (string.IsNullOrEmpty(path))
{
    Console.WriteLine("Please provide a path to a directory.");
    return;
}
if (Directory.Exists(path) is false)
{
    Console.WriteLine("The provided path does not exist or is not a directory.");
    return;
}

var root = new CsDirModel
{
    DirPathFull = Path.GetFullPath(path),
    DirName = new DirectoryInfo(path).Name,
    SubDirs = [],
    CsProjs = [],
    CsFiles = [],
};

Console.WriteLine($"Processing path: {path}");
Console.WriteLine($"Processing path full: {root.DirPathFull}");

root.Mutable_Fill();

if (print)
{
    var lines = root.GetTreePrintLines();
    lines.Print();

    return;
}

await root.UpdateNamespacesAndUsingsAsync();
