namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

internal readonly record struct IfdTagPairQuery(
	ushort PrimaryOffsetTag,
	ushort PrimaryLengthTag,
	ushort AltTag,
	bool LittleEndian);
