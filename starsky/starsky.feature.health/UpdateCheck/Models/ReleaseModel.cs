using System;
using System.Text.Json.Serialization;

namespace starsky.feature.health.UpdateCheck.Models
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

		private string _tagName = string.Empty;
		
		/// <summary>
		/// Should start with v
		/// </summary>
		[JsonPropertyName("tag_name")]
		public string TagName {
			get => _tagName;
			set
			{
				if ( string.IsNullOrWhiteSpace(value)) return;
				if ( !value.StartsWith("v") ) Console.WriteLine($"{_tagName} Should start with v");
				_tagName = value;
			} 
		}
	}
}
