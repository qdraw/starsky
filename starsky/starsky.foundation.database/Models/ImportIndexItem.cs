using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Models;

/// <summary>
///     Used to display file status (eg. NotFoundNotInIndex, Ok)
/// </summary>
public enum ImportStatus
{
	Default,
	Ok,
	IgnoredAlreadyImported,
	FileError,
	NotFound,
	Ignore,
	ParentDirectoryNotFound,
	ReadOnlyFileSystem
}

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public sealed class ImportIndexItem
{
	/// <summary>
	///     In order to create an instance of 'ImportIndexItem'
	///     EF requires that a parameter-less constructor be declared.
	/// </summary>
	public ImportIndexItem()
	{
	}

	public ImportIndexItem(AppSettings appSettings)
	{
		Structure = appSettings.Structure;
	}

	/// <summary>
	///     Database Number (isn't used anywhere)
	/// </summary>
	[JsonIgnore]
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	/// <summary>
	///     FileHash before importing
	///     When using a -ColorClass=1 overwrite the fileHash changes during the import process
	/// </summary>
	public string? FileHash { get; set; } = string.Empty;

	/// <summary>
	///     The location where the image should be stored.
	///     When the user move an item this field is NOT updated
	/// </summary>
	public string? FilePath { get; set; } = string.Empty;

	/// <summary>
	///     UTC DateTime when the file is imported
	/// </summary>
	public DateTime AddToDatabase { get; set; }

	/// <summary>
	///     DateTime of the photo/or when it is originally is made
	/// </summary>
	public DateTime DateTime { get; set; }

	[NotMapped]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public ImportStatus Status { get; set; }

	[NotMapped] public FileIndexItem? FileIndexItem { get; set; }

	[NotMapped] [JsonIgnore] public string SourceFullFilePath { get; set; } = string.Empty;


	/// <summary>
	///     Defaults to _appSettings.Structure
	///     Feature to overwrite system structure by request
	/// </summary>
	[NotMapped]
	[JsonIgnore]
	public AppSettingsStructureModel Structure { get; set; } = new();

	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public string? MakeModel { get; set; } = string.Empty;

	[MaxLength(200)] public string? Artist { get; set; } = string.Empty;

	/// <summary>
	///     Is the Exif DateTime parsed from the fileName
	/// </summary>
	public bool DateTimeFromFileName { get; set; }

	/// <summary>
	///     ColorClass
	/// </summary>
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public ColorClassParser.Color ColorClass { get; set; }

	/// <summary>
	///     Store imageFormat like jpeg, png, webp
	/// </summary>
	public ExtensionRolesHelper.ImageFormat ImageFormat { get; set; }

	/// <summary>
	///     Size of the file in bytes
	/// </summary>
	public long Size { get; set; }

	/// <summary>
	///     Where the file is imported from
	/// </summary>
	[MaxLength(100)]
	public string Origin { get; set; } = string.Empty;

	public string GetFileHashWithUpdate()
	{
		if ( FileIndexItem == null && FileHash != null )
		{
			return FileHash;
		}

		return FileIndexItem?.FileHash ?? string.Empty;
	}
}
