using starsky.foundation.platform.Helpers;

namespace starsky.foundation.platform.Models;

public static class SelectStructureSettingsService
{
	public static string GetStructureSetting(AppSettingsStructureModel config,
		ExtensionRolesHelper.ImageFormat? imageFormat = null)
	{
		foreach ( var rule in config.Rules )
		{
			if ( imageFormat != null && rule.Conditions.ImageFormats.Contains(
				     ( ExtensionRolesHelper.ImageFormat ) imageFormat) &&
			     !string.IsNullOrEmpty(rule.Pattern) )
			{
				return rule.Pattern;
			}
		}

		return config.DefaultPattern;
	}
}
