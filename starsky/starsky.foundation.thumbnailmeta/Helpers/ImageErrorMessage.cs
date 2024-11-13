using System;

namespace starsky.foundation.thumbnailmeta.Helpers;

public static class ImageErrorMessage
{
	public static string Error(Exception exception)
	{
		const string imageCannotBeLoadedErrorMessage = "Image cannot be loaded";

		var message = exception.Message;
		if ( message.StartsWith(imageCannotBeLoadedErrorMessage) )
		{
			message = imageCannotBeLoadedErrorMessage;
		}

		return message;
	}
}
