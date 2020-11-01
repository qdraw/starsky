using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.VersionHelpers
{
	/// <summary>
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
		public SemVersion(int major, int minor = 0, int patch = 0, string prerelease = "", string build = "")
		{
			Major = major;
			Minor = minor;
			Patch = patch;

			Prerelease = prerelease ?? "";
			Build = build ?? "";
		}
		
		private static readonly Regex ParseEx =  new Regex(@"^(?<major>\d+)" +
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
        /// <param name="strict">If set to <see langword="true"/> minor and patch version are required,
        /// otherwise they are optional.</param>
        /// <returns>The <see cref="SemVersion"/> object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="version"/> has an invalid format.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="version"/> is missing Minor or Patch versions and <paramref name="strict"/> is <see langword="true"/>.</exception>
        /// <exception cref="OverflowException">The Major, Minor, or Patch versions are larger than <code>int.MaxValue</code>.</exception>
        public static SemVersion Parse(string version, bool strict = false)
        {
            var match = ParseEx.Match(version);
            if (!match.Success)
                throw new ArgumentException($"Invalid version '{version}'.", nameof(version));

            var major = int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture);

            var minorMatch = match.Groups["minor"];
            int minor = 0;
            if (minorMatch.Success)
                minor = int.Parse(minorMatch.Value, CultureInfo.InvariantCulture);
            else if (strict)
                throw new InvalidOperationException("Invalid version (no minor version given in strict mode)");

            var patchMatch = match.Groups["patch"];
            int patch = 0;
            if (patchMatch.Success)
                patch = int.Parse(patchMatch.Value, CultureInfo.InvariantCulture);
            else if (strict)
                throw new InvalidOperationException("Invalid version (no patch version given in strict mode)");

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
		public int Major { get; }

		/// <summary>
		/// Gets the minor version.
		/// </summary>
		/// <value>
		/// The minor version.
		/// </value>
		public int Minor { get; }

		/// <summary>
		/// Gets the patch version.
		/// </summary>
		/// <value>
		/// The patch version.
		/// </value>
		public int Patch { get; }

		/// <summary>
		/// Gets the prerelease version.
		/// </summary>
		/// <value>
		/// The prerelease version. Empty string if this is a release version.
		/// </value>
		public string Prerelease { get; }

		/// <summary>
		/// Gets the build metadata.
		/// </summary>
		/// <value>
		/// The build metadata. Empty string if there is no build metadata.
		/// </value>
		public string Build { get; }
		
		        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        /// other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// The return value has these meanings:
        ///  Less than zero: This instance precedes <paramref name="obj" /> in the sort order.
        ///  Zero: This instance occurs in the same position in the sort order as <paramref name="obj" />.
        ///  Greater than zero: This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        /// <exception cref="InvalidCastException">The <paramref name="obj"/> is not a <see cref="SemVersion"/>.</exception>
        public int CompareTo(object obj)
        {
            return CompareTo((SemVersion)obj);
        }

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
            if (r != 0) return r;

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
        public int CompareByPrecedence(SemVersion other)
        {
            if (other is null)
                return 1;

            var r = Major.CompareTo(other.Major);
            if (r != 0) return r;

            r = Minor.CompareTo(other.Minor);
            if (r != 0) return r;

            r = Patch.CompareTo(other.Patch);
            if (r != 0) return r;

            return CompareComponent(Prerelease, other.Prerelease, true);
        }

        private static int CompareComponent(string a, string b, bool nonemptyIsLower = false)
        {
            var aEmpty = string.IsNullOrEmpty(a);
            var bEmpty = string.IsNullOrEmpty(b);
            if (aEmpty && bEmpty)
                return 0;

            if (aEmpty)
                return nonemptyIsLower ? 1 : -1;
            if (bEmpty)
                return nonemptyIsLower ? -1 : 1;

            var aComps = a.Split('.');
            var bComps = b.Split('.');

            var minLen = Math.Min(aComps.Length, bComps.Length);
            for (int i = 0; i < minLen; i++)
            {
                var ac = aComps[i];
                var bc = bComps[i];
                var aIsNum = int.TryParse(ac, out var aNum);
                var bIsNum = int.TryParse(bc, out var bNum);
                int r;
                if (aIsNum && bIsNum)
                {
                    r = aNum.CompareTo(bNum);
                    if (r != 0) return r;
                }
                else
                {
                    if (aIsNum)
                        return -1;
                    if (bIsNum)
                        return 1;
                    r = string.CompareOrdinal(ac, bc);
                    if (r != 0)
                        return r;
                }
            }

            return aComps.Length.CompareTo(bComps.Length);
        }
        
		/// <summary>
		/// Compares the specified versions.
		/// </summary>
		/// <param name="versionA">The first version to compare.</param>
		/// <param name="versionB">The second version to compare.</param>
		/// <returns>A signed number indicating the relative values of <paramref name="versionA"/> and <paramref name="versionB"/>.</returns>
		public static int Compare(SemVersion versionA, SemVersion versionB)
		{
			if (ReferenceEquals(versionA, versionB)) return 0;
			if (versionA is null) return -1;
			if (versionB is null) return 1;
			return versionA.CompareTo(versionB);
		}

		
		/// <summary>
		/// Compares two semantic versions.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>If left is less than or equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
		public static bool operator <=(SemVersion left, SemVersion right)
		{
			return Equals(left, right) || Compare(left, right) < 0;
		}
		
		/// <summary>
		/// Compares two semantic versions.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>If left is greater than or equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
		public static bool operator >=(SemVersion left, SemVersion right)
		{
			return Equals(left, right) || Compare(left, right) > 0;
		}
	}
}
