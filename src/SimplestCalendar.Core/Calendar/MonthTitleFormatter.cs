using System.Globalization;
using System.Text;

namespace SimplestCalendar.Core.Calendar;

public static class MonthTitleFormatter
{
    /// <summary>
    /// Supports tokens: MMMM, MMM, MM, yyyy, yy. Invalid/empty → MMMM.
    /// </summary>
    public static string Format(DateOnly monthStart, string? pattern, CultureInfo? culture = null)
    {
        culture ??= new CultureInfo("en-US");
        var p = string.IsNullOrWhiteSpace(pattern) ? "MMMM" : pattern.Trim();
        try
        {
            // Replace longest tokens first via a simple scanner.
            var dt = new DateTime(monthStart.Year, monthStart.Month, 1);
            var sb = new StringBuilder();
            for (var i = 0; i < p.Length;)
            {
                if (Match(p, i, "MMMM")) { sb.Append(dt.ToString("MMMM", culture)); i += 4; }
                else if (Match(p, i, "MMM")) { sb.Append(dt.ToString("MMM", culture)); i += 3; }
                else if (Match(p, i, "MM")) { sb.Append(dt.ToString("MM", culture)); i += 2; }
                else if (Match(p, i, "yyyy")) { sb.Append(dt.ToString("yyyy", culture)); i += 4; }
                else if (Match(p, i, "yy")) { sb.Append(dt.ToString("yy", culture)); i += 2; }
                else { sb.Append(p[i]); i++; }
            }

            var result = sb.ToString();
            return string.IsNullOrWhiteSpace(result) ? dt.ToString("MMMM", culture) : result;
        }
        catch
        {
            return new DateTime(monthStart.Year, monthStart.Month, 1).ToString("MMMM", culture);
        }
    }

    private static bool Match(string s, int index, string token) =>
        index + token.Length <= s.Length &&
        string.CompareOrdinal(s, index, token, 0, token.Length) == 0;
}
