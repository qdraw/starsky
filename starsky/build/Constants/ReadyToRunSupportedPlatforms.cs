using System.Collections.Generic;
using System.Collections.Immutable;

namespace Constants;

public static class ReadyToRunSupportedPlatforms
{
	/// <summary>
	///     SDK-platform 	Supported target platforms
	///     Windows X64 	Windows (X86, X64, Arm64), Linux (X64, Arm32, Arm64), macOS (X64, Arm64)
	///     win-x64 - supports: win-x86, win-x64, win-arm64, linux-x64, linux-arm, linux-arm64, osx-x64,
	///     osx-arm64
	///     Windows X86 	Windows (X86), Linux (Arm32)
	///     win-x86 - supports: win-x86, linux-arm
	///     Linux X64 	Linux (X64, Arm32, Arm64), macOS (X64, Arm64)
	///     linux-x64 - supports linux-x64, linux-arm, linux-arm64, osx-x64, osx-arm64
	///     Linux Arm32 	Linux Arm32
	///     linux-arm, supports linux-arm
	///     Linux Arm64 	Linux (X64, Arm32, Arm64), macOS (X64, Arm64)
	///     linux-arm64 supports linux-x64, linux-arm, linux-arm64, osx-x64, osx-arm64
	///     macOS X64 	Linux (X64, Arm32, Arm64), macOS (X64, Arm64)
	///     osx-x64 supports linux-x64, linux-arm, linux-arm64, osx-x64, osx-arm64
	///     macOS Arm64 	Linux (X64, Arm32, Arm64), macOS (X64, Arm64)
	///     osx-arm64 supports linux-x64, linux-arm, linux-arm64, osx-x64, osx-arm64
	///     @see: https://learn.microsoft.com/en-us/dotnet/core/deploying/ready-to-run
	/// </summary>
	public static readonly ImmutableDictionary<string, ImmutableList<string>> SupportedPlatforms =
		new Dictionary<string, ImmutableList<string>>
		{
			{
				PlatformConstants.WinX64, ImmutableList.Create(PlatformConstants.WinX86,
					PlatformConstants.WinX64,
					PlatformConstants.WinArm64, PlatformConstants.LinuxX64,
					PlatformConstants.LinuxArm, PlatformConstants.LinuxArm64,
					PlatformConstants.OsxX64, PlatformConstants.OsxArm64)
			},
			{
				PlatformConstants.WinX86,
				ImmutableList.Create(PlatformConstants.WinX86, PlatformConstants.LinuxArm)
			},
			{ PlatformConstants.WinArm64, [] },
			{
				PlatformConstants.LinuxX64, ImmutableList.Create(PlatformConstants.LinuxX64,
					PlatformConstants.LinuxArm,
					PlatformConstants.LinuxArm64, PlatformConstants.OsxX64,
					PlatformConstants.OsxArm64)
			},
			{ PlatformConstants.LinuxArm, ImmutableList.Create(PlatformConstants.LinuxArm) },
			{
				PlatformConstants.LinuxArm64, ImmutableList.Create(PlatformConstants.LinuxX64,
					PlatformConstants.LinuxArm,
					PlatformConstants.LinuxArm64, PlatformConstants.OsxX64,
					PlatformConstants.OsxArm64)
			},
			{
				PlatformConstants.OsxX64, ImmutableList.Create(PlatformConstants.LinuxX64,
					PlatformConstants.LinuxArm,
					PlatformConstants.LinuxArm64, PlatformConstants.OsxX64,
					PlatformConstants.OsxArm64)
			},
			{
				PlatformConstants.OsxArm64, ImmutableList.Create(PlatformConstants.LinuxX64,
					PlatformConstants.LinuxArm,
					PlatformConstants.LinuxArm64, PlatformConstants.OsxX64,
					PlatformConstants.OsxArm64)
			}
		}.ToImmutableDictionary();
}
