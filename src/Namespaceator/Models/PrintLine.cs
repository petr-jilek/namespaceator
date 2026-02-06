namespace Namespaceator.Models;

public class PrintLine
{
    public required List<PrintItem> Items { get; set; }

    public void Print()
    {
        foreach (var item in Items)
            item.Print();

        Console.WriteLine();
    }
}
