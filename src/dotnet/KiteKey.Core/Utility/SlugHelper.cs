using System.Globalization;
using System.Text.RegularExpressions;

namespace KiteKey.Core.Utility;

/// <summary>General-purpose URL slug transformations.</summary>
public static partial class SlugHelper
{
    public static string AddQueryStringValue(string queryString, string parameterName, string parameterValue)
    {
        ArgumentNullException.ThrowIfNull(queryString);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
        if (string.IsNullOrEmpty(parameterValue))
            return queryString;
        string separator = string.IsNullOrEmpty(queryString) ? "?" : queryString.EndsWith('?') || queryString.EndsWith('&') ? "" : "&";
        return queryString + separator + Uri.EscapeDataString(parameterName) + "=" + Uri.EscapeDataString(parameterValue);
    }

    public static string CreateSlugFromName(string? name)
        => name is null ? "" : name.RemovePunctuation().CollapseWhitespace().Replace(' ', '-').ToLowerInvariant();

    public static string GetNameFromSlug(string slug)
        => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(slug.Replace('-', ' '));

    public static string Increment(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        Match match = SuffixRegex().Match(slug);
        if (!match.Success)
            return slug + "-2";
        int next = checked(int.Parse(match.Groups["suffix"].Value, CultureInfo.InvariantCulture) + 1);
        return $"{match.Groups["root"].Value}-{next}";
    }

    [GeneratedRegex(@"^(?<root>.+)-(?<suffix>[2-9][0-9]*)$", RegexOptions.CultureInvariant)]
    private static partial Regex SuffixRegex();
}
