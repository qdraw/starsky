using System.Collections.Generic;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;

namespace starsky.foundation.platform.Models;

public class AppSettingsPublishProfilesDefaults
{
	/// <summary>
	///     Enable optimization and publishing features by default for new publish profiles.
	///     This setting can be overridden in individual publish profiles.
	/// </summary>
	public ProfileFeatures ProfileFeatures { get; set; } = new();

	/// <summary>
	///     List of optimizers to apply by default for new publish profiles.
	/// </summary>
	public List<Optimizer> Optimizers { get; set; } = [];

	public PublishTargets PublishTargets { get; set; } = new();
}

public class ProfileFeatures
{
	public Optimization Optimization { get; set; } = new();
	public Publishing Publishing { get; set; } = new();
}

public class Optimization
{
	public bool Enabled { get; set; }
}

public class Publishing
{
	public bool Enabled { get; set; }
}

public class Optimizer
{
	[JsonConverter(typeof(EnumListConverter<ExtensionRolesHelper.ImageFormat>))]
	public List<ExtensionRolesHelper.ImageFormat> ImageFormats { get; set; } = [];

	public string Id { get; set; } = string.Empty;
	public bool Enabled { get; set; }
	public OptimizerOptions Options { get; set; } = new();
}

public class OptimizerOptions
{
	public int Quality { get; set; } = 80;
}

public class PublishTargets
{
	public FtpTarget Ftp { get; set; } = new();
}

public class FtpTarget
{
	public bool Enabled { get; set; }
}
