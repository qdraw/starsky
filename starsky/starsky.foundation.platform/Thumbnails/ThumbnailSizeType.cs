namespace starsky.foundation.platform.Thumbnails;

/// <summary>
///     These values are stored in a database, so don't change them
/// </summary>
public enum ThumbnailSize
{
	/// <summary>
	///     Should not use this one
	/// </summary>
	Unknown = 0,

	/// <summary>
	///     Very tiny, 4x4 pixels.
	/// </summary>
	TinyIcon = 5,

	/// <summary>
	///     150px
	/// </summary>
	TinyMeta = 10,

	/// <summary>
	///     300px
	/// </summary>
	Small = 20,

	/// <summary>
	///     1000px
	/// </summary>
	Large = 30,

	/// <summary>
	///     2000px
	/// </summary>
	ExtraLarge = 40
}
