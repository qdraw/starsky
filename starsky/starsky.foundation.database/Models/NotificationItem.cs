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

		public DateTime DateTime { get; set; }

		[Timestamp]
		[JsonIgnore]
		[Required(AllowEmptyStrings = false)]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public byte[] LastEdited { get; set; } = Array.Empty<byte>();
	}
}

