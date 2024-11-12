using System;
using System.IO;
using System.Text;

namespace starsky.foundation.thumbnailmeta.ServicesPreviewSize.Helpers;

public class PreviewImageExtractor
{
	public (int Width, int Height)? GetImageSize(string imagePath)
	{
		using ( var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read) )
		using ( var reader = new BinaryReader(stream) )
		{
			// Ensure the file starts with JPEG SOI marker (0xFFD8)
			var soiMarker = ReadBigEndianUInt16(reader);
			if ( soiMarker != 0xFFD8 )
			{
				Console.WriteLine("Not a valid JPEG file.");
				return null;
			}

			// Loop through segments to find SOF marker (0xFFC0 or 0xFFC2)
			while ( reader.BaseStream.Position < reader.BaseStream.Length )
			{
				var marker = ReadBigEndianUInt16(reader);
				var segmentLength = ReadBigEndianUInt16(reader);

				// Check for SOF0 (0xFFC0) or SOF2 (0xFFC2)
				if ( marker == 0xFFC0 || marker == 0xFFC2 )
				{
					reader.ReadByte(); // Skip precision byte

					int height = ReadBigEndianUInt16(reader); // Read height (2 bytes)
					int width = ReadBigEndianUInt16(reader); // Read width (2 bytes)

					return ( width, height );
				}

				// Skip to the next segment if not SOF
				reader.BaseStream.Seek(segmentLength - 2, SeekOrigin.Current);
			}
		}

		Console.WriteLine("SOF marker not found.");
		return null;
	}

	private static ushort ReadBigEndianUInt16(BinaryReader reader)
	{
		var bytes = reader.ReadBytes(2);
		Array.Reverse(bytes); // Convert little-endian to big-endian
		return BitConverter.ToUInt16(bytes, 0);
	}

	public byte[] ExtractTagData(string imagePath, ushort tagToFind)
	{
		using ( var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read) )
		using ( var reader = new BinaryReader(stream) )
		{
			// Check for JPEG SOI marker (0xFFD8)
			if ( ReadBigEndianUInt16(reader) != 0xFFD8 )
			{
				Console.WriteLine("Not a valid JPEG file.");
				return null;
			}

			// Loop through segments to find Exif APP1 (0xFFE1) marker
			while ( reader.BaseStream.Position < reader.BaseStream.Length )
			{
				var marker = ReadBigEndianUInt16(reader);
				var segmentLength = ReadBigEndianUInt16(reader);

				// Look for Exif APP1 marker (0xFFE1)
				if ( marker == 0xFFE1 )
				{
					var exifHeader = reader.ReadBytes(6);
					var exifIdentifier = Encoding.ASCII.GetString(exifHeader, 0, 6);

					if ( exifIdentifier == "Exif\0\0" )
					{
						// We are now in the Exif data; search for the tag
						// return FindTagData(reader, segmentLength - 8, tagToFind);
						ReadExifTags(reader, segmentLength - 8);
					}
				}
				else
				{
					// Skip to the next segment if not APP1
					reader.BaseStream.Seek(segmentLength - 2, SeekOrigin.Current);
				}
			}
		}

		Console.WriteLine($"Tag 0x{tagToFind:X4} not found.");
		return null;
	}
	
	private void ReadExifTags(BinaryReader reader, int length)
	{
		long endPosition = reader.BaseStream.Position + length;

		while (reader.BaseStream.Position < endPosition)
		{
			ushort tag = ReadBigEndianUInt16(reader);
			ushort format = ReadBigEndianUInt16(reader);
			int componentCount = ReadBigEndianUInt32(reader);
			int dataOffset = ReadBigEndianUInt32(reader);

			// Display tag details
			Console.WriteLine($"Tag: 0x{tag:X4}, Format: {format}, Components: {componentCount}");

			// Read tag data based on format and component count
			long currentPosition = reader.BaseStream.Position;
			reader.BaseStream.Seek(currentPosition + dataOffset - 8, SeekOrigin.Begin);
			byte[] data = reader.ReadBytes(componentCount * 2);

			// Display tag data
			DisplayTagData(data, format);
			reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin); // Return to original position

			// Each entry is 12 bytes
			reader.BaseStream.Seek(8, SeekOrigin.Current);
		}
	}

	private void DisplayTagData(byte[] data, ushort format)
	{
		if (format == 2) // ASCII string
		{
			Console.WriteLine($"Data (Text): {Encoding.ASCII.GetString(data)}");
		}
		else if (format == 3) // Short
		{
			ushort value = BitConverter.ToUInt16(data, 0);
			Console.WriteLine($"Data (Short): {value}");
		}
		else if (format == 4) // Long
		{
			uint value = BitConverter.ToUInt32(data, 0);
			Console.WriteLine($"Data (Long): {value}");
		}
		else
		{
		 //	Console.WriteLine("Data (Hex): " + BitConverter.ToString(data));
		}
	}

	private byte[] FindTagData(BinaryReader reader, int length, ushort tagToFind)
	{
		// Read through the Exif data segment
		var endPosition = reader.BaseStream.Position + length;

		while ( reader.BaseStream.Position < endPosition )
		{
			var tag = ReadBigEndianUInt16(reader);
			var format = ReadBigEndianUInt16(reader); // Data format, not used here
			var componentCount = ReadBigEndianUInt32(reader); // Number of components
			var dataOffset = ReadBigEndianUInt32(reader); // Offset to data

			if ( tag == tagToFind )
			{
				// Go to the data offset if it’s within the segment
				var currentPosition = reader.BaseStream.Position;
				reader.BaseStream.Seek(currentPosition + dataOffset - 8, SeekOrigin.Begin);

				// Read the data based on the component count and format
				var t = reader.ReadBytes(componentCount *
				                         2); // Adjust as needed based on tag data format;
				return t;

			}

			// Skip to the next entry (each entry is 12 bytes)
			reader.BaseStream.Seek(8, SeekOrigin.Current);
		}

		return null;
	}

	private int ReadBigEndianUInt32(BinaryReader reader)
	{
		var bytes = reader.ReadBytes(4);
		Array.Reverse(bytes); // Convert to big-endian
		return BitConverter.ToInt32(bytes, 0);
	}
}
