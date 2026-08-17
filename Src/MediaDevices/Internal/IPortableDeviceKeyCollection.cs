using System;
using System.Runtime.InteropServices;

namespace MediaDevices.Internal
{
    [Guid("DADA2357-E0AD-492E-98DB-DD61C53BA353")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPortableDeviceKeyCollection
    {
        [PreserveSig]
        int GetCount(
            [In] ref uint pcElems);

        [PreserveSig]
        int GetAt(
            [In] uint dwIndex,
            [In] ref PropertyKey pKey);

        [PreserveSig]
        int Add(
            [In] ref PropertyKey key);

        [PreserveSig]
        int Clear();

        [PreserveSig]
        int RemoveAt(
            [In] uint dwIndex);
    }
}
