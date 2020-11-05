namespace starsky.feature.health.UpdateCheck.Models
{
	public enum UpdateStatus
	{
		Disabled,
		InputNotValid,
		HttpError,
		NoReleasesFound,
		NeedToUpdate,
		CurrentVersionIsLatest
	}
}
