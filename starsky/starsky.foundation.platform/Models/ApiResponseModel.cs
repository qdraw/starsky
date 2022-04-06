#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.platform.Models
{
	[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
	public class ApiResponseModel
	{
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public ApiMessageType Type { get; set; } = ApiMessageType.Unknown;
	}

	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class ApiResponseModel<T> : ApiResponseModel
	{
		public ApiResponseModel(T? data = default, ApiMessageType type = ApiMessageType.Unknown)
		{
			Data = data;
			Type = type;
		}
		public T? Data { get; set; }
	}
}
