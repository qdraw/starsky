using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.health.UpdateCheck.Interfaces;
using starsky.feature.health.UpdateCheck.Models;

namespace starsky.Controllers
{
	[AllowAnonymous]
	public sealed class HealthCheckForUpdatesController : Controller
	{
		private readonly ICheckForUpdates _checkForUpdates;
		private readonly ISpecificVersionReleaseInfo _specificVersionReleaseInfo;

		public HealthCheckForUpdatesController(ICheckForUpdates checkForUpdates,
			ISpecificVersionReleaseInfo specificVersionReleaseInfo)
		{
			_checkForUpdates = checkForUpdates;
			_specificVersionReleaseInfo = specificVersionReleaseInfo;
		}

		/// <summary>
		/// Check if Client/App version has a match with the API-version
		/// </summary>
		/// <returns>status if you need to update</returns>
		/// <response code="208">Feature disabled</response>
		/// <response code="400">http request error or version number is not valid</response>
		/// <response code="206">There are no releases found</response>
		/// <response code="202">Need To Update</response>
		/// <response code="200">Current Version Is Latest</response>
		[HttpGet("/api/health/check-for-updates")]
		[AllowAnonymous]
		[ResponseCache(Duration = 7257600, Location = ResponseCacheLocation.Client)]
		[Produces("application/json")]
		public async Task<IActionResult> CheckForUpdates(string currentVersion = "")
		{
			var (key, value) = await _checkForUpdates.IsUpdateNeeded(currentVersion);
			return key switch
			{
				UpdateStatus.Disabled => StatusCode(StatusCodes.Status208AlreadyReported,
					$"feature is disabled"),
				UpdateStatus.HttpError => BadRequest("something went wrong (http)"),
				UpdateStatus.NoReleasesFound => StatusCode(StatusCodes.Status206PartialContent,
					$"There are no releases found"),
				UpdateStatus.NeedToUpdate => StatusCode(StatusCodes.Status202Accepted, value),
				UpdateStatus.CurrentVersionIsLatest => StatusCode(StatusCodes.Status200OK, value),
				UpdateStatus.InputNotValid => BadRequest("something went wrong (version)"),
				_ => throw new NotSupportedException(
					"IsUpdateNeeded didn't pass any valid selection")
			};
		}

		/// <summary>
		/// Get more info to show about the release
		/// </summary>
		/// <returns>status if you need to update</returns>
		/// <response code="200">result</response>
		[HttpGet("/api/health/release-info")]
		[AllowAnonymous]
		[ResponseCache(Duration = 7257600, Location = ResponseCacheLocation.Client)]
		[Produces("application/json")]
		public async Task<IActionResult> SpecificVersionReleaseInfo(string v = "")
		{
			var result =
				await _specificVersionReleaseInfo.SpecificVersionMessage(v);
			return Json(result);
		}
	}
}
