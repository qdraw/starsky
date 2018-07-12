using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using starsky.Models;

namespace starsky.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<FileIndexItem> FileIndex { get; set; }
        public DbSet<ImportIndexItem> ImportIndex { get; set; }

    }
}
