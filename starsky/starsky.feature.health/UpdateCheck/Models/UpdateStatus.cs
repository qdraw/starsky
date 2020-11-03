namespace starsky.feature.health.UpdateCheck.Models
{
	public enum UpdateStatus
	{
		Disabled,
		HttpError,
		NoReleasesFound,
		NeedToUpdate,
		CurrentVersionIsLatest
	}
}
