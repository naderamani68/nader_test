namespace nader_test.Models
{
    /// <summary>
    /// مدل صفحه‌ی فهرست افراد همراه با اطلاعات جستجو، مرتب‌سازی و صفحه‌بندی.
    /// </summary>
    public class PersonListViewModel
    {
        public List<Person> People { get; set; } = new();

        public string? Search { get; set; }

        public string? Sort { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }

        public bool HasPrevious => Page > 1;

        public bool HasNext => Page < TotalPages;
    }
}
