using Microsoft.Win32;
using System.Runtime.Versioning;
using RuntimeChecker.Utility;

namespace RuntimeChecker;

[SupportedOSPlatform("Windows")]
internal static partial class DotnetFramework
{
    const string runtimeNameStr = ".NET Framework";

    // インストールされている .NET Framework 4.5以降を表示する関数
    public static RuntimeInfo? CheckAfter4d5(bool is64Bit = true)
    {
        const string subKey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

        using var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryHelper.To64Bit(is64Bit)).OpenSubKey(subKey);
        var _releaseValue = ndpKey.GetInt("Release");

        if (_releaseValue is int releaseValue)
        {
            return new(runtimeNameStr, RuntimeInfo.To64Bit(is64Bit), CheckMinVersion(releaseValue), releaseValue.ToString(),
                null, VersionDetail(releaseValue));
        }
        else
        {
            return null;
        }

        static string CheckMinVersion(int releaseKey) => releaseKey switch
        {
            >= 533320 => "4.8.1 or later",
            >= 528040 => "4.8",
            >= 461808 => "4.7.2",
            >= 461308 => "4.7.1",
            >= 460798 => "4.7",
            >= 394802 => "4.6.2",
            >= 394254 => "4.6.1",
            >= 393295 => "4.6",
            >= 379893 => "4.5.2",
            >= 378675 => "4.5.1",
            >= 378389 => "4.5",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseKey), ".NET Framework4.5+ Release value is out of range."),
        };

        static string? VersionDetail(int releaseKey)
        {
            const string standardMsg = "standard";

            return releaseKey switch
            {
                378389 => standardMsg, // 4.5
                378675 => "Windows 8.1 & Windows Server 2012 R2", // 4.5.1
                378758 => standardMsg,
                379893 => standardMsg, // 4.5.2
                393295 => "Windows 10", // 4.6
                393297 => standardMsg,
                394254 => "Windows 10 November Update", // 4.6.1
                394271 => standardMsg,
                394802 => "Windows 10 Anniversary Update & Windows Server 2016", // 4.6.2
                394806 => standardMsg,
                460798 => "Windows 10 Creators Update", // 4.7
                460805 => standardMsg,
                461308 => "Windows 10 Fall Creators Update & Windows Server Ver 1709", // 4.7.1
                461310 => standardMsg,
                461808 => "Windows 10 April 2018 Update & Windows Server Ver 1803", // 4.7.2
                461814 => standardMsg,
                528040 => "Windows 10 May 2019 Update, November 2019 Update", // 4.8
                528372 => "Windows 10 May 2020 Update, October 2020 Update, May 2021 Update, November 2021 Update, 2022 Update",
                528449 => "Windows 11 & Windows Server 2022",
                528049 => standardMsg,
                533320 => "Windows 11 2022 Update & Windows 11 2023 Update", // 4.8.1
                533325 => standardMsg,
                _ => null,
            };
        }
    }

    // インストールされている .NET Framework 1.1~4.0 を列挙する関数（1.0は非対応）
    public static List<RuntimeInfo> Check1To4(bool is64Bit = true)
    {
        const string subKey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\";
        static bool Installed(int? val) => val == 1;

        List<RuntimeInfo> list = [];

        using var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryHelper.To64Bit(is64Bit)).OpenSubKey(subKey);

        foreach (var versionKeyName in ndpKey?.GetSubKeyNames() ?? [])
        {
            // .NET Framework 4.5 以降はスキップ
            if (versionKeyName == "v4") continue;

            if (versionKeyName.StartsWith('v'))
            {
                var versionKey = ndpKey?.OpenSubKey(versionKeyName);
                if (versionKey is null) continue;

                var version = versionKey.GetString("Version");
                var servicePackNumber = versionKey.GetInt("SP");
                var install = versionKey.GetInt("Install");

                if (Installed(install))
                {
                    list.Add(new(runtimeNameStr, RuntimeInfo.To64Bit(is64Bit), versionKeyName, version,
                        servicePackNumber is null ? null : $"SP{servicePackNumber}"));
                }

                // 4.0はサービスパックなし。キーも3.x以前と異なる。
                // バージョンキーのサブキー下を検索
                if (string.IsNullOrEmpty(version))
                {
                    foreach (var subKeyName in versionKey?.GetSubKeyNames() ?? [])
                    {
                        var _subKey = versionKey?.OpenSubKey(subKeyName);
                        if (_subKey is null) continue;

                        var _version = _subKey.GetString("Version");
                        var _install = _subKey.GetInt("Install");

                        if (Installed(_install))
                        {
                            list.Add(new(runtimeNameStr, RuntimeInfo.To64Bit(is64Bit), versionKeyName, _version, subKeyName));
                        }
                    }
                }
            }
        }

        return list;
    }
}
