using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TechCorner_ECommerce.Helpers {
    public class SlugGenerate {
        public static string GenerateSlug(string phrase) {
            // convert to lower
            string str = phrase.ToLowerInvariant();

            // remove dấu tiếng Việt
            str = RemoveDiacritics(str);

            // remove ký tự đặc biệt
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // replace space -> -
            str = Regex.Replace(str, @"\s+", "-").Trim();

            // remove multiple -
            str = Regex.Replace(str, @"-+", "-");

            return str;
        }

        private static string RemoveDiacritics(string text) {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized) {
                var unicodeCategory = Char.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
