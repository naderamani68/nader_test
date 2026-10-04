namespace nader_test.Models
{
    /// <summary>
    /// مدل صفحه‌ی گزارش: آمار کلی و گزارش‌های گروهی.
    /// </summary>
    public class ReportViewModel
    {
        /// <summary>تعداد کل افراد ثبت‌شده.</summary>
        public int TotalPeople { get; set; }

        /// <summary>تعداد افرادی که در ۷ روز گذشته ثبت شده‌اند.</summary>
        public int AddedLast7Days { get; set; }

        /// <summary>تعداد افرادی که تاکنون ویرایش شده‌اند.</summary>
        public int EditedCount { get; set; }

        /// <summary>تعداد نام خانوادگی‌های متفاوت.</summary>
        public int DistinctLastNames { get; set; }

        /// <summary>گزارش تعداد افراد به تفکیک حرف اول نام خانوادگی.</summary>
        public List<GroupCountItem> ByLastNameInitial { get; set; } = new();

        /// <summary>گزارش تعداد ثبت‌نام به تفکیک روز (۱۴ روز گذشته).</summary>
        public List<GroupCountItem> ByDay { get; set; } = new();

        /// <summary>آخرین افراد ثبت‌شده.</summary>
        public List<Person> RecentPeople { get; set; } = new();

        /// <summary>فیلتر جستجو، در صورت وجود.</summary>
        public string? Search { get; set; }
    }

    /// <summary>یک ردیف گزارش گروهی (عنوان + تعداد).</summary>
    public class GroupCountItem
    {
        public string Title { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
