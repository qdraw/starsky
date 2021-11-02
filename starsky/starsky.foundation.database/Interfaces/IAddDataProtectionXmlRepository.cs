using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace starsky.foundation.database.Interfaces
{
	public interface IAddDataProtectionXmlRepository : IXmlRepository
	{
		IReadOnlyCollection<XElement> GetAllElements();
		
		void StoreElement(XElement element, string friendlyName);
	}
}
