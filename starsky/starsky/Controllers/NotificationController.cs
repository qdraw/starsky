using System;
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
		/// </summary>
		/// <returns>list of notification items</returns>
		/// <response code="200">list of all publish profiles in the _appSettings scope</response>
		/// <response code="400">Longer than 1 ago requested, or null</response>
		/// <response code="401">User unauthorized</response>
		[HttpGet("/api/notification/notification")]
		[Produces("application/json")]
		public async Task<IActionResult> GetNotifications(string dateTime)
		{
			DateTime.TryParse(dateTime, out var parsedDateTime);
			if ( (DateTime.UtcNow - parsedDateTime.ToUniversalTime() ).TotalDays >= 1 )
			{
				return BadRequest("wrong input");
			}
			return Json(await _notificationQuery.Get(parsedDateTime.ToUniversalTime()));
		}	
	}
}

