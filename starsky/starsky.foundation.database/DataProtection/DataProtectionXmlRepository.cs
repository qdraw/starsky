using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.injection;

namespace starsky.foundation.database.DataProtection
{
	[Service(typeof(IXmlRepository), InjectionLifetime = InjectionLifetime.Singleton)]
	public class DataProtectionXmlRepository: IXmlRepository
	{
		private readonly ApplicationDbContext _dbContext;

		public DataProtectionXmlRepository(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}
		
		public IReadOnlyCollection<XElement> GetAllElements()
		{
			return new ReadOnlyCollection<XElement>(
				_dbContext.DataProtectionKeys
					.Select(k => XElement.Parse(k.XmlData)).ToList()
				);
		}

		public void StoreElement(XElement element, string friendlyName)
		{
			var entity = _dbContext.DataProtectionKeys
				.SingleOrDefault(k => k.Name == friendlyName);
			
			if (null != entity)
			{
				entity.XmlData = element.ToString();
				_dbContext.DataProtectionKeys.Update(entity);
			}
			else
			{
				_dbContext.DataProtectionKeys.Add(new DataProtectionModel
				{
					Name = friendlyName,
					XmlData = element.ToString(),
					Expire = DateTime.UtcNow
				});
			}

			_dbContext.SaveChanges();
		}
	}
}
