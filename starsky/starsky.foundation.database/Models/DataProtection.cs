using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace starsky.foundation.database.Models
{
	public class DataProtection
	{
		[JsonIgnore]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		
		public string Key { get; set; }
		public string FriendlyName { get; set; }
		public DateTime Date { get; set; }
	}
}
