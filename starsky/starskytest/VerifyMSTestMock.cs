using System.Runtime.CompilerServices;

namespace VerifyMSTest;

public class VerifyBase
{
	
}

public class Verifier
{
	public static VerifyResultMock Verify(object result)
	{
		return new VerifyResultMock();
	}
}

public class VerifyResultMock : INotifyCompletion
{
	public VerifyResultMock DontScrubDateTimes()
	{
		return this;
	}

	public VerifyResultMock UseParameters(object parameters)
	{
		return this;
	}

	// Awaiter pattern implementation
	public VerifyResultMock GetAwaiter() => this;
	public bool IsCompleted => true;
	public void OnCompleted(System.Action continuation) { continuation?.Invoke(); }
	public void GetResult() { }
}
