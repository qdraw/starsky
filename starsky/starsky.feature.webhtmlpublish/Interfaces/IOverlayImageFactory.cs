namespace starsky.feature.webhtmlpublish.Interfaces
{
	/// <summary>
	///     The one place that decides which image library is used to merge the logo
	/// </summary>
	public interface IOverlayImageFactory
	{
		IOverlayImage Create();
	}
}
