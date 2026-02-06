namespace Namespaceator.Models;

public class NamespaceChange
{
    public required string OldNamespace { get; set; }
    public required string TargetNamespace { get; set; }

    public bool IsNoChange => OldNamespace == TargetNamespace;
}
