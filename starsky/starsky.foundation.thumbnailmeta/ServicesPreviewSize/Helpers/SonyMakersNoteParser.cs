using System;
using System.IO;
using System.Text;

public class SonyMakerNotesParser
{
	public static void Main()
	{
		var imagePath = "your_image.jpg";
		ParseSonyMakerNotes(imagePath);
	}

	public static void ParseSonyMakerNotes(string imagePath)
	{
		using ( var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read) )
		using ( var reader = new BinaryReader(fs) )
		{
			if ( !IsJpeg(reader) )
			{
				Console.WriteLine("Not a valid JPEG file.");
				return;
			}

			// Search for the APP1 (Exif) segment
			while ( reader.BaseStream.Position < reader.BaseStream.Length )
			{
				var marker1 = reader.ReadByte();
				var marker2 = reader.ReadByte();

				// Check for known markers: APP1 (Exif), APP2 (Sony), etc.
				if (marker1 == 0xFF)
				{
					// If it's APP1 or APP2 segment (common for Exif or Sony MakerNotes)
					if (marker2 == 0xE1 || marker2 == 0xE2) 
					{
						ushort segmentLength = ReadBigEndianUInt16(reader);
						byte[] segmentData = reader.ReadBytes(segmentLength - 2);
						Console.WriteLine($"Found Segment 0x{marker2:X2}, Length: {segmentLength}");

						// Check for Exif or other known formats inside the segment
						if (marker2 == 0xE1) // APP1 - Exif
						{
							Console.WriteLine("Found Exif Segment. Skipping.");
						}
						else if (marker2 == 0xE2) // APP2 - Sony MakerNotes
						{
							Console.WriteLine("Found Sony MakerNotes Segment.");
							// Here you can further inspect the data
						}
					}
					else
					{
						// Skip over other segments
						ushort segmentLength = ReadBigEndianUInt16(reader);
						reader.BaseStream.Seek(segmentLength - 2, SeekOrigin.Current);
					}
				}
				else
				{
					// Skip over any non-marked sections
					reader.BaseStream.Seek(1, SeekOrigin.Current);
				}
				
				
				// Check for APP1 (Exif) segment
				if ( marker1 == 0xFF && marker2 == 0xE1 )
				{
					var segmentLength = ReadBigEndianUInt16(reader);
					reader.BaseStream.Seek(segmentLength - 2,
						SeekOrigin.Current); // Skip the rest of the segment

					// Read Exif Header bytes
					var exifHeader = reader.ReadBytes(6); // Read 6 bytes
					Console.WriteLine("Exif Header (raw): " + BitConverter.ToString(exifHeader));

					// Check for "Exif" in the header
					if ( Encoding.ASCII.GetString(exifHeader, 0, 4) == "Exif" )
					{
						Console.WriteLine("Exif Header Found.");
						// Parse TIFF header
						reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip TIFF header
						var tagCount = ReadBigEndianUInt16(reader);

						// Search for Sony's MakerNotes tag
						for ( var i = 0; i < tagCount; i++ )
						{
							var tagId = ReadBigEndianUInt16(reader);
							var tagType = ReadBigEndianUInt16(reader);
							uint tagCountData = ReadBigEndianUInt16(reader);
							uint tagValue = ReadBigEndianUInt16(reader);

							// Check for MakerNotes or PreviewImage (Sony-specific tags)
							if ( tagId == 0x927C ) // Sony MakerNotes tag ID
							{
								Console.WriteLine("Found MakerNotes.");
								reader.BaseStream.Seek(tagValue,
									SeekOrigin.Begin); // Seek to MakerNotes section

								// Now manually process MakerNotes content
								ParseMakerNotes(reader);
								break; // Exit the loop after processing the MakerNotes
							}
						}
					}

					break; // Exit the loop after processing the Exif section
				}
				else
				{
					// Skip over other segments
					var segmentLength = ReadBigEndianUInt16(reader);
					reader.BaseStream.Seek(segmentLength - 2, SeekOrigin.Current);
				}
			}
		}
	}

	private static void ParseMakerNotes(BinaryReader reader)
	{
		// Example parsing: Read and interpret Sony MakerNotes data
		// Sony's MakerNotes may store preview image data in specific offsets, which you would need to find.

		// Read bytes from the MakerNotes section only if there are enough bytes remaining
		var remainingBytes = ( int ) ( reader.BaseStream.Length - reader.BaseStream.Position );
		if ( remainingBytes > 0 )
		{
			var makerNotesData =
				reader.ReadBytes(Math.Min(remainingBytes,
					128)); // Read bytes, ensure not exceeding remaining stream length

			// Inspect these bytes manually to see where the PreviewImage data might be stored
			Console.WriteLine(BitConverter.ToString(makerNotesData, 0,
				Math.Min(makerNotesData.Length, 64)));
		}
		else
		{
			Console.WriteLine("No data available to read from MakerNotes.");
		}
	}

	private static bool IsJpeg(BinaryReader reader)
	{
		reader.BaseStream.Seek(0, SeekOrigin.Begin);
		return reader.ReadByte() == 0xFF && reader.ReadByte() == 0xD8;
	}

	private static ushort ReadBigEndianUInt16(BinaryReader reader)
	{
		if ( reader.BaseStream.Position + 2 <= reader.BaseStream.Length )
		{
			var highByte = reader.ReadByte();
			var lowByte = reader.ReadByte();
			return ( ushort ) ( ( highByte << 8 ) | lowByte );
		}

		throw new EndOfStreamException("Unable to read beyond the end of the stream.");
	}
}
