using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Services
{
	public class AddDataProtectionXmlRepository : IAddDataProtectionXmlRepository
	{
		private readonly ApplicationDbContext _context;

		public AddDataProtectionXmlRepository(ApplicationDbContext context)
		{
			_context = context;
		}
		
		public IReadOnlyCollection<XElement> GetAllElements()
		{
			var dataProtections = _context.DataProtection.ToList();
			return dataProtections.Select(p => p.Key).Select(XElement.Parse).ToList();
		}

		public void StoreElement(XElement element, string friendlyName)
		{
			_context.DataProtection.Add(new DataProtection
			{
				Key = element.InnerXml(),
				FriendlyName = friendlyName,
				Date = DateTime.UtcNow
			});
		}
	}
}
