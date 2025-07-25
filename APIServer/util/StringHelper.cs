using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace APIServer.util
{
    public class StringHelper
    {
        public static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC)
                     .Replace('đ', 'd')
                     .Replace('Đ', 'D');
        }

        // Chuẩn hoá chuỗi để so sánh
        public static string NormalizeString(string input)
        {
            if (input == null) return "";

            var noDiacritics = RemoveDiacritics(input);
            var collapsedWhitespace = Regex.Replace(noDiacritics, @"\s+", " ");
            return collapsedWhitespace.Trim().ToLowerInvariant();
        }

        // Hàm kiểm tra
        public static bool ExistsInList(string input, List<string> list)
        {
            var normalizedInput = NormalizeString(input);

            return list.Any(item => NormalizeString(item) == normalizedInput);
        }

        public static string GenerateIsbn()
        {
            return $"978-{Random.Shared.Next(100000000, 999999999)}";
        }

        public static string GenerateBarcode()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var rand = Random.Shared.Next(1000, 9999);
            return $"BC-{datePart}-{rand}";
        }

    }
}
