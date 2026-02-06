namespace Namespaceator.Models;

public class PrintLines
{
    public required List<PrintLine> Lines { get; set; }

    public void Add(PrintLines lines) => Lines.AddRange(lines.Lines);

    public void Print()
    {
        foreach (var line in Lines)
            line.Print();
    }
}
