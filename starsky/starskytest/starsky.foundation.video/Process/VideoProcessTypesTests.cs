using System.Diagnostics.CodeAnalysis;
using starsky.foundation.video.Process;

namespace starskytest.starsky.foundation.video.Process;

public class VideoProcessTypesTests
{
	/// <summary>
	///     Override the enum with reflection
	///     Set invalid enum value to trigger an exception
	///     Set Enum invalid value
	/// </summary>
	/// <returns></returns>
	[SuppressMessage("Style", "IDE0017:Simplify object initialization")]
	[SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
	public static VideoProcessTypes InvalidEnum()
	{
		var myClass = new SetSearchWideDateTimeOverrideObject();
		myClass.Type = VideoProcessTypes.Thumbnail;

		var propertyObject = myClass.GetType().GetProperty("Type");

		// Set an invalid value that should trigger an exception
		propertyObject?.SetValue(myClass, 44, null);

		return myClass.Type;
	}

	private class SetSearchWideDateTimeOverrideObject
	{
		public VideoProcessTypes Type { get; set; }
	}
}
