using System.Text.RegularExpressions;
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

    // Matches:
    //   namespace Foo.Bar;
    //   namespace Foo.Bar {
    // (supports attributes/whitespace above due to multiline scanning, but only replaces the first declaration found)
    private static readonly Regex NamespaceDeclRegex = new(
        pattern: @"(?m)^(?<indent>\s*)namespace\s+(?<ns>[A-Za-z_][A-Za-z0-9_\.]*)\s*(?<term>;|\{)\s*$",
        options: RegexOptions.Compiled
    );

    // Matches using directives at start of line (including global using), without trying to parse the whole language.
    // Captures the right-hand target (after '=' if alias, otherwise the namespace/type).
    private static readonly Regex UsingLineRegex = new(
        pattern: @"(?m)^(?<indent>\s*)(?<global>global\s+)?using\s+(?<body>[^;]+)\s*;\s*$",
        options: RegexOptions.Compiled
    );

    public async Task<List<NamespaceChange>> UpdateNamespacesAsync()
    {
        var text = await File.ReadAllTextAsync(PathFull).ConfigureAwait(false);

        var match = NamespaceDeclRegex.Match(text);
        if (match.Success is false)
            return [];

        var oldNs = match.Groups["ns"].Value;
        var targetNs = TargetNamespace;

        var change = new NamespaceChange { OldNamespace = oldNs, TargetNamespace = targetNs };

        if (change.IsNoChange is false)
        {
            // Replace only the namespace identifier, keep indentation + terminator (; or {) exactly as-is.
            var nsStart = match.Groups["ns"].Index;
            var nsLen = match.Groups["ns"].Length;

            text = text.Remove(nsStart, nsLen).Insert(nsStart, targetNs);

            await File.WriteAllTextAsync(PathFull, text).ConfigureAwait(false);
        }

        return [change];
    }

    public async Task UpdateUsingsAsync(List<NamespaceChange> namespaceChanges)
    {
        if (namespaceChanges is null || namespaceChanges.Count == 0)
            return;

        // Only changes that actually change something
        var changes = namespaceChanges.Where(c => c.IsNoChange is false).ToList();
        if (changes.Count == 0)
            return;

        var text = await File.ReadAllTextAsync(PathFull).ConfigureAwait(false);
        var original = text;

        // Update using lines one by one (safer than global search/replace).
        text = UsingLineRegex.Replace(
            text,
            m =>
            {
                var indent = m.Groups["indent"].Value;
                var globalKw = m.Groups["global"].Success ? "global " : "";
                var body = m.Groups["body"].Value.Trim();

                // body examples:
                //   Foo.Bar
                //   static Foo.Bar.Baz
                //   Alias = Foo.Bar
                //   Alias = global::Foo.Bar.Baz
                //
                // We will:
                // - Leave "static ..." untouched (namespace changes there are ambiguous).
                // - Update RHS of alias (after '=') for namespace prefix matches.
                // - Update non-alias plain `using Foo.Bar` when it exactly matches OldNamespace.
                // - Also update `global::Old...` prefix on RHS (still only in using lines).

                if (body.StartsWith("static ", StringComparison.Ordinal))
                    return m.Value; // do nothing

                string updatedBody = body;

                // Alias form?
                var eqIndex = body.IndexOf('=');
                if (eqIndex >= 0)
                {
                    var left = body[..eqIndex].TrimEnd();
                    var right = body[(eqIndex + 1)..].TrimStart();

                    // Normalize optional global:: prefix (we can update it too, but only when it refers to the old namespace)
                    foreach (var ch in changes)
                        right = ReplaceNamespacePrefixInUsingRhs(right, ch.OldNamespace, ch.TargetNamespace);

                    updatedBody = $"{left} = {right}";
                }
                else
                {
                    // Non-alias: only update if it matches exactly the old namespace.
                    foreach (var ch in changes)
                    {
                        if (string.Equals(updatedBody, ch.OldNamespace, StringComparison.Ordinal))
                        {
                            updatedBody = ch.TargetNamespace;
                            break;
                        }

                        // Also handle `global::OldNamespace` exactly
                        if (string.Equals(updatedBody, $"global::{ch.OldNamespace}", StringComparison.Ordinal))
                        {
                            updatedBody = $"global::{ch.TargetNamespace}";
                            break;
                        }
                    }
                }

                // Reconstruct the using line, preserving indent/global using
                return $"{indent}{globalKw}using {updatedBody};";
            }
        );

        if (string.Equals(text, original, StringComparison.Ordinal) is false)
            await File.WriteAllTextAsync(PathFull, text).ConfigureAwait(false);
    }

    private static string ReplaceNamespacePrefixInUsingRhs(string rhs, string oldNs, string targetNs)
    {
        // rhs might be:
        //   Foo.Bar
        //   Foo.Bar.Something
        //   global::Foo.Bar
        //   global::Foo.Bar.Something
        //
        // Replace only if rhs is exactly oldNs OR starts with oldNs + "." (and same with global:: prefix)

        if (string.Equals(rhs, oldNs, StringComparison.Ordinal))
            return targetNs;

        if (rhs.StartsWith(oldNs + ".", StringComparison.Ordinal))
            return string.Concat(targetNs, rhs.AsSpan(oldNs.Length));

        var globalPrefix = "global::";
        if (rhs.StartsWith(globalPrefix, StringComparison.Ordinal))
        {
            var after = rhs.Substring(globalPrefix.Length);

            if (string.Equals(after, oldNs, StringComparison.Ordinal))
                return globalPrefix + targetNs;

            if (after.StartsWith(oldNs + ".", StringComparison.Ordinal))
                return globalPrefix + targetNs + after.Substring(oldNs.Length);
        }

        return rhs;
    }
}
