using Microsoft.EntityFrameworkCore;

namespace nader_test.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Person> People => Set<Person>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.FirstName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(p => p.LastName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(p => p.NationalCode)
                      .IsRequired()
                      .HasMaxLength(10);

                entity.Property(p => p.PhoneNumber)
                      .IsRequired()
                      .HasMaxLength(11);

                // FullName یک پراپرتی محاسباتی است و در دیتابیس ذخیره نمی‌شود.
                entity.Ignore(p => p.FullName);

                // کد ملی نباید تکراری باشد.
                entity.HasIndex(p => p.NationalCode)
                      .IsUnique()
                      .HasDatabaseName("IX_People_NationalCode_Unique");

                // جستجو بر اساس نام خانوادگی پرکاربرد است.
                entity.HasIndex(p => p.LastName);
            });
        }
    }
}
