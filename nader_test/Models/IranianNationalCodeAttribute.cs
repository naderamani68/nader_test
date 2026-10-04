using System.ComponentModel.DataAnnotations;

namespace nader_test.Models
{
    /// <summary>
    /// اعتبارسنجی کد ملی ایران بر اساس رقم کنترلی (الگوریتم رسمی).
    /// </summary>
    public class IranianNationalCodeAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var code = value as string;

            if (string.IsNullOrWhiteSpace(code))
            {
                // خالی بودن را ولیدیشن‌های Required بررسی می‌کنند.
                return true;
            }

            code = code.Trim();

            // باید دقیقاً ۱۰ رقم باشد.
            if (code.Length != 10 || !code.All(char.IsDigit))
            {
                return false;
            }

            // کدهای تکراری و ساختگی مانند 0000000000 یا 1111111111 نامعتبرند.
            if (code.Distinct().Count() == 1)
            {
                return false;
            }

            var checkDigit = code[9] - '0';
            var sum = 0;

            for (var i = 0; i < 9; i++)
            {
                sum += (code[i] - '0') * (10 - i);
            }

            var remainder = sum % 11;

            return remainder < 2
                ? checkDigit == remainder
                : checkDigit == 11 - remainder;
        }

        public override string FormatErrorMessage(string name)
        {
            return ErrorMessage ?? $"کد ملی وارد شده برای {name} معتبر نیست.";
        }
    }
}
