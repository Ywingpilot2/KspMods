using SteelLanguage.Extensions;

namespace ProgrammableMod.Extensions;

public static class StringExtension
{
    private static readonly Replacer[] Replacers = 
    {
        new("{", "|bra|"),
        new("}", "|arb|"),
        new("\t", "|t|"),
        new("[", "|[|"),
        new("]", "|]|"),
        new("//", "|/|")
    };
    
    private readonly record struct Replacer(string A, string B)
    {
        public string A { get; } = A;
        public string B { get; } = B;
    }

    public static string ConfigClean(this string dirty)
    {
        dirty = dirty.Trim('\r', '\n');

        foreach (Replacer replacer in Replacers)
        {
            dirty = dirty.Replace(replacer.A, replacer.B);
        }

        return dirty;
    }

    public static string ConfigDirty(this string clean)
    {
        foreach (Replacer replacer in Replacers)
        {
            clean = clean.Replace(replacer.B, replacer.A);
        }

        return clean;
    }
}