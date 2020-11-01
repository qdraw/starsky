using System.Text.Json.Serialization;

namespace starsky.feature.checkForUpdates.Models
{
	public class ReleaseModel
	{
		/// <summary>
		/// prerelease
		/// </summary>
		///
		[JsonPropertyName("prerelease")]
		public bool PreRelease { get; set; }
		
		/// <summary>
		/// draft
		/// </summary>
		[JsonPropertyName("draft")]
		public bool Draft { get; set; }

		/// <summary>
		/// tag_name
		/// </summary>
		[JsonPropertyName("tag_name")]
		public string TagName { get; set; }
	}
}
