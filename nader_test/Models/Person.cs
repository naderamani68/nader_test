using System.ComponentModel.DataAnnotations;

namespace nader_test.Models
{
    /// <summary>
    /// یک شخص (فرد) با نام، نام خانوادگی، کد ملی و شماره تماس.
    /// </summary>
    public class Person
    {
        public int Id { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} باید بین {2} تا {1} کاراکتر باشد.")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} باید بین {2} تا {1} کاراکتر باشد.")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "کد ملی")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "{0} باید دقیقاً ۱۰ رقم باشد.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "{0} باید فقط شامل ۱۰ رقم باشد.")]
        [IranianNationalCode(ErrorMessage = "{0} وارد شده معتبر نیست.")]
        public string NationalCode { get; set; } = string.Empty;

        [Display(Name = "شماره تماس")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [RegularExpression(@"^0\d{10}$", ErrorMessage = "{0} باید با ۰ شروع شده و ۱۱ رقم باشد. مثال: 09121234567")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "تاریخ ثبت")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>نام کامل، برای نمایش در جدول‌ها و گزارش‌ها.</summary>
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
