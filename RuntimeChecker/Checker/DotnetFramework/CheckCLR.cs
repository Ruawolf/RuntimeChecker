using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using RuntimeChecker.Utility;

namespace RuntimeChecker;

[SupportedOSPlatform("Windows")]
internal static partial class DotnetFramework
{
    internal static partial class NativeMethods
    {
        internal static bool IsSafeHResult(int hResult, bool throwException = false)
        {
            const int S_OK = 0;
            const int S_FALSE = 1;

            if (hResult == S_OK)
            {
                return true;
            }
            else
            {
                if (!throwException)
                    return false;

                throw hResult switch
                {
                    S_FALSE => new COMException(),
                    _ => new COMException(),
                };
            }
        }

        [LibraryImport("mscoree.dll")]
        internal static partial int CLRCreateInstance(
            [MarshalUsing(typeof(GuidMarshaller))] in Guid clsid,
            [MarshalUsing(typeof(GuidMarshaller))] in Guid riid,
            out ICLRMetaHost ppInterface
            );

        // IEnumUnknown
        [GeneratedComInterface]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("00000100-0000-0000-C000-000000000046")]
        internal partial interface IEnumICLRRuntimeInfo
        {
            [PreserveSig]
            int Next(
                uint celt,
                [Out, MarshalAs(UnmanagedType.LPArray)] ICLRRuntimeInfo[] rgelt,
                out uint celtFetched
                );
        }

        [GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("D332DB9E-B9B3-4125-8207-A14884F53216")]
        internal partial interface ICLRMetaHost
        {
            [PreserveSig]
            int GetRuntime(
                string pwzVersion,
                [MarshalUsing(typeof(GuidMarshaller))] in Guid riid,
                out ICLRRuntimeInfo ppRuntime
                );

            void GetVersionFromFile();

            [PreserveSig]
            int EnumerateInstalledRuntimes(
                out IEnumICLRRuntimeInfo ppEnumerator
                );

            void EnumerateLoadedRuntimes();
            void RequestRuntimeLoadedNotification();
            void QueryLegacyV2RuntimeBinding();
            void ExitProcess();
        }

        [GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("BD39D1D2-BA2F-486a-89B0-B4B0CB466891")]
        internal partial interface ICLRRuntimeInfo
        {
            [PreserveSig]
            int GetVersionString(
                [MarshalUsing(CountElementName = "pcchBuffer"), Out] char[]? pwzBuffer,
                ref uint pcchBuffer
                );

            void GetRuntimeDirectory();
            void IsLoaded();
            void LoadErrorString();
            void LoadLibrary();
            void GetProcAddress();
            void GetInterface();
            void IsLoadable();
            void SetDefaultStartupFlags();
            void GetDefaultStartupFlags();
            void BindAsLegacyV2Runtime();
            void IsStarted();
        }
    }

    private static List<NativeMethods.ICLRRuntimeInfo> EnumUnknownToList(NativeMethods.IEnumICLRRuntimeInfo enumUnknown)
    {
        List<NativeMethods.ICLRRuntimeInfo> list = [];
        var array = new NativeMethods.ICLRRuntimeInfo[1];
        while (true)
        {
            if (!NativeMethods.IsSafeHResult(enumUnknown.Next(1, array, out var _)))
                break;

            list.AddRange(array);
        }

        return list;
    }

    private static string ICLRRuntimeInfoToVersion(NativeMethods.ICLRRuntimeInfo runtimeInfo)
    {
        uint bufferSize = 0;
        runtimeInfo.GetVersionString(null, ref bufferSize);

        var buffer = ArrayPool<char>.Shared.Rent((int)bufferSize);
        uint pcchBuffer = (uint)buffer.Length;

        if (!NativeMethods.IsSafeHResult(runtimeInfo.GetVersionString(buffer, ref pcchBuffer)))
        {
            ArrayPool<char>.Shared.Return(buffer);
            throw new COMException();
        }

        string result = new(buffer.AsSpan()[..((int)bufferSize - 1)]);
        ArrayPool<char>.Shared.Return(buffer);

        return result;
    }

    public static List<RuntimeInfo> CheckCLR()
    {
        try
        {
            return CheckCLRCore();
        }
        catch (DllNotFoundException)
        {
            return [];
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static List<RuntimeInfo> CheckCLRCore()
    {
        Guid CLSID_CLRMetaHost = Guid.Parse("9280188D-0E8E-4867-B30C-7FA83884E8DE");
        Guid IID_ICLRMetaHost = Guid.Parse("D332DB9E-B9B3-4125-8207-A14884F53216");

        var hr = NativeMethods.CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, out var cLRMetaHost);
        NativeMethods.IsSafeHResult(hr, true);

        hr = cLRMetaHost.EnumerateInstalledRuntimes(out var ppEnumerator);
        NativeMethods.IsSafeHResult(hr, true);

        var cLRRuntimeInfoList = EnumUnknownToList(ppEnumerator);
        var runtimeVersions = cLRRuntimeInfoList.Select(ICLRRuntimeInfoToVersion);

        return runtimeVersions.Select(version => new RuntimeInfo($"{runtimeNameStr} CLR", RuntimeInfo.Is64Bit(), null, version)).ToList();
    }
}
