using System;
using System.Collections.Generic;

namespace starsky.feature.webftppublish.Models
{
	public class FtpPublishManifestModel
	{
		/// <summary>
		/// Short for name, without spaces and non-ascii
		/// </summary>
		public string Slug { get; set; }
		
		/// <summary>
		/// List of files to Copy, string is relative path and bool is True for copy
		/// </summary>
		public Dictionary<string, bool> Copy { get; set; } = new Dictionary<string, bool>();
	}
}
