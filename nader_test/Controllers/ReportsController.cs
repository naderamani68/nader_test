using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nader_test.Models;

namespace nader_test.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _db;

        public ReportsController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Reports?search=...
        public async Task<IActionResult> Index(string? search)
        {
            var query = _db.People.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.FirstName, $"%{term}%") ||
                    EF.Functions.Like(p.LastName, $"%{term}%") ||
                    EF.Functions.Like(p.NationalCode, $"%{term}%") ||
                    EF.Functions.Like(p.PhoneNumber, $"%{term}%"));
            }

            // داده‌ها را در حافظه می‌خوانیم تا گروه‌بندی روی حرف اول نام خانوادگی
            // بدون وابستگی به توابع مخصوص SQLite انجام شود.
            var people = await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();

            var today = DateTime.Today;
            var last7 = today.AddDays(-6);
            var last14 = today.AddDays(-13);

            var model = new ReportViewModel
            {
                TotalPeople = people.Count,
                AddedLast7Days = people.Count(p => p.CreatedAt.Date >= last7),
                EditedCount = people.Count(p => p.UpdatedAt.HasValue),
                DistinctLastNames = people
                    .Select(p => p.LastName.Trim())
                    .Where(n => n.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                RecentPeople = people
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToList(),
                Search = search
            };

            // گزارش به تفکیک حرف اول نام خانوادگی.
            model.ByLastNameInitial = people
                .Where(p => !string.IsNullOrWhiteSpace(p.LastName))
                .GroupBy(p => p.LastName.Trim()[..1])
                .Select(g => new GroupCountItem { Title = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ThenBy(g => g.Title)
                .ToList();

            // گزارش ثبت‌نام‌های ۱۴ روز گذشته (روزهای بدون ثبت هم نمایش داده می‌شوند).
            var perDay = people
                .Where(p => p.CreatedAt.Date >= last14)
                .GroupBy(p => p.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            for (var day = last14; day <= today; day = day.AddDays(1))
            {
                model.ByDay.Add(new GroupCountItem
                {
                    Title = day.ToString("yyyy/MM/dd"),
                    Count = perDay.TryGetValue(day, out var count) ? count : 0
                });
            }

            return View(model);
        }

        // GET: /Reports/ExportExcel?search=...
        public async Task<IActionResult> ExportExcel(string? search)
        {
            var query = _db.People.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.FirstName, $"%{term}%") ||
                    EF.Functions.Like(p.LastName, $"%{term}%") ||
                    EF.Functions.Like(p.NationalCode, $"%{term}%") ||
                    EF.Functions.Like(p.PhoneNumber, $"%{term}%"));
            }

            var people = await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("افراد");

            // چیدمان راست‌به‌چپ برای متن فارسی.
            sheet.RightToLeft = true;

            // سرصفحه‌ها.
            string[] headers = { "ردیف", "نام", "نام خانوادگی", "کد ملی", "شماره تماس", "تاریخ ثبت" };
            for (var i = 0; i < headers.Length; i++)
            {
                sheet.Cell(1, i + 1).Value = headers[i];
            }

            var headerRange = sheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontName = "Tahoma";
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // ردیف‌های داده.
            var row = 2;
            var index = 1;
            foreach (var person in people)
            {
                sheet.Cell(row, 1).Value = index++;
                sheet.Cell(row, 2).Value = person.FirstName;
                sheet.Cell(row, 3).Value = person.LastName;
                // کد ملی و شماره تماس به صورت متن نوشته می‌شوند تا صفر ابتدایی حفظ شود.
                sheet.Cell(row, 4).Value = person.NationalCode;
                sheet.Cell(row, 5).Value = person.PhoneNumber;
                sheet.Cell(row, 6).Value = person.CreatedAt.ToString("yyyy/MM/dd HH:mm");
                row++;
            }

            var dataRange = sheet.Range(1, 1, Math.Max(row - 1, 1), headers.Length);
            dataRange.Style.Font.FontName = "Tahoma";
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            sheet.Column(4).Style.NumberFormat.Format = "@";
            sheet.Column(5).Style.NumberFormat.Format = "@";
            sheet.Row(1).Height = 22;

            sheet.Columns().AdjustToContents();

            // حداقل و حداکثر عرض ستون‌ها برای خوانایی بهتر.
            foreach (var column in sheet.ColumnsUsed())
            {
                if (column.Width < 10)
                {
                    column.Width = 10;
                }

                if (column.Width > 45)
                {
                    column.Width = 45;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            var fileName = $"people-report-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
