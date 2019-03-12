
using System.Reflection;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
	public class ReplaceService
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		
		/// <summary>Do a sync of files uning a subpath</summary>
		/// <param name="query">Starsky IQuery interface to do calls on the database</param>
		/// <param name="appSettings">Settings of the application</param>
		public ReplaceService(IQuery query, AppSettings appSettings)
		{
			_query = query;
			_appSettings = appSettings;
		}

		/// <summary>
		/// Search and replace in string based fields
		/// </summary>
		/// <param name="f">subPath</param>
		/// <param name="search"></param>
		/// <param name="replace"></param>
		/// <param name="fieldName"></param>
		public void Replace(string f, string fieldName, string search, string replace)
		{

		}

		public bool CheckIfPropertyExist(string fieldName)
		{
			PropertyInfo[] propertiesA = new FileIndexItem().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			return true;
		}
	}
}
