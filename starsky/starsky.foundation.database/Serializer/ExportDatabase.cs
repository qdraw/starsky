using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private Dictionary<string,  List<KeyValuePair<string, string>> > GetItemsByModelNameAllReflection()
		{
			var itemByModelName = new Dictionary<string, List<KeyValuePair<string, string>> > ();
			foreach ( var entityType in _dbContext.Model.GetEntityTypes() )
			{
				var entityString = _dbContext.Model.FindEntityType(entityType.ClrType).GetTableName();
				
				foreach (var property in entityType.GetProperties())
				{
					if ( itemByModelName.ContainsKey( entityString ) )
					{
						itemByModelName[entityString].Add(new KeyValuePair<string, string>(property.Name, property.ClrType.ToString()));
					}
					else
					{
						itemByModelName.Add(entityString, new List<KeyValuePair<string, string>>{new KeyValuePair<string, string>(property.Name, property.ClrType.ToString())});
					}
				}
			}
			return itemByModelName;
		}


		public async Task Export()
		{


			
			var itemByModelName = GetItemsByModelNameAllReflection();

			foreach ( var singleItemByModelName in itemByModelName )
			{
				var entityType = _dbContext.Model.GetEntityTypes()
					.FirstOrDefault(p => _dbContext.Model.FindEntityType(p.ClrType).GetTableName() == singleItemByModelName.Key);

				
				// var propertyInfo = _dbContext.GetType().GetProperty(singleItemByModelName.Key);
				// var entityType = _dbContext.Model.FindEntityType(propertyInfo.PropertyType);
				var schema = entityType.GetProperties();
				var keys = entityType.GetKeys();
				
				Console.WriteLine();
				foreach ( var propertyInfo in entityType.GetProperties() )
				{
					var value = propertyInfo.GetMemberInfo().
				}
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
