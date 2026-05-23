using System.Text.RegularExpressions;

namespace MyApp.Helpers;

public static class ExtensionMethod
{
    public static bool IsMobile(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return false;

        if (Regex.IsMatch(userAgent, "(tablet|ipad|playbook|silk)|(android(?!.*mobile))", RegexOptions.IgnoreCase))
            return true;

        const string mobileRegex =
            "blackberry|iphone|mobile|windows ce|opera mini|htc|sony|palm|symbianos|ipad|ipod|blackberry|bada|kindle|symbian|sonyericsson|android|samsung|nokia|wap|motor";

        return Regex.IsMatch(userAgent, mobileRegex, RegexOptions.IgnoreCase);
    }
}
