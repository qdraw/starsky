using System.Diagnostics.CodeAnalysis;

namespace starsky.foundation.platform.Models
{
	public class ApiResponseModel
	{
		public string DebugName { get; set; }
		public string Type { get; set; } = "default";
	}

	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class ApiResponseModel<T> : ApiResponseModel
	{
		public ApiResponseModel(T data = default, string debugName = null)
		{
			Data = data;
			Type = typeof(T).Name;
			DebugName = debugName;
		}
		public T Data { get; set; }

	}
}
