using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskytest.Models
{
    public class FakeExifTool : IExifTool
    {
		public Task<bool> WriteTagsAsync(string subPath, string command)
		{
			return Task.FromResult(true);
	    }

	    public Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
	    {
		    return Task.FromResult(true);
	    }
    }
}