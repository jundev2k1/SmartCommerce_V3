using System.Text.RegularExpressions;

namespace BuildingBlock.SharedKernel.RegexPatterns;

public static partial class RegexPatterns
{
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    public static partial Regex Slug();
}
