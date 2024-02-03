using Microsoft.Win32;
using RuntimeChecker.Utility;
using System.Runtime.Versioning;

namespace RuntimeChecker;

[SupportedOSPlatform("Windows")]
internal static partial class Dotnet
{
    public static string? GetFxrPath(bool is64Bit, bool useEnvironmentVariable = false)
    {
        var dotnetRoot = useEnvironmentVariable ? FxrEnvironmentVariable(is64Bit) : null;

        if (dotnetRoot is null)
        {
            dotnetRoot ??= FxrRegistry(is64Bit);
            dotnetRoot ??= FxrFolderPath(is64Bit);
        }

        var fxrPath = Path.Combine(dotnetRoot, @"host\fxr");
        if (!Directory.Exists(fxrPath)) return null;

        var fxrVer = Directory.GetDirectories(fxrPath)
            .Select(Path.GetFileName)
            .Select(x => new Version(x!))
            .Max()!
            .ToString();

        fxrPath = Path.Combine(fxrPath, fxrVer, "hostfxr.dll");
        if (!File.Exists(fxrPath)) return null;

        return fxrPath;
    }

    private static readonly string[] archList = ["arm", "arm64", "armv6", "loongarch64", "ppc64le", "riscv64", "s390x", "x64", "x86"];
    private static string GetArchName(bool is64Bit) => archList[is64Bit ? 7 : 8];

    private static string? FxrEnvironmentVariable(bool is64Bit)
    {
        static string? GetEnv(string variable) =>
            Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine);

        const string DotnetRootStr = "DOTNET_ROOT";

        string dotnetRootEnv1 = DotnetRootStr + "_" + GetArchName(is64Bit).ToUpper();
        string dotnetRootEnv2 = is64Bit ? string.Empty : "DOTNET_ROOT(x86)";
        string dotnetRootEnv3 = DotnetRootStr;

        return GetEnv(dotnetRootEnv1) ?? GetEnv(dotnetRootEnv2) ?? GetEnv(dotnetRootEnv3);
    }

    private static string? FxrRegistry(bool is64Bit)
    {
        var subKey = $@"SOFTWARE\dotnet\Setup\InstalledVersions\{GetArchName(is64Bit)}\";
        using var regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subKey);

        return regKey.GetString("InstallLocation");
    }

    private static string FxrFolderPath(bool is64Bit)
    {
        static string GetdotnetDirPath(Environment.SpecialFolder folder) => Path.Combine(Environment.GetFolderPath(folder), "dotnet");

        return GetdotnetDirPath(is64Bit ? Environment.SpecialFolder.ProgramFiles : Environment.SpecialFolder.ProgramFilesX86);
    }
}

