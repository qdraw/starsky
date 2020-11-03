using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.VersionHelpers
{
	/// <summary>
	/// Sem V2
	/// Credits for: https://github.com/maxhauser/semver/blob/master/Semver/SemVersion.cs
	/// </summary>
	public class SemVersion
	{
		/// <summary>
		/// Constructs a new instance of the <see cref="SemVersion" /> class.
		/// </summary>
		/// <param name="major">The major version.</param>
		/// <param name="minor">The minor version.</param>
		/// <param name="patch">The patch version.</param>
		/// <param name="prerelease">The prerelease version (e.g. "alpha").</param>
		/// <param name="build">The build metadata (e.g. "nightly.232").</param>
		public SemVersion(int major, int minor = 0, int patch = 0, string prerelease = "",
			string build = "")
		{
			Major = major;
			Minor = minor;
			Patch = patch;

			Prerelease = prerelease ?? "";
			Build = build ?? "";
		}

		private static readonly Regex ParseEx = new Regex(@"^(?<major>\d+)" +
		                                                  @"(?>\.(?<minor>\d+))?" +
		                                                  @"(?>\.(?<patch>\d+))?" +
		                                                  @"(?>\-(?<pre>[0-9A-Za-z\-\.]+))?" +
		                                                  @"(?>\+(?<build>[0-9A-Za-z\-\.]+))?$",
			RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture,
			TimeSpan.FromSeconds(0.5));

		/// <summary>
		/// Converts the string representation of a semantic version to its <see cref="SemVersion"/> equivalent.
		/// </summary>
		/// <param name="version">The version string.</param>
		/// <returns>The <see cref="SemVersion"/> object.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">The <paramref name="version"/> has an invalid format.</exception>
		/// <exception cref="InvalidOperationException">The <paramref name="version"/> is missing Minor or Patch versions and <paramref name="strict"/> is <see langword="true"/>.</exception>
		/// <exception cref="OverflowException">The Major, Minor, or Patch versions are larger than <code>int.MaxValue</code>.</exception>
		public static SemVersion Parse(string version)
		{
			var match = ParseEx.Match(version);
			if ( !match.Success )
				throw new ArgumentException($"Invalid version '{version}'.", nameof(version));

			var major = int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture);

			var minorMatch = match.Groups["minor"];
			var minor = 0;
			if ( minorMatch.Success )
				minor = int.Parse(minorMatch.Value, CultureInfo.InvariantCulture);

			var patchMatch = match.Groups["patch"];
			var patch = 0;
			if ( patchMatch.Success )
				patch = int.Parse(patchMatch.Value, CultureInfo.InvariantCulture);

			var prerelease = match.Groups["pre"].Value;
			var build = match.Groups["build"].Value;

			return new SemVersion(major, minor, patch, prerelease, build);
		}

		/// <summary>
		/// Gets the major version.
		/// </summary>
		/// <value>
		/// The major version.
		/// </value>
		public int Major { get; set; }

		/// <summary>
		/// Gets the minor version.
		/// </summary>
		/// <value>
		/// The minor version.
		/// </value>
		public int Minor { get; set;}

		/// <summary>
		/// Gets the patch version.
		/// </summary>
		/// <value>
		/// The patch version.
		/// </value>
		public int Patch { get; set;}

		/// <summary>
		/// Gets the prerelease version.
		/// </summary>
		/// <value>
		/// The prerelease version. Empty string if this is a release version.
		/// </value>
		public string Prerelease { get; set;}

		/// <summary>
		/// Gets the build metadata.
		/// </summary>
		/// <value>
		/// The build metadata. Empty string if there is no build metadata.
		/// </value>
		public string Build { get; set;}

		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates
		/// whether the current instance precedes, follows, or occurs in the same position in the sort order as the
		/// other object.
		/// </summary>
		/// <param name="other">An object to compare with this instance.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared.
		/// The return value has these meanings:
		///  Less than zero: This instance precedes <paramref name="other" /> in the sort order.
		///  Zero: This instance occurs in the same position in the sort order as <paramref name="other" />.
		///  Greater than zero: This instance follows <paramref name="other" /> in the sort order.
		/// </returns>
		public int CompareTo(SemVersion other)
		{
			var r = CompareByPrecedence(other);
			if ( r != 0 ) return r;

#pragma warning disable CA1062 // Validate arguments of public methods
			// If other is null, CompareByPrecedence() returns 1
			return CompareComponent(Build, other.Build);
#pragma warning restore CA1062 // Validate arguments of public methods
		}

		/// <summary>
		/// Compares two semantic versions by precedence as defined in the SemVer spec. Versions
		/// that differ only by build metadata have the same precedence.
		/// </summary>
		/// <param name="other">The semantic version.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared.
		/// The return value has these meanings:
		///  Less than zero: This instance precedes <paramref name="other" /> in the sort order.
		///  Zero: This instance occurs in the same position in the sort order as <paramref name="other" />.
		///  Greater than zero: This instance follows <paramref name="other" /> in the sort order.
		/// </returns>
		private int CompareByPrecedence(SemVersion other)
		{
			if ( other is null )
				return 1;

			var r = Major.CompareTo(other.Major);
			if ( r != 0 ) return r;

			r = Minor.CompareTo(other.Minor);
			if ( r != 0 ) return r;

			r = Patch.CompareTo(other.Patch);
			if ( r != 0 ) return r;

			return CompareComponent(Prerelease, other.Prerelease, true);
		}

		private static int CompareComponent(string a, string b, bool nonemptyIsLower = false)
		{
			var aEmpty = string.IsNullOrEmpty(a);
			var bEmpty = string.IsNullOrEmpty(b);
			if ( aEmpty && bEmpty )
				return 0;

			if ( aEmpty )
				return nonemptyIsLower ? 1 : -1;
			if ( bEmpty )
				return nonemptyIsLower ? -1 : 1;

			var aComps = a.Split('.');
			var bComps = b.Split('.');

			var compareLoop = CompareComponentCompareLoop(aComps, bComps);
			// ReSharper disable once ConvertIfStatementToReturnStatement
			if ( compareLoop != int.MaxValue ) return compareLoop;

			return aComps.Length.CompareTo(bComps.Length);
		}

		private static int CompareComponentCompareLoop(string[] aComps, string[] bComps)
		{
			var minLen = Math.Min(aComps.Length, bComps.Length);
			for ( int i = 0; i < minLen; i++ )
			{
				var ac = aComps[i];
				var bc = bComps[i];
				var aIsNum = int.TryParse(ac, out var aNum);
				var bIsNum = int.TryParse(bc, out var bNum);
				if ( aIsNum && bIsNum )
				{
					var r = aNum.CompareTo(bNum);
					if ( r != 0 ) return r;
				}
				else
				{
					var value = CompareComponentCompareOther(aIsNum, bIsNum, ac, bc);
					if ( value != int.MaxValue )
					{
						return value;
					}	
				}
			}
			return int.MaxValue;
		}

		private static int CompareComponentCompareOther(bool aIsNum, bool bIsNum, string ac, string bc)
		{
			if ( aIsNum )
				return -1;
			if ( bIsNum )
				return 1;
			var r = string.CompareOrdinal(ac, bc);
			// ReSharper disable once ConvertIfStatementToReturnStatement
			if ( r != 0 )
				return r;

			return int.MaxValue;
		}

		/// <summary>
		/// Compares the specified versions.
		/// </summary>
		/// <param name="versionA">The first version to compare.</param>
		/// <param name="versionB">The second version to compare.</param>
		/// <returns>A signed number indicating the relative values of <paramref name="versionA"/> and <paramref name="versionB"/>.</returns>
		public static int Compare(SemVersion versionA, SemVersion versionB)
		{
			if ( ReferenceEquals(versionA, versionB) ) return 0;
			if ( versionA is null ) return -1;
			if ( versionB is null ) return 1;
			return versionA.CompareTo(versionB);
		}

		/// <summary>
		/// Returns the <see cref="string" /> equivalent of this version.
		/// </summary>
		/// <returns>
		/// The <see cref="string" /> equivalent of this version.
		/// </returns>
		public override string ToString()
		{
			var version = new System.Text.StringBuilder();
			version.Append(Major);
			version.Append('.');
			version.Append(Minor);
			version.Append('.');
			version.Append(Patch);
			if ( Prerelease.Length > 0 )
			{
				version.Append('-');
				version.Append(Prerelease);
			}

			if ( Build.Length > 0 )
			{
				version.Append('+');
				version.Append(Build);
			}

			return version.ToString();
		}

		/// <summary>
		/// Compares two semantic versions.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>If left is less than or equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
		public static bool operator <=(SemVersion left, SemVersion right)
		{
			return Compare(left, right) <= 0;
		}

		/// <summary>
		/// Compares two semantic versions.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>If left is greater than or equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
		public static bool operator >=(SemVersion left, SemVersion right)
		{
			return Compare(left, right) >= 0;
		}
		
		/// <summary>
		/// Compares two semantic versions.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>If left is greater than right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
		public static bool operator >(SemVersion left, SemVersion right)
		{
			return Compare(left, right) > 0;
		}

		/// <summary>
		/// Compares two semantic versions.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>If left is less than right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
		public static bool operator <(SemVersion left, SemVersion right)
		{
			return Compare(left, right) < 0;
		}

	}
}
