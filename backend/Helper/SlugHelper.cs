using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Backend.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Normalize tiếng Việt không dấu
            string normalized = RemoveVietnameseTones(input);
            
            // Lowercase
            normalized = normalized.ToLowerInvariant();
            
            // Replace spaces và ký tự đặc biệt với dấu gạch ngang
            normalized = Regex.Replace(normalized, @"[^a-z0-9\s-]", "");
            normalized = Regex.Replace(normalized, @"\s+", "-");
            normalized = Regex.Replace(normalized, @"-+", "-");
            
            // Trim dấu gạch ngang ở đầu/cuối
            return normalized.Trim('-');
        }

        private static string RemoveVietnameseTones(string input)
        {
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    input = input.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            return input;
        }
    }
}