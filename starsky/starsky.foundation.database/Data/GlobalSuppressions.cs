using System.Diagnostics.CodeAnalysis;

// S1309: Do not suppress issues.
[assembly:
	SuppressMessage("Sonar", "S1309",
		Justification = "Disable warning when disabling warnings for Generated code",
		Scope = "module")]

// S138: Methods should not have too many lines of code
// S1128: Remove unused 'using' directives such as 'using System.Linq;'
// S1192: Define a constant instead of using this literal
// S3254: Remove this default value assigned to parameter 'nullable'.
// S1192: Define a constant instead of using this literal * times

[assembly:
	SuppressMessage("Sonar", "S138", Justification = "Generated code",
		Scope = "namespaceanddescendants", Target = "starsky.foundation.database.Data.Migrations")]
[assembly:
	SuppressMessage("Sonar", "S1128", Justification = "Generated code",
		Scope = "namespaceanddescendants", Target = "starsky.foundation.database.Data.Migrations")]
[assembly:
	SuppressMessage("Sonar", "S1192", Justification = "Generated code",
		Scope = "namespaceanddescendants", Target = "starsky.foundation.database.Data.Migrations")]
[assembly:
	SuppressMessage("Sonar", "S3254", Justification = "Generated code",
		Scope = "namespaceanddescendants", Target = "starsky.foundation.database.Data.Migrations")]
[assembly:
	SuppressMessage("Sonar", "S1192", Justification = "Generated code",
		Scope = "namespaceanddescendants", Target = "starsky.foundation.database.Data.Migrations")]
