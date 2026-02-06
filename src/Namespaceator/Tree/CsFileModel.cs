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
    // Only replaces the first namespace declaration found.
    private static readonly Regex NamespaceDeclRegex = new(
        pattern: @"(?m)^(?<indent>[ \t]*)namespace[ \t]+(?<ns>[A-Za-z_][A-Za-z0-9_\.]*)[ \t]*(?<term>;|\{)[ \t]*$",
        options: RegexOptions.Compiled
    );

    // IMPORTANT: This regex is written to NEVER consume newlines around the using directive,
    // which prevents "random" removal of blank lines when we replace the match.
    // Captures optional EOL so we can preserve it.
    private static readonly Regex UsingLineRegex = new(
        pattern: @"(?m)^(?<indent>[ \t]*)(?<global>global[ \t]+)?using[ \t]+(?<body>[^;\r\n]+)[ \t]*;[ \t]*(?<eol>\r?\n)?$",
        options: RegexOptions.Compiled
    );

    public async Task<List<NamespaceChange>> UpdateNamespacesAsync()
    {
        var text = await File.ReadAllTextAsync(PathFull).ConfigureAwait(false);

        var match = NamespaceDeclRegex.Match(text);
        if (!match.Success)
            return [];

        var oldNs = match.Groups["ns"].Value;
        var targetNs = TargetNamespace;

        var change = new NamespaceChange { OldNamespace = oldNs, TargetNamespace = targetNs };

        if (!change.IsNoChange)
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
        var changes = namespaceChanges.Where(c => !c.IsNoChange).ToList();
        if (changes.Count == 0)
            return;

        var text = await File.ReadAllTextAsync(PathFull).ConfigureAwait(false);
        var original = text;

        text = UsingLineRegex.Replace(
            text,
            m =>
            {
                var indent = m.Groups["indent"].Value;
                var globalKw = m.Groups["global"].Success ? "global " : "";
                var body = m.Groups["body"].Value.Trim();
                var eol = m.Groups["eol"].Success ? m.Groups["eol"].Value : "";

                // body examples:
                //   Foo.Bar
                //   static Foo.Bar.Baz
                //   Alias = Foo.Bar
                //   Alias = global::Foo.Bar.Baz
                //
                // Policy:
                // - Leave "static ..." untouched (could be type references; risky).
                // - Update RHS of alias (after '=') for namespace prefix matches.
                // - Update non-alias `using Foo.Bar;` only when it exactly matches OldNamespace.
                // - Also update `global::Old...` in the same cases (only within using lines).

                if (body.StartsWith("static ", StringComparison.Ordinal))
                    return m.Value; // preserve original exactly

                string updatedBody = body;

                var eqIndex = body.IndexOf('=');
                if (eqIndex >= 0)
                {
                    // Alias form: "Alias = Something"
                    var left = body[..eqIndex].TrimEnd();
                    var right = body[(eqIndex + 1)..].TrimStart();

                    foreach (var ch in changes)
                        right = ReplaceNamespacePrefixInUsingRhs(right, ch.OldNamespace, ch.TargetNamespace);

                    updatedBody = $"{left} = {right}";
                }
                else
                {
                    // Non-alias: update only exact matches
                    foreach (var ch in changes)
                    {
                        if (string.Equals(updatedBody, ch.OldNamespace, StringComparison.Ordinal))
                        {
                            updatedBody = ch.TargetNamespace;
                            break;
                        }

                        if (string.Equals(updatedBody, $"global::{ch.OldNamespace}", StringComparison.Ordinal))
                        {
                            updatedBody = $"global::{ch.TargetNamespace}";
                            break;
                        }
                    }
                }

                // Reconstruct and preserve original EOL (prevents accidental blank-line loss)
                return $"{indent}{globalKw}using {updatedBody};{eol}";
            }
        );

        if (!string.Equals(text, original, StringComparison.Ordinal))
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

        const string globalPrefix = "global::";
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
