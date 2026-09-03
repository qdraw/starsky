using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Starsky.Desktop.Services;

public static class AppcastChecker
{
	[SuppressMessage("Sonar",
		"S5332: Using http protocol is insecure. Use https instead.",
		Justification = "Xml namespace")]
    private static readonly XNamespace Sparkle = "http://www.andymatuschak.org/xml-namespaces/sparkle";

    /// <summary>
    /// Parses a Sparkle appcast XML string and returns the first item whose version is
    /// newer than <paramref name="currentVersion"/>. Returns null if already up to date
    /// or if the XML cannot be parsed.
    /// </summary>
    public static (string Version, string HtmlUrl)? FindNewerRelease(
        string xml, string currentVersion, ILogger? logger = null)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            foreach (var item in doc.Descendants("item"))
            {
                var version = (string?)item.Element(Sparkle + "shortVersionString")
                              ?? (string?)item.Element(Sparkle + "version");

                if (string.IsNullOrEmpty(version))
                {
	                continue;
                }

                if (IsNewerVersion(version, currentVersion))
                {
	                return (version, BuildReleaseUrl(version));
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "[AppcastChecker] Failed to parse appcast response");
        }

        return null;
    }

    public static bool IsNewerVersion(string candidate, string current)
    {
        var c = ParseVersion(candidate);
        var x = ParseVersion(current);

        var numeric = CompareNumericParts(c, x);
        if (numeric != 0)
            return numeric > 0;

        return ComparePreRelease(c.Pre, x.Pre);
    }

    private static int CompareNumericParts(
        (int Maj, int Min, int Pat, string? Pre) c,
        (int Maj, int Min, int Pat, string? Pre) x)
    {
        if (c.Maj != x.Maj) return c.Maj.CompareTo(x.Maj);
        if (c.Min != x.Min) return c.Min.CompareTo(x.Min);
        return c.Pat.CompareTo(x.Pat);
    }

    private static bool ComparePreRelease(string? cPre, string? xPre)
    {
        if (cPre == null) return xPre != null;  // stable > pre-release; or same stable
        if (xPre == null) return false;         // pre-release < stable

        var cSegs = cPre.Split('.');
        var xSegs = xPre.Split('.');
        for (var i = 0; i < Math.Max(cSegs.Length, xSegs.Length); i++)
        {
            var result = CompareSegment(
                cSegs.ElementAtOrDefault(i) ?? "0",
                xSegs.ElementAtOrDefault(i) ?? "0");
            if (result != 0) return result > 0;
        }
        return false;
    }

    private static int CompareSegment(string cs, string xs)
    {
        if (int.TryParse(cs, out var cn) && int.TryParse(xs, out var xn))
            return cn.CompareTo(xn);
        return string.Compare(cs, xs, StringComparison.OrdinalIgnoreCase);
    }

    private static (int Maj, int Min, int Pat, string? Pre) ParseVersion(string v)
    {
        var dashIdx = v.IndexOf('-');
        var numPart = dashIdx >= 0 ? v[..dashIdx] : v;
        var pre = dashIdx >= 0 ? v[(dashIdx + 1)..] : null;
        var parts = numPart.Split('.');
        return (
            int.TryParse(parts.ElementAtOrDefault(0), out var maj) ? maj : 0,
            int.TryParse(parts.ElementAtOrDefault(1), out var min) ? min : 0,
            int.TryParse(parts.ElementAtOrDefault(2), out var pat) ? pat : 0,
            pre
        );
    }

    private static string BuildReleaseUrl(string version) =>
        $"https://github.com/qdraw/starsky/releases/tag/v{version}";
}
