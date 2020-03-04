namespace starsky.foundation.ioc
{
	public enum IoCLifetime
	{
		/// <summary>
		/// objects are always different.
		/// </summary>
		Transient,
		/// <summary>
		/// objects are the same for every object and every request.
		/// </summary>
		Singleton,
		/// <summary>
		/// objects are the same within a request, but different across different requests.
		/// </summary>
		Scoped
	}
}
