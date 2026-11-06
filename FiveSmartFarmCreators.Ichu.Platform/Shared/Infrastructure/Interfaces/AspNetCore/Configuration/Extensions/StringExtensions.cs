using System.Text.RegularExpressions;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration.Extensions;

/// <summary>
///     Text conversions used by the ASP.NET Core conventions.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    ///     Converts a PascalCase or camelCase text to lowercase kebab-case.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>The kebab-case text, or the same text when it is null or empty.</returns>
    public static string ToKebabCase(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return KebabCaseRegex().Replace(text, "-$1").Trim().ToLowerInvariant();
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])")]
    private static partial Regex KebabCaseRegex();
}
