using System.Diagnostics.CodeAnalysis;

// GlobalSuppressions.cs

// S6931:Change the paths of the actions of this controller to be relative
// and add a controller route with the common prefix.
[assembly:
	SuppressMessage("Sonar", "S6931",
		Justification =
			"Change the paths of the actions of this controller to be relative and " +
			"add a controller route with the common prefix.",
		Scope = "module")]
