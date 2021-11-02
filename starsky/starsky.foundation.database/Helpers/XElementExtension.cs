using System.Xml.Linq;

namespace starsky.foundation.database.Helpers
{
	public static class XElementExtension
	{
		public static string InnerXml(this XElement el) {
			var reader = el.CreateReader();
			reader.MoveToContent();
			return reader.ReadInnerXml();
		}
	}
}
