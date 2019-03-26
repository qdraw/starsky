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
		    throw new NotImplementedException();
	    }

	    public Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
	    {
		    throw new NotImplementedException();
	    }
    }
}