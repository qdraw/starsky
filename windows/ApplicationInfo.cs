using System.Reflection;

namespace Starsky.Desktop;

internal static class ApplicationInfo
{
    public static string Version { get; } =
        typeof(ApplicationInfo).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion?.Split('+')[0]
        ?? "0.0.0";
}
