using System.Collections.Generic;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;

namespace starsky.foundation.platform.Models;

public class AppSettingsPublishProfilesDefaults
{
	/// <summary>
	///     Enable optimization and publishing features by default for new publish profiles.
	///     This setting can be overridden in individual Publish profiles.
	/// </summary>
	public ProfileFeatures ProfileFeatures { get; set; } = new();

	/// <summary>
	///     List of optimizers to apply by default for new publish profiles.
	/// </summary>
	public List<Optimizer> Optimizers { get; set; } = [];
}

public class ProfileFeatures
{
	public Optimization Optimization { get; set; } = new();
}

public class Optimization
{
	public bool Enabled { get; set; }
}

public class Optimizer
{
	/// <summary>
	///     Image formats that this optimizer should be applied to by default for new publish profiles.
	/// </summary>
	[JsonConverter(typeof(EnumListConverter<ExtensionRolesHelper.ImageFormat>))]
	public List<ExtensionRolesHelper.ImageFormat> ImageFormats { get; set; } = [];

	/// <summary>
	///     Unique identifier for the optimizer, used to reference it in publish profiles.
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	///     Enable or disable this optimizer by default for new publish profiles.
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	///     Options for the optimizer, such as quality settings for image optimization.
	/// </summary>
	public OptimizerOptions Options { get; set; } = new();
}

public class OptimizerOptions
{
	/// <summary>
	///     Quality level for image optimization (0-100).
	///     Higher values result in better quality but larger file sizes.
	/// </summary>
	public int Quality { get; set; } = 80;
}
