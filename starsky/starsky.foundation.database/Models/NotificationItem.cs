using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models
{
	public class NotificationItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// Size: MediumText
		/// </summary>
		public string Content { get; set; }

		public DateTime DateTime { get; set; }
	}
}

