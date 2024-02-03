using System.Runtime.Versioning;

namespace RuntimeChecker;

[SupportedOSPlatform("Windows")]
internal static partial class Dotnet
{
    const string runtimeNameStr = ".NET";

    public static (List<RuntimeInfo> sdk, List<RuntimeInfo> framework) Check()
    {
        var envInfo = GetDotnetEnvironmentInfo();
        if (envInfo is null) return ([], []);

        var sdkInfo = envInfo.Sdks.Select(sdk => new RuntimeInfo($"{runtimeNameStr} SDK", RuntimeInfo.Is64Bit(), null, sdk.Version)).ToList();

        var frameworkInfo = envInfo.Frameworks.Select(framework => new RuntimeInfo($"{runtimeNameStr} {ConvertFrameworkName(framework.Name)}", 
            RuntimeInfo.Is64Bit(), null, framework.Version, Note: framework.Name)).ToList();

        return (sdkInfo, frameworkInfo);
    }

    static string ConvertFrameworkName(string frameworkName) => frameworkName switch
    {
        "Microsoft.NETCore.App" => "Runtime",
        "Microsoft.WindowsDesktop.App" => "Desktop",
        "Microsoft.AspNetCore.App" or
        "Microsoft.AspNetCore.All" => "AspNetCore",
        _ => "<Unknown Framework>",
    };
}
