using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.VersionHelpers;

namespace starskytest.starsky.foundation.platform.VersionHelpers
{
	[TestClass]
	public class VersionHelpersTest
	{

		[TestMethod]
		public void CtorNull()
		{
			var v = new SemVersion(1, 0, 0, null, null);
			Assert.AreEqual("1.0.0", v.ToString());
		}
		
		private class SemVersionBasic
		{
			public SemVersionBasic(int major, int minor = 0, int patch = 0, string prerelease = "", string build = "")
			{
				Major = major;
				Minor = minor;
				Patch = patch;
				Prerelease = prerelease;
				Build = build;
			}

			public int Major { get; }
			public int Minor { get; }
			public int Patch { get; }
			public string Prerelease { get; }
			public string Build { get; }

		}
		
		/// <summary>
        /// These are version numbers given with the link in the spec to a regex for semver versions
        /// @see: https://github.com/maxhauser/semver/blob/master/Semver.Test/SemVersionComparisonTests.cs
        /// </summary>
		private static readonly Dictionary<string, SemVersionBasic> RegexValidExamples =
            new Dictionary<string, SemVersionBasic>()
            {
                {"0.0.4", new SemVersionBasic(0, 0, 4,"","")},
                {"1.2.3", new SemVersionBasic(1, 2, 3, "", "")},
                {"10.20.30", new SemVersionBasic(10, 20, 30, "", "")},
                {"1.1.2-prerelease+meta", new SemVersionBasic(1, 1, 2, "prerelease", "meta")},
                {"1.1.2+meta", new SemVersionBasic(1, 1, 2, "", "meta")},
                {"1.1.2+meta-valid", new SemVersionBasic(1, 1, 2, "", "meta-valid")},
                {"1.0.0-alpha", new SemVersionBasic(1, 0, 0, "alpha", "")},
                {"1.0.0-beta", new SemVersionBasic(1, 0, 0, "beta", "")},
                {"1.0.0-alpha.beta", new SemVersionBasic( 1, 0, 0, "alpha.beta", "")},
                {"1.0.0-alpha.beta.1", new SemVersionBasic( 1, 0, 0, "alpha.beta.1", "")},
                {"1.0.0-alpha.1", new SemVersionBasic( 1, 0, 0, "alpha.1", "")},
                {"1.0.0-alpha0.valid", new SemVersionBasic( 1, 0, 0, "alpha0.valid", "")},
                {"1.0.0-alpha.0valid", new SemVersionBasic( 1, 0, 0, "alpha.0valid", "")},
                {"1.0.0-alpha-a.b-c-somethinglong+build.1-aef.1-its-okay", new SemVersionBasic( 1, 0, 0, 
	                "alpha-a.b-c-somethinglong", "build.1-aef.1-its-okay")},
                {"1.0.0-rc.1+build.1", new SemVersionBasic( 1, 0, 0, "rc.1", "build.1")},
                {"2.0.0-rc.1+build.123", new SemVersionBasic( 2, 0, 0, "rc.1", "build.123")},
                {"1.2.3-beta", new SemVersionBasic( 1, 2, 3, "beta", "")},
                {"10.2.3-DEV-SNAPSHOT", new SemVersionBasic( 10, 2, 3, "DEV-SNAPSHOT", "")},
                {"1.2.3-SNAPSHOT-123", new SemVersionBasic( 1, 2, 3, "SNAPSHOT-123", "")},
                {"1.0.0", new SemVersionBasic( 1, 0, 0, "", "")},
                {"2.0.0", new SemVersionBasic( 2, 0, 0, "", "")},
                {"1.1.7", new SemVersionBasic( 1, 1, 7, "", "")},
                {"2.0.0+build.1848", new SemVersionBasic( 2, 0, 0, "", "build.1848")},
                {"2.0.1-alpha.1227", new SemVersionBasic( 2, 0, 1, "alpha.1227", "")},
                {"1.0.0-alpha+beta", new SemVersionBasic( 1, 0, 0, "alpha", "beta")},
                {"1.2.3----RC-SNAPSHOT.12.9.1--.12+788", new SemVersionBasic( 1, 2, 3, 
	                "---RC-SNAPSHOT.12.9.1--.12", "788")},
                {"1.2.3----R-S.12.9.1--.12+meta", new SemVersionBasic( 1, 2, 3, "---R-S.12.9.1--.12", "meta")},
                {"1.2.3----RC-SNAPSHOT.12.9.1--.12", new SemVersionBasic( 1, 2, 3, "---RC-SNAPSHOT.12.9.1--.12", "")},
                {"1.0.0+0.build.1-rc.10000aaa-kk-0.1", new SemVersionBasic( 1, 0, 0, "", 
	                "0.build.1-rc.10000aaa-kk-0.1")},
                {"1.0.0-0A.is.legal", new SemVersionBasic( 1, 0, 0, "0A.is.legal", "")},
            };
		
