using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Data;

namespace starsky.foundation.database.Serializer
{
	public class ExportDatabase
	{
		private readonly ApplicationDbContext _dbContext;

		public ExportDatabase(ApplicationDbContext context)
		{
			_dbContext = context;
		}

		private Dictionary<string, List<string>> GetItemsByModelNameAllReflection()
		{
			var itemByModelName = new Dictionary<string, List<string>>();
			foreach (var property in _dbContext.Model.GetEntityTypes()
				.SelectMany(t => t.GetProperties()))
			{
				if ( itemByModelName.ContainsKey(property.DeclaringType.Name) )
				{
					itemByModelName[property.DeclaringType.Name].Add(property.Name);
				}
				else
				{
					itemByModelName.Add(property.DeclaringType.Name,new List<string>{property.Name});
				}
			}

			return itemByModelName;
		}

		public async Task Export()
		{
			var itemByModelName = GetItemsByModelNameAllReflection();
			foreach ( var item in itemByModelName )
			{
				// var result = _dbContext.Model.(item.Value.FirstOrDefault());
			}
		}

		// private GetData()
		// {
		// 	Type TableType = _dbContext.GetType().Assembly.GetExportedTypes().FirstOrDefault(t => t.Name == table);
		// 	IQueryable<Object> ObjectContext = _dbContext.Set(TableTypeDictionary[table]);
		// 	return ObjectContext;
		// }
	}

}
