namespace Namespaceator.Models;

public class PrintLine
{
    public required string Text { get; set; }
    public required ConsoleColor Color { get; set; }

    public void Print()
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = Color;
        Console.WriteLine(Text);
        Console.ForegroundColor = previousColor;
    }
}
