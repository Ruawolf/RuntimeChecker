using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Ruawolf;

namespace RuntimeChecker;

[SupportedOSPlatform("Windows")]
internal static partial class Dotnet
{
    private static DotnetEnvironmentInfo? GetDotnetEnvironmentInfo()
    {
        var fxrPath = GetFxrPath(Environment.Is64BitProcess);
        if (fxrPath is null) return null;

        using var dll = new TryNativeLibraryLoad(fxrPath);
        var Func = dll.GetProcDelegate<NativeMethods.HostfxrGetDotnetEnvironmentInfo>("hostfxr_get_dotnet_environment_info");
        if (Func is null) return null;

        var result_context = new DotnetEnvironmentInfo();
        unsafe
        {
            Func(null, IntPtr.Zero, new NativeMethods.ResultCallback(ResultCallbackFunc), (IntPtr)Unsafe.AsPointer(ref result_context));
        }

        return result_context;
    }

    private static unsafe void ResultCallbackFunc(IntPtr info, IntPtr result_context)
    {
        var pInfo = (NativeMethods.DotnetEnvironmentInfoStruct*)info;
        if ((nuint)sizeof(NativeMethods.DotnetEnvironmentInfoStruct) != pInfo->size) return;


        List<SdkInfo> sdkInfoList = [];
        NativeMethods.SdkInfoStruct* pSdks = pInfo->sdks;

        for (nuint i = 0; i < pInfo->sdk_count; i++, pSdks++)
        {
            if ((nuint)sizeof(NativeMethods.SdkInfoStruct) != pSdks->size) return;
            sdkInfoList.Add(new(new string(pSdks->version), new string(pSdks->path)));
        }

        List<FrameworkInfo> frameworkInfoList = [];
        NativeMethods.FrameworkInfoStruct* pFrameworks = pInfo->frameworks;

        for (nuint i = 0; i < pInfo->framework_count; i++, pFrameworks++)
        {
            if ((nuint)sizeof(NativeMethods.FrameworkInfoStruct) != pFrameworks->size) return;
            frameworkInfoList.Add(new(new string(pFrameworks->name), new string(pFrameworks->version), new string(pFrameworks->path)));
        }

        var result = Unsafe.AsRef<DotnetEnvironmentInfo>(result_context.ToPointer());
        result.HostfxrVersion = new string(pInfo->hostfxr_version);
        result.HostfxrCommitHash = new string(pInfo->hostfxr_commit_hash);
        result.Sdks = sdkInfoList;
        result.Frameworks = frameworkInfoList;
    }

    internal static partial class NativeMethods
    {
        /// <Summary>利用可能なSDKとフレームワークを返します。</Summary>
        /// <remarks>
        /// <para>dotnet_rootディレクトリ (指定した場合) またはグローバルデフォルトからSDKとフレームワークを解決します。<br/>マルチレベル検索が有効で、dotnet_rootがグローバルと異なる場合、SDKとフレームワークは両方の場所から列挙されます。</para>
        /// <para>SDKはバージョンの昇順でソートされ、マルチレベルルックアップはプライベートより前に置かれます。<br/>フレームワークは、名前の昇順、バージョンの昇順でソートされます。</para>
        /// </remarks>
        /// <returns>成功の場合は0、それ以外の場合は失敗</returns>
        /// <param name="dotnet_root">dotnet実行ファイルを含むディレクトリへのパス</param>
        /// <param name="reserved">将来のために予約されています</param>
        /// <param name="result">SDKとフレームワークのリストを返すコールバック呼び出し<br/>構造体とその要素は呼び出しの間有効です</param>
        /// <param name="result_context">resultコールバックに渡される追加コンテキスト</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal delegate int HostfxrGetDotnetEnvironmentInfo(string? dotnet_root, IntPtr reserved, ResultCallback result, IntPtr result_context);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void ResultCallback(IntPtr info, IntPtr result_context);


        [StructLayout(LayoutKind.Sequential)]
        internal unsafe readonly struct DotnetEnvironmentInfoStruct
        {
            public readonly nuint size;

            public readonly char* hostfxr_version;
            public readonly char* hostfxr_commit_hash;

            public readonly nuint sdk_count;
            public readonly SdkInfoStruct* sdks;

            public readonly nuint framework_count;
            public readonly FrameworkInfoStruct* frameworks;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe readonly struct SdkInfoStruct
        {
            public readonly nuint size;
            public readonly char* version;
            public readonly char* path;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe readonly struct FrameworkInfoStruct
        {
            public readonly nuint size;
            public readonly char* name;
            public readonly char* version;
            public readonly char* path;
        }
    }
}
