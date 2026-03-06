using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace starsky.foundation.platform.Models;

public enum TemplateContentType
{
	/// <summary>
	///     Default, should pick one of the other options
	/// </summary>
	None = 0,

	/// <summary>
	///     Generate Html lists
	/// </summary>
	Html = 1,

	/// <summary>
	///     Create a Jpeg Image
	/// </summary>
	Jpeg = 2,

	/// <summary>
	///     To move the source images to a folder, when using the web ui, this means copying
	/// </summary>
	MoveSourceFiles = 3,

	/// <summary>
	///     Content to be copied from WebHtmlPublish/PublishedContent to include
	///     For example JavaScript files
	/// </summary>
	PublishContent = 4,

	/// <summary>
	///     Include manifest file _settings.json in Copy list
	/// </summary>
	PublishManifest = 6,

	/// <summary>
	///     Only the first image, useful for og:image in template
	/// </summary>
	OnlyFirstJpeg = 7,

	/// <summary>
	///     Publish to Ftp or other remote targets enabled
	/// </summary>
	PublishRemote = 8
}

public class TemplateContentTypeAllowedProperties
{
	public bool SourceMaxWidth { get; set; }
	public bool OverlayMaxWidth { get; set; }
	public bool OverlayFullPath { get; set; }
	public bool Path { get; set; }

	public bool Prepend { get; set; }

	public bool Template { get; set; }
	public bool Copy { get; set; }
	public bool Optimizers { get; set; }
	public bool Folder { get; set; }
	public bool Append { get; set; }
	public bool MetaData { get; set; }
}

public class TemplateContentTypeDisplay
{
	public TemplateContentType Id { get; set; }

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public TemplateContentType Type { get; set; }

	public TemplateContentTypeAllowedProperties Properties { get; set; } = new();
}

public static class AppSettingsPublishProfilesTemplateContentType
{
	public static IEnumerable<TemplateContentTypeDisplay> GetAll()
	{
		return Enum.GetValues(typeof(TemplateContentType))
			.Cast<TemplateContentType>()
			.Where(e => e != TemplateContentType.None)
			.Select(Enrich);
	}

	private static TemplateContentTypeDisplay Enrich(TemplateContentType p)
	{
		var display = new TemplateContentTypeDisplay { Id = p, Type = p };
		switch ( p )
		{
			case TemplateContentType.Html:
				display.Properties.SourceMaxWidth = false;
				display.Properties.OverlayMaxWidth = false;
				display.Properties.OverlayFullPath = false;
				display.Properties.Path = true;
				display.Properties.Folder = true;
				display.Properties.Prepend = true;
				display.Properties.Template = true;
				display.Properties.Copy = true;
				display.Properties.Optimizers = false;
				break;
			case TemplateContentType.Jpeg:
				display.Properties.SourceMaxWidth = true;
				display.Properties.OverlayMaxWidth = true;
				display.Properties.OverlayFullPath = true;
				display.Properties.Path = true;
				display.Properties.Folder = true;
				display.Properties.Prepend = false;
				display.Properties.Template = true;
				display.Properties.Copy = true;
				display.Properties.Optimizers = true;
				display.Properties.Append = false;
				display.Properties.MetaData = true;
				break;
			case TemplateContentType.MoveSourceFiles:
			case TemplateContentType.PublishContent:
			case TemplateContentType.PublishManifest:
				display.Properties.Folder = true;
				display.Properties.Copy = true;
				break;
			case TemplateContentType.OnlyFirstJpeg:
				display.Properties.SourceMaxWidth = true;
				display.Properties.Folder = true;
				display.Properties.Append = true;
				display.Properties.Copy = true;
				display.Properties.MetaData = false;
				break;
			case TemplateContentType.PublishRemote:
				break;
			case TemplateContentType.None:
			default:
				throw new ArgumentOutOfRangeException(nameof(p), p, null);
		}
		return display;
	}
}
