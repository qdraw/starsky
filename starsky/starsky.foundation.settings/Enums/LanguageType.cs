using System.Runtime.Serialization;

namespace starsky.foundation.settings.Enums;

public enum LanguageType
{
	[EnumMember(Value = "NL-nl")]
	Dutch = 1,
	[EnumMember(Value = "EN-gb")]
	English = 2
}
