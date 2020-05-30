using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.storage.Exceptions
{
	[Serializable]
	public class DecodingException : Exception
	{
            
		/// <summary>
		/// Without this constructor, deserialization will fail
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected internal DecodingException(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}
	}
}
