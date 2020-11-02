using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.health.UpdateCheck.Interfaces;
using starsky.feature.health.UpdateCheck.Models;

namespace starsky.Controllers
{
	public class HealthCheckForUpdates : Controller
	{
		private readonly ICheckForUpdates _checkForUpdates;

		public HealthCheckForUpdates(ICheckForUpdates checkForUpdates)
		{
			_checkForUpdates = checkForUpdates;
		}
		
		/// <summary>
		/// Check if Client/App version has a match with the API-version
		/// </summary>
		/// <returns>status if you need to update</returns>
		/// <response code="202">Upgrade is needed</response>
		/// <response code="200">you are at the latest version</response>
		/// <response code="400">disabled the setting OR something went wrong checking</response>
		[HttpGet("/api/health/check-for-updates")]
		public async Task<IActionResult> CheckForUpdates()
		{
			return await _checkForUpdates.IsUpdateNeeded() switch
			{
				UpdateStatus.NeedToUpdate => StatusCode(StatusCodes.Status202Accepted, $"please upgrade"),
				UpdateStatus.CurrentVersionIsLatest => StatusCode(StatusCodes.Status200OK, $"This is the latest version"),
				UpdateStatus.Disabled => StatusCode(StatusCodes.Status208AlreadyReported, $"feature is disabled"),
				UpdateStatus.HttpError => BadRequest("something went wrong (http)"),
				UpdateStatus.NoReleasesFound => StatusCode(StatusCodes.Status200OK, $"There are no releases found")
			};
		}
	}
}
