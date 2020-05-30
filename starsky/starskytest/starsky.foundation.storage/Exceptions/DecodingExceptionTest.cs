using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Exceptions;

namespace starskytest.starsky.foundation.storage.Exceptions
{
	[TestClass]
	public class DecodingExceptionTest
	{
		[TestMethod]
		public void DecodingException()
		{
			var serializationInfo = new SerializationInfo(null,null );
			var streamingContext = new StreamingContext(StreamingContextStates.All);
			var decodingException = new DecodingException(serializationInfo, streamingContext);
		}
	}
}
