using System;
using System.Collections.Generic;

namespace starskywebftpcli.Model
{
	public class FtpPublishManifestModel
	{
		public string Slug { get; set; }
		
		public IEnumerable<Tuple<string, bool>> Copy { get; set; } = new List<Tuple<string, bool>>();

	}
}
