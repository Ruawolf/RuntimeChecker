using Microsoft.Win32;
using System.Runtime.Versioning;

namespace RuntimeChecker.Utility;

[SupportedOSPlatform("Windows")]
internal class SystemInfo
{
    public static (string edition, string displayVersion, string buildVersion) WindowsInfo()
    {
        string subKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\";
        using var regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, 
            RegistryHelper.To64Bit(Environment.Is64BitOperatingSystem)).OpenSubKey(subKey);

        var edition = regKey.GetString("ProductName") ?? string.Empty;
        var displayVersion = regKey.GetString("DisplayVersion") ?? string.Empty;
        var buildVersionUpper = regKey.GetString("CurrentBuildNumber") ?? string.Empty;
        var buildVersionLower = regKey.GetInt("UBR")?.ToString() ?? string.Empty;

        return (edition, displayVersion, $"{buildVersionUpper}.{buildVersionLower}");
    }
}
