namespace starsky.foundation.platform.Enums;

/// <summary>
/// These values are stored in a database, so don't change them
/// </summary>
public enum ThumbnailSize
{
	/// <summary>
	/// Should not use this one
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// 150px
	/// </summary>
	TinyMeta = 10,

	/// <summary>
	/// 300px
	/// </summary>
	Small = 20,

	/// <summary>
	/// 1000px
	/// </summary>
	Large = 30,

	/// <summary>
	/// 2000px
	/// </summary>
	ExtraLarge = 40,
}
