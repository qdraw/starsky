namespace starsky.project.web.ViewModels;

public class EnvFeaturesViewModel
{
	/// <summary>
	/// Trash is very dependent on the OS
	/// </summary>
	public bool SystemTrashEnabled { get; set; }

	/// <summary>
	/// Enable or disable some features on the frontend
	/// </summary>
	public bool UseLocalDesktop { get; set; }

	/// <summary>
	/// Is supported and enabled in the feature toggle
	/// </summary>
	public bool OpenEditorEnabled { get; set; }
}
