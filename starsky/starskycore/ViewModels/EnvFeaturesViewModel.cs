namespace starskycore.ViewModels;

public class EnvFeaturesViewModel
{
	
	/// <summary>
	/// Trash is very dependent on the OS
	/// </summary>
	public bool SystemTrashEnabled { get; set; }
	
	/// <summary>
	/// Enable or disable some features on the frontend
	/// </summary>
	public bool UseLocalDesktopUi { get; set; }
}
