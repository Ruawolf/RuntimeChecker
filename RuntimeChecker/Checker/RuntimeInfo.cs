using System.ComponentModel;
using System.Text;

namespace RuntimeChecker;

internal record RuntimeInfo(string RuntimeName, RuntimeInfo.ArchitEnum Archit,
    string? Version, string? DetailVersion, string? VersionExtension = null, string? Note = null)
{
    public enum ArchitEnum
    {
        Unknown,
        x86,
        x64,
    }

    public static ArchitEnum To64Bit(bool is64Bit) => is64Bit ? ArchitEnum.x64 : ArchitEnum.x86;
    public static ArchitEnum Is64Bit() => To64Bit(Environment.Is64BitProcess);

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.Append(RuntimeName);

        if (Archit != Is64Bit())
        {
            var architStr = Archit switch
            {
                ArchitEnum.Unknown => string.Empty,
                ArchitEnum.x86 => "(x86)",
                ArchitEnum.x64 => "(x64)",
                _ => throw new InvalidEnumArgumentException(),
            };

            sb.Append(architStr);
        }

        if (Version != null && DetailVersion != null)
        {
            // "xxx v4(4.2.1)"
            sb.Append(' ');
            sb.Append(Version);
            sb.Append('(');
            sb.Append(DetailVersion);
            sb.Append(')');
        }
        else if (Version != null || DetailVersion != null)
        {
            // "xxx v4" or "xxx 4.2.1"
            sb.Append(' ');
            sb.Append(Version ?? DetailVersion);
        }

        if (VersionExtension != null && (Version != null || DetailVersion != null))
        {
            sb.Append(' ');
            sb.Append(VersionExtension);
        }

        if (Note != null)
        {
            sb.Append(" - ");
            sb.Append(Note);
        }

        return sb.ToString();
    }

    public static List<RuntimeInfo> Merge(params object?[] arg)
    {
        List<RuntimeInfo> list = [];

        foreach (object? obj in arg)
        {

            if (obj is RuntimeInfo runtimeInfo)
            {
                list.Add(runtimeInfo);
            }
            else if (obj is List<RuntimeInfo> runtimeInfoList)
            {
                list.AddRange(runtimeInfoList);
            }
        }

        return list;
    }
}
