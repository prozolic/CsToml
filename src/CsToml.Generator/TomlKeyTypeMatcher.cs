using System.Text.RegularExpressions;

namespace CsToml.Generator;

internal static class TomlKeyTypeMatcher
{
    private static readonly Regex barekeyRegex = new Regex(@"^[A-Za-z0-9_-]+$");

    public static bool IsMatchBarekey(string key)
    {
        return barekeyRegex.IsMatch(key);
    }
}