namespace Namespaceator.Models;

public class PrintItem
{
    public required string Text { get; set; }
    public required ConsoleColor Color { get; set; }

    public void Print()
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = Color;
        Console.Write(Text);
        Console.ForegroundColor = previousColor;
    }
}
