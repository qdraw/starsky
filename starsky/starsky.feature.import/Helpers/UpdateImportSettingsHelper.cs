using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starsky.feature.import.Helpers;

public class UpdateImportSettingsHelper(AppSettings appSettings)
{
	internal ColorClassParser.Color ColorClassTransformation(int colorClassTransformation,
		FileIndexItem? fileIndexItem, string origin)
	{
		return ColorClassTransformation(colorClassTransformation,
			fileIndexItem?.ImageFormat ?? ExtensionRolesHelper.ImageFormat.notfound,
			origin);
	}

	private ColorClassParser.Color ColorClassTransformation(int colorClassTransformation,
		ExtensionRolesHelper.ImageFormat imageFormat, string origin)
	{
		if ( colorClassTransformation >= 0 )
		{
			return ( ColorClassParser.Color ) colorClassTransformation;
		}

		var setting =
			GetTransformationSetting(appSettings.ImportTransformation, imageFormat, origin);
		return setting.ColorClass ?? ColorClassParser.Color.DoNotChange;
	}

	internal static TransformationRule GetTransformationSetting(
		AppSettingsImportTransformationModel config,
		ExtensionRolesHelper.ImageFormat imageFormat,
		string origin)
	{
		foreach ( var rule in config.Rules )
		{
			if ( rule.Conditions.ImageFormats.Contains(imageFormat) ||
			     ( !string.IsNullOrEmpty(rule.Conditions.Origin)
			       && rule.Conditions.Origin == origin ) )
			{
				return rule;
			}
		}

		return new TransformationRule();
	}
}
