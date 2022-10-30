using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace starsky.foundation.database.Models
{
	public sealed class NotificationItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// Size: MediumText
		/// </summary>
		[ConcurrencyCheck]
		public string? Content { get; set; }

		[Timestamp]
		public DateTime DateTime { get; set; }
		
	}
}

