using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;
using starsky.Helpers;

namespace starskytests
{
    [TestClass]
    public class MigrationsTest
    {
        public MigrationsTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseSqlite("Data Source=" + new CreateAnImage().BasePath + "temp.db");
            var options = builder.Options;
            _context = new ApplicationDbContext(options);
        }
        
        private readonly ApplicationDbContext _context;

        [TestMethod]
        public void MigrationsTest_contextDatabaseMigrate()
        {
            _context.Database.Migrate();
            File.Delete(new CreateAnImage().BasePath + "temp.db");
        }
    }
}