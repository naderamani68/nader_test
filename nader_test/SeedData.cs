using nader_test.Models;

namespace nader_test
{
    /// <summary>
    /// داده‌های نمونه که فقط زمانی درج می‌شوند که جدول افراد خالی باشد.
    /// </summary>
    public static class SeedData
    {
        public static void Initialize(AppDbContext db)
        {
            if (db.People.Any())
            {
                return;
            }

            db.People.AddRange(
                new Person { FirstName = "علی", LastName = "رضایی", NationalCode = "1234567891", PhoneNumber = "09121234567" },
                new Person { FirstName = "مریم", LastName = "احمدی", NationalCode = "2345678917", PhoneNumber = "09351234568" },
                new Person { FirstName = "حسین", LastName = "محمدی", NationalCode = "3456789173", PhoneNumber = "09129876543" },
                new Person { FirstName = "زهرا", LastName = "کریمی", NationalCode = "4567891724", PhoneNumber = "09901234567" },
                new Person { FirstName = "محمد", LastName = "حسینی", NationalCode = "5678917234", PhoneNumber = "09121112233" }
            );

            db.SaveChanges();
        }
    }
}
