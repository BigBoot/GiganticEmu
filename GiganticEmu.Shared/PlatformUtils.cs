namespace GiganticEmu.Shared;

using System.Runtime.InteropServices;

public static class PlatformUtils
{
    public static string ExecutableExtension
    {
        get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
    }

    public static bool IsWindows
    {
        get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    public static bool IsLinux
    {
        get => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    public static bool IsMac
    {
        get => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }
}