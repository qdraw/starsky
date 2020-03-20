namespace starsky.foundation.injection
{
	public enum InjectionLifetime
	{
		/// <summary>
		/// It's opposite to singleton. You'll get as many object as you call Resolve
		/// </summary>
		Transient,
		
		/// <summary>
		/// It's mean "one instance for all". All times when you call Resolve (even implicitly) you got the same object
		/// </summary>
		Singleton,
		
		/// <summary>
		/// Scoped lifetime services are created once per client request (connection). but different across different requests.
		/// </summary>
		Scoped
	}
}
