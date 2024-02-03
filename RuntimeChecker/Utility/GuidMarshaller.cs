using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace RuntimeChecker.Utility;

[CustomMarshaller(typeof(Guid), MarshalMode.Default, typeof(GuidMarshaller))]
internal static class GuidMarshaller
{
    public unsafe struct UnmanagedGuid
    {
        public fixed byte data[16];
    }

    static unsafe GuidMarshaller()
    {
        if (sizeof(Guid) != sizeof(UnmanagedGuid))
        {
            throw new InvalidOperationException();
        }
    }

    [SkipLocalsInit]
    public static UnmanagedGuid ConvertToUnmanaged(in Guid managed)
    {
        Unsafe.SkipInit<UnmanagedGuid>(out var unmanaged);
        var source = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in managed), 1));
        var destination = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref unmanaged, 1));
        source.CopyTo(destination);
        return unmanaged;
    }

    [SkipLocalsInit]
    public static Guid ConvertToManaged(in UnmanagedGuid unmanaged)
    {
        Unsafe.SkipInit<Guid>(out var managed);
        var source = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in unmanaged), 1));
        var destination = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in managed), 1));
        source.CopyTo(destination);
        return managed;
    }
}