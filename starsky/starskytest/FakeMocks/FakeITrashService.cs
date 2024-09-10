using System.Collections.Generic;
using starsky.foundation.native.Trash.Interfaces;

namespace starskytest.FakeMocks;

public class FakeITrashService : ITrashService
{
	public FakeITrashService(bool isSupported = true)
	{
		IsSupported = isSupported;
	}

	public bool IsSupported { get; set; }
	public List<string> InTrash { get; set; } = new();

	public bool DetectToUseSystemTrash()
	{
		return IsSupported;
	}

	public bool? Trash(string fullPath)
	{
		if ( !IsSupported )
		{
			return null;
		}

		InTrash.Add(fullPath);
		return true;
	}

	public bool? Trash(List<string> fullPaths)
	{
		if ( !IsSupported )
		{
			return null;
		}

		InTrash.AddRange(fullPaths);
		return true;
	}
}
