using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TradeHub.BLL.Utils
{
    public static class NormalizeName
    {
        public static string Normalize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            name = name.ToLowerInvariant();

            string normalized = name.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                var unicodeCategory = Char.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            string result = sb.ToString().Normalize(NormalizationForm.FormC);

            result = result.Replace("đ", "d");

            result = Regex.Replace(result, @"\s+", " ").Trim();

            return result;
        }
    }
}