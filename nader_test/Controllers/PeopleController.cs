using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nader_test.Models;

namespace nader_test.Controllers
{
    public class PeopleController : Controller
    {
        private readonly AppDbContext _db;

        public PeopleController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /People?search=...&sort=...&page=1
        public async Task<IActionResult> Index(string? search, string? sort, int page = 1)
        {
            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            var query = _db.People.AsNoTracking().AsQueryable();

            // جستجو در نام، نام خانوادگی، کد ملی و شماره تماس.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.FirstName, $"%{term}%") ||
                    EF.Functions.Like(p.LastName, $"%{term}%") ||
                    EF.Functions.Like(p.NationalCode, $"%{term}%") ||
                    EF.Functions.Like(p.PhoneNumber, $"%{term}%"));
            }

            // مرتب‌سازی.
            query = sort switch
            {
                "name_desc" => query.OrderByDescending(p => p.LastName).ThenByDescending(p => p.FirstName),
                "date" => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.LastName),
                "date_desc" => query.OrderBy(p => p.CreatedAt).ThenBy(p => p.LastName),
                _ => query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var model = new PersonListViewModel
            {
                People = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(),
                Search = search,
                Sort = sort,
                Page = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return View(model);
        }

        // GET: /People/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var person = await _db.People.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person is null)
            {
                return NotFound();
            }

            return View(person);
        }

        // GET: /People/Create
        public IActionResult Create()
        {
            return View(new Person());
        }

        // POST: /People/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,NationalCode,PhoneNumber")] Person person)
        {
            if (await NationalCodeExistsAsync(person.NationalCode))
            {
                ModelState.AddModelError(nameof(Person.NationalCode), "این کد ملی قبلاً ثبت شده است.");
            }

            if (!ModelState.IsValid)
            {
                return View(person);
            }

            person.CreatedAt = DateTime.Now;
            _db.People.Add(person);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"«{person.FullName}» با موفقیت ثبت شد.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /People/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var person = await _db.People.FindAsync(id);

            if (person is null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: /People/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,NationalCode,PhoneNumber,CreatedAt")] Person person)
        {
            if (id != person.Id)
            {
                return NotFound();
            }

            if (await NationalCodeExistsAsync(person.NationalCode, person.Id))
            {
                ModelState.AddModelError(nameof(Person.NationalCode), "این کد ملی برای شخص دیگری ثبت شده است.");
            }

            if (!ModelState.IsValid)
            {
                return View(person);
            }

            try
            {
                person.UpdatedAt = DateTime.Now;
                _db.Update(person);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"اطلاعات «{person.FullName}» به‌روزرسانی شد.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PersonExistsAsync(person.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /People/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var person = await _db.People.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person is null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: /People/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _db.People.FindAsync(id);

            if (person is not null)
            {
                _db.People.Remove(person);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"«{person.FullName}» حذف شد.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /People/CheckNationalCode?nationalCode=...&id=0
        // برای اعتبارسنجی آنی سمت کلاینت.
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckNationalCode(string nationalCode, int id = 0)
        {
            if (string.IsNullOrWhiteSpace(nationalCode))
            {
                return Json(true);
            }

            var exists = await NationalCodeExistsAsync(nationalCode, id);

            return Json(!exists
                ? (object)true
                : "این کد ملی قبلاً ثبت شده است.");
        }

        private Task<bool> PersonExistsAsync(int id) =>
            _db.People.AnyAsync(p => p.Id == id);

        private Task<bool> NationalCodeExistsAsync(string? nationalCode, int excludeId = 0)
        {
            if (string.IsNullOrWhiteSpace(nationalCode))
            {
                return Task.FromResult(false);
            }

            var code = nationalCode.Trim();

            return _db.People.AnyAsync(p => p.NationalCode == code && p.Id != excludeId);
        }
    }
}
