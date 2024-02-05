using System;

namespace starsky.foundation.webtelemetry.Models;

// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0


using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <summary>
/// It is recommended to use a custom type to hold references for
/// ActivitySource and Instruments. This avoids possible type collisions
/// with other components in the DI container.
/// </summary>
public class Instrumentation : IDisposable
{
	internal const string ActivitySourceName = "Examples.AspNetCore";
	internal const string MeterName = "Examples.AspNetCore";
	private readonly Meter _meter;

	public Instrumentation()
	{
		var version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
		ActivitySource = new ActivitySource(ActivitySourceName, version);
		_meter = new Meter(MeterName, version);
		FreezingDaysCounter = this._meter.CreateCounter<long>("weather.days.freezing", description: "The number of days where the temperature is below freezing");
	}

	public ActivitySource ActivitySource { get; }

	public Counter<long> FreezingDaysCounter { get; }

	public void Dispose()
	{
		this.ActivitySource.Dispose();
		this._meter.Dispose();
	}
}
