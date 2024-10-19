using Microsoft.EntityFrameworkCore;
using DotNetAssignment2.Models;

namespace DotNetAssignment2.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<UploadedFile> UploadedFiles { get; set; }

        public DbSet<TrainingForm> TrainingForms { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=uploads.db");
            }
        }
    }
}
