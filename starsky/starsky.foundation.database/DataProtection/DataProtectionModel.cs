using System;
using System.ComponentModel.DataAnnotations;

namespace starsky.foundation.database.DataProtection
{
	public class DataProtectionModel
	{
		[Key]
		public int Id { get; set; }
		
		[MaxLength(500)]
		public string XmlData { get; set; }

		[MaxLength(100)]
		public string Name { get; set; }

		public DateTime Expire { get; set; }
	}
}