		[TestMethod]
		public void Parse_V040()
		{
			var version = SemVersion.Parse("0.4.0").ToString();
			Assert.AreEqual("0.4.0",version);
		}
		
		[TestMethod]
		public void Parse_List()
		{
			foreach ( var (exampleKey, exampleValue) in RegexValidExamples )
			{
				var parsed = SemVersion.Parse(exampleKey);

				Assert.AreEqual(exampleValue.Build,parsed.Build);
				Assert.AreEqual(exampleValue.Major,parsed.Major);
				Assert.AreEqual(exampleValue.Minor,parsed.Minor);
				Assert.AreEqual(exampleValue.Patch,parsed.Patch);
				Assert.AreEqual(exampleValue.Prerelease,parsed.Prerelease);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Test()
		{
			SemVersion.Parse("test"); 
			// expect wrong input
		}

		private static readonly IReadOnlyList<SemVersion> VersionsInOrder = new List<SemVersion>()
        {
            new SemVersion(-2),
            new SemVersion(-1, -1),
            new SemVersion(-1),
            new SemVersion(0, -1),
            new SemVersion(0, 0, -1),
            new SemVersion(0),
            new SemVersion(0, 0, 1, "13"),
            new SemVersion(0, 0, 1, "."),
            new SemVersion(0, 0, 1, ".."),
            new SemVersion(0, 0, 1, ".a"),
            new SemVersion(0, 0, 1, "b"),
            new SemVersion(0, 0, 1, "gamma.12.87"),
            new SemVersion(0, 0, 1, "gamma.12.87.1"),
            new SemVersion(0, 0, 1, "gamma.12.87.99"),
            new SemVersion(0, 0, 1, "gamma.12.87.X"),
            new SemVersion(0, 0, 1, "gamma.12.88"),
            new SemVersion(0, 0, 1, "", "12"),
            new SemVersion(0, 0, 1, "", "."),
            new SemVersion(0, 0, 1, "", ".."),
            new SemVersion(0, 0, 1, "", ".a"),
            new SemVersion(0, 0, 1, "", "bu"),
            new SemVersion(0, 0, 1, "", "build.12"),
            new SemVersion(0, 0, 1, "", "build.12.2"),
            new SemVersion(0, 0, 1, "", "build.13"),
            new SemVersion(0, 0, 1, "", "uiui"),
            new SemVersion(0, 1, 1),
            new SemVersion(0, 2, 1),
            new SemVersion(1, 0, 0, "alpha"),
            new SemVersion(1, 0, 0, "alpha", "dev.123"),
            new SemVersion(1, 0, 0, "alpha", "😞"),
            new SemVersion(1, 0, 0, "alpha.1"),
            new SemVersion(1, 0, 0, "alpha.beta"),
            new SemVersion(1, 0, 0, "beta"),
            new SemVersion(1, 0, 0, "beta", "dev.123"),
            new SemVersion(1, 0, 0, "beta.2"),
            new SemVersion(1, 0, 0, "beta.11"),
            new SemVersion(1, 0, 0, "rc.1"),
            new SemVersion(1, 0, 0, "😞"),
            new SemVersion(1),
            new SemVersion(1, 0, 10, "alpha"),
            new SemVersion(1, 2, 0, "alpha", "dev"),
            new SemVersion(1, 2, 0, "nightly"),
            new SemVersion(1, 2, 0, "nightly", "dev"),
            new SemVersion(1, 2, 0, "nightly2"),
            new SemVersion(1, 2),
            new SemVersion(1, 2, 0, "", "nightly"),
            new SemVersion(1, 2, 1, "0"),
            new SemVersion(1, 2, 1, "99"),
            new SemVersion(1, 2, 1, "-"),
            new SemVersion(1, 2, 1, "0A"),
            new SemVersion(1, 2, 1, "A"),
            new SemVersion(1, 2, 1, "a"),
            new SemVersion(1, 2, 1),
            new SemVersion(1, 4),
            new SemVersion(2),
            new SemVersion(2, 1),
            new SemVersion(2, 1, 1),
        }.AsReadOnly();

		private static readonly IReadOnlyList<(SemVersion, SemVersion)> VersionPairs =
			AllPairs(VersionsInOrder).ToList().AsReadOnly();
		
		
		[TestMethod]
		public void ComparisonOperatorsLesserToGreaterTest()
		{
			foreach (var (v1, v2) in VersionPairs)
			{
				Assert.IsTrue(v1 <= v2, $"{v1} <= {v2}");
				Assert.IsFalse(v1 >= v2, $"{v1} >= {v2}");
			}
		}

		[TestMethod]
		public void ComparisonOperatorsGreaterToLesserTest()
		{
			foreach (var (v1, v2) in VersionPairs)
			{
				Assert.IsFalse(v2 <= v1, $"{v2} <= {v1}");
				Assert.IsTrue(v2 >= v1, $"{v2} >= {v1}");
			}
		}

		[TestMethod]
		public void ComparisonTwoEqVersion()
		{
			// ReSharper disable once EqualExpressionComparison
			var v2 = new SemVersion(1) >= new SemVersion(1);
			Assert.IsTrue(v2);
		}
		
		[TestMethod]
		public void ComparisonTwoEqVersion2()
		{
			// ReSharper disable once EqualExpressionComparison
			var v2 = new SemVersion(1) <= new SemVersion(1);
			Assert.IsTrue(v2);
		}
		
#pragma warning disable 1718
		[TestMethod]
		public void ComparisonTwoEqVersion3()
        {
            var directQ = new SemVersion(1);
            // ReSharper disable once EqualExpressionComparison
        	var v2 = directQ >= directQ;
        	Assert.IsTrue(v2);
            
            // ReSharper disable once EqualExpressionComparison
            var v3 = directQ > directQ;
            Assert.IsFalse(v3);
        }
#pragma warning restore 1718
		
		[TestMethod]
		public void ComparisonNull()
		{
			var directQ = new SemVersion(1);
			// ReSharper disable once EqualExpressionComparison
			var v2 = null >= directQ;
			Assert.IsFalse(v2);
			
			var v3 = null > directQ;
			Assert.IsFalse(v3);
		}
		
		[TestMethod]
		public void ComparisonNull2()
		{
			var directQ = new SemVersion(1);
			// ReSharper disable once EqualExpressionComparison
			var v2 = directQ >= null;
			Assert.IsTrue(v2);
			var v3 = directQ > null;
			Assert.IsTrue(v3);
		}

		[TestMethod]
		public void ComparisonOperatorsNullToValueTest()
		{
			var v1 = default(SemVersion);
			var v2 = new SemVersion(1);

			Assert.IsTrue(v1 <= v2, $"{v1} <= {v2}");
			Assert.IsFalse(v1 >= v2, $"{v1} >= {v2}");
			
			Assert.IsTrue(v1 < v2, $"{v1} < {v2}");
			Assert.IsFalse(v1 > v2, $"{v1} > {v2}");
		}
		
		[TestMethod]
		public void ComparisonBetaWith_Major_Beta()
		{
			var newerRelease = new SemVersion(0,4);
			var beta = new SemVersion(0,4,0,"-beta.1");

			Assert.IsTrue(beta <= newerRelease, $"{beta} <= {newerRelease}");
			Assert.IsFalse(beta >= newerRelease, $"{beta} >= {newerRelease}");

			Assert.IsTrue(beta < newerRelease, $"{beta} < {newerRelease}");
			Assert.IsFalse(beta > newerRelease, $"{beta} > {newerRelease}");
		}
		
		[TestMethod]
		public void ComparisonBetaSameVersion()
		{
			var sameBeta = new SemVersion(0,4,0,"-beta.1");
			var beta = new SemVersion(0,4,0,"-beta.1");

			Assert.IsTrue(beta <= sameBeta, $"{beta} <= {sameBeta}");
			Assert.IsTrue(beta >= sameBeta, $"{beta} >= {sameBeta}");

			Assert.IsFalse(beta < sameBeta, $"{beta} < {sameBeta}");
			Assert.IsFalse(beta > sameBeta, $"{beta} > {sameBeta}");
		}
		
		[TestMethod]
		public void ComparisonOperatorsValueToNullTest()
		{
			var v1 = new SemVersion(1);
			var v2 = default(SemVersion);

			Assert.IsFalse(v1 <= v2, $"{v1} <= {v2}");
			Assert.IsTrue(v1 >= v2, $"{v1} >= {v2}");
			
			Assert.IsFalse(v1 < v2, $"{v1} < {v2}");
			Assert.IsTrue(v1 > v2, $"{v1} < {v2}");
		}

		[TestMethod]
		public void ComparisonOperatorsNullToNullTest()
		{
			var v1 = default(SemVersion);
			var v2 = default(SemVersion);

			Assert.IsTrue(v1 <= v2, $"{v1} <= {v2}");
			Assert.IsTrue(v1 >= v2, $"{v1} >= {v2}");
			
			Assert.IsFalse(v1 < v2, $"{v1} < {v2}");
			Assert.IsFalse(v1 > v2, $"{v1} > {v2}");
		}

		[TestMethod]
		public void Equals_same_version()
		{
			var sameBeta = new SemVersion(0,4,0,"-beta.1");
			var beta = new SemVersion(0,4,0,"-beta.1");
			Assert.IsTrue(SemVersion.Equals(sameBeta, beta));
		}
		
		
		[TestMethod]
		public void UpdateAfterAssign()
		{
			var version = new SemVersion(1)
			{
				Build = string.Empty,
				Major = 1,
				Minor = 1,
				Patch = 1,
				Prerelease = "-beta.1"
			};
			Assert.AreEqual("1.1.1--beta.1",version.ToString());
		}

		[TestMethod]
		public void GetHashCodeTest()
		{
			var version = new SemVersion(1).GetHashCode();
			Assert.IsNotNull(version);
		}
		
		[TestMethod]
		public void EqualsIdenticalTest()
		{
			foreach (var v in VersionsInOrder)
			{
				// Construct an identical version, but different instance
				var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Build);
				Assert.IsTrue(v.Equals(identical), v.ToString());
			}
		}

		[TestMethod]
		public void EqualsSameTest()
		{
			foreach (var version in VersionsInOrder)
				Assert.IsTrue(version.Equals(version), version.ToString());
		}

		[TestMethod]
		public void EqualsDifferentTest()
		{
			foreach (var (v1, v2) in VersionPairs)
				Assert.IsFalse(v1.Equals(v2), $"({v1}).Equals({v2})");
		}


		[TestMethod]
		public void EqualsPrereleaseLeadingZerosTest()
		{
			var s1 = "1.2.3-01";
			var s2 = "1.2.3-1";
			var v1 = SemVersion.Parse(s1);
			var v2 = SemVersion.Parse(s2);

			var r = v1.Equals(v2);

			Assert.IsFalse(r, $"({v1}).Equals({v2})");
		}
		
		[TestMethod]
		public void EqualsPrereleaseLeadingZerosTest2()
		{
			var s1 = "1.2.3-a.01";
			var s2 = "1.2.3-a.1";
			var v1 = SemVersion.Parse(s1);
			var v2 = SemVersion.Parse(s2);

			var r = v1.Equals(v2);

			Assert.IsFalse(r, $"({v1}).Equals({v2})");
		}
		
		[TestMethod]
		public void EqualsPrereleaseLeadingZerosTest3()
		{
			var s1 = "1.2.3-a.000001";
			var s2 = "1.2.3-a.1";
			var v1 = SemVersion.Parse(s1);
			var v2 = SemVersion.Parse(s2);

			var r = v1.Equals(v2);

			Assert.IsFalse(r, $"({v1}).Equals({v2})");
		}

		[TestMethod]
		public void EqualsNullTest()
		{
			foreach (var version in VersionsInOrder)
				Assert.IsFalse(version.Equals(null), version.ToString());
		}
		
		private static IEnumerable<(SemVersion, SemVersion)> AllPairs(IReadOnlyList<SemVersion> versions)
		{
			for (var i = 0; i < versions.Count; i++)
			for (var j = i + 1; j < versions.Count; j++)
				yield return (versions[i], versions[j]);
		}
	}
}
