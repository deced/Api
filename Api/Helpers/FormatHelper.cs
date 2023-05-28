using System.Text.RegularExpressions;

namespace Api.Helpers;

public static class FormatHelper
{
    public static string ConvertToCode(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var specialCharacters = "[ -]+";
        return Regex.Replace(value, specialCharacters, "-").Trim('-').ToLower();
    }
}