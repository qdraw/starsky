#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.platform.Models
{
	[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
	public class ApiNotificationResponseModel
	{
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public ApiNotificationType Type { get; set; } = ApiNotificationType.Unknown;
	}

	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class ApiNotificationResponseModel<T> : ApiNotificationResponseModel
	{
		public ApiNotificationResponseModel(T? data = default, ApiNotificationType type = ApiNotificationType.Unknown)
		{
			Data = data;
			Type = type;
		}
		public T? Data { get; set; }
	}
}
