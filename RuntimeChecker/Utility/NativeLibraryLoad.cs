using System.Runtime.InteropServices;

namespace Ruawolf;

internal class NativeLibraryLoad(string libraryPath) : IDisposable
{
    public IntPtr ModuleHandle { get; } = NativeLibrary.Load(libraryPath);

    public T GetProcDelegate<T>(string? methodName = null) where T : Delegate =>
        Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(ModuleHandle, methodName ?? typeof(T).Name));


    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                NativeLibrary.Free(ModuleHandle);
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

internal class TryNativeLibraryLoad : IDisposable
{
    public IntPtr? ModuleHandle { get; } = null;

    public TryNativeLibraryLoad(string libraryPath)
    {
        if (NativeLibrary.TryLoad(libraryPath, out IntPtr handle))
        {
            ModuleHandle = handle;
        }
    }

    public T? GetProcDelegate<T>(string? methodName = null) where T : Delegate
    {
        if (ModuleHandle is IntPtr handle)
        {
            if (NativeLibrary.TryGetExport(handle, methodName ?? typeof(T).Name, out IntPtr address))
            {
                return Marshal.GetDelegateForFunctionPointer<T>(address);
            }
        }
        return null;
    }


    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (ModuleHandle is IntPtr handle)
                {
                    NativeLibrary.Free(handle);
                }
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
