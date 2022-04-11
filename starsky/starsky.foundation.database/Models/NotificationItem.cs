using System;
using System.ComponentModel.DataAnnotations;

namespace starsky.foundation.database.Models
{
	public class NotificationItem
	{
		[Key]
		public int Id { get; set; }

		public string Content { get; set; }

		public DateTime DateTime { get; set; }
	}
}

