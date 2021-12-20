[< starsky](../readme.md)

# starsky.foundation.injection


Use for example to get the following class automapped
```cs
[Service(typeof(IConsole), InjectionLifetime = InjectionLifetime.Scoped)]
public class ConsoleWrapper : IConsole
{
```