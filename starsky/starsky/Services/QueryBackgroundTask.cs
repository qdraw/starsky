using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.Data;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    public class QueryBackgroundTask
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IQuery _query;

        public QueryBackgroundTask(IServiceScopeFactory scopeFactory, IQuery query)
        {
            _scopeFactory = scopeFactory;
            _query = query;
        }
        
        public FileIndexItem UpdateItem(FileIndexItem updateStatusContent)
        {
            // Get Depedency injection from scope
            // In a background task it will fail without scope
            // https://www.infoworld.com/article/3085390/application-development/how-to-handle-concurrency-conflicts-in-entity-framework.html
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Attach(updateStatusContent).State = EntityState.Modified;
                    try
                    {
                        dbContext.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        ex.Entries.Single().Reload();
                        dbContext.SaveChanges();
                    }
                    _query.CacheUpdateItem(new List<FileIndexItem> {updateStatusContent});
                }
            return updateStatusContent;
        }
                
    }
}