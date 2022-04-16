using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Interfaces;

namespace starsky.Controllers
{
	[Authorize]
	public class NotificationController : Controller
	{
		private readonly INotificationQuery _notificationQuery;

		public NotificationController(INotificationQuery notificationQuery)
		{
			_notificationQuery = notificationQuery;
		}
		
		/// <summary>
		/// Get recent notifications
		/// Use dateTime 2022-04-16T17:33:10.323974Z to get the latest notifications
		/// or use numbers to get hours from now (e.g. 1 for last hour)
		/// </summary>
		/// <returns>list of notification items</returns>
		/// <response code="200">list of recent items</response>
		/// <response code="400">Longer than 1 dag ago requested, or null</response>
		/// <response code="401">User unauthorized</response>
		[HttpGet("/api/notification/notification")]
		[Produces("application/json")]
		public async Task<IActionResult> GetNotifications(string dateTime)
		{
			var (parsed, parsedDateTime) = ParseDate(dateTime);
			
			if ( !parsed || (DateTime.UtcNow - parsedDateTime ).TotalDays >= 1 )
			{
				return BadRequest("Please enter a valid dateTime");
			}
			return Json(await _notificationQuery.GetNewerThan(parsedDateTime.ToUniversalTime()));
		}

		internal static Tuple<bool, DateTime> ParseDate(string dateTime)
		{
			var isParsed = DateTime.TryParse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDateTime);
			if ( isParsed )
				return new Tuple<bool, DateTime>(true, parsedDateTime.ToUniversalTime());
			
			if ( !int.TryParse(dateTime, out var parsedInt) )
				return new Tuple<bool, DateTime>(false, DateTime.UtcNow);

			parsedDateTime = DateTime.UtcNow.AddHours(parsedInt * -1);
			return new Tuple<bool, DateTime>(true, parsedDateTime.ToUniversalTime());
		}
	}
}

