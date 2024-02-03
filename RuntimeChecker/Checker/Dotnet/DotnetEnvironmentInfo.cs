namespace RuntimeChecker;

internal class DotnetEnvironmentInfo
{
    public string? HostfxrVersion { get; set; }
    public string? HostfxrCommitHash { get; set; }
    public List<SdkInfo> Sdks { get; set; } = [];
    public List<FrameworkInfo> Frameworks { get; set; } = [];
}

internal record SdkInfo(string Version, string Path);

internal record FrameworkInfo(string Name, string Version, string Path);