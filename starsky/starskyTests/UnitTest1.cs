using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

           var options = builder.Options;

            var context = new ApplicationDbContext(options);
            _query = new Query(context);
            _syncservice = new SyncService(context, _query);
        }

        private readonly Query _query;
        private readonly SyncService _syncservice;

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
