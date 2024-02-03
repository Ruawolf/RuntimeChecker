using Microsoft.Win32;
using System.Runtime.Versioning;

namespace RuntimeChecker.Utility;

[SupportedOSPlatform("Windows")]
internal static class RegistryHelper
{
    public static RegistryView To64Bit(bool is64Bit) => is64Bit ? RegistryView.Registry64 : RegistryView.Registry32;

    /// <summary>型引数が値型のときはNullable&lt;T&gt;にすることを忘れずに</summary>
    public static T? GetValue<T>(this RegistryKey? registryKey, string? name) => (T?)registryKey?.GetValue(name);

    public static int? GetInt(this RegistryKey? registryKey, string? name) => registryKey.GetValue<int?>(name);

    public static string? GetString(this RegistryKey? registryKey, string? name) => registryKey.GetValue<string?>(name);
}