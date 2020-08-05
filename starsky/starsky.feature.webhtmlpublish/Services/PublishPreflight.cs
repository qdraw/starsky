using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.Services
{
	public class PublishPreflight
	{
		private readonly AppSettings _appSettings;

		public PublishPreflight(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}
		
		public List<Tuple<int,string>> GetPublishProfileNames()
		{
			var returnList = new List<Tuple<int,string>>();
			if ( !_appSettings.PublishProfiles.Any() )
			{
				return returnList;
			}

			var i = 0;
			foreach ( var profile in _appSettings.PublishProfiles )
			{
				returnList.Add(new Tuple<int, string>(i, profile.Key));
				i++;
			}
			return returnList;
		}

		public string GetPublishProfileNameByIndex(int index)
		{
			return _appSettings.PublishProfiles.ElementAt(index).Key;
		}
	}
}
