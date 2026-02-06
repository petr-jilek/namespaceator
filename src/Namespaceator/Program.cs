using Namespaceator.Tree;

var path = args.Length > 0 ? args[0] : null;
if (string.IsNullOrEmpty(path))
{
    Console.WriteLine("Please provide a path to a .cs file or a directory containing .cs files.");
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

var lines = root.GetTreePrintLines();
lines.Print();
