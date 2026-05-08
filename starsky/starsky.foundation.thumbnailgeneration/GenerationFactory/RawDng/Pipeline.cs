using System;
using System.Collections.Generic;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal class Pipeline<T>
{
	private readonly List<Func<T, T>> _steps = [];

	public Pipeline<T> Add(Func<T, T> step)
	{
		_steps.Add(step);
		return this;
	}

	public T Run(T input)
	{
		foreach ( var step in _steps )
		{
			input = step(input);
		}

		return input;
	}
}

