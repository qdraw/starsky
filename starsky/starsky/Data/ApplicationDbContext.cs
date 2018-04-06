using Microsoft.EntityFrameworkCore;
using starsky.Models;

namespace starsky.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<FileIndexItem> FileIndex { get; set; }
    }
}
