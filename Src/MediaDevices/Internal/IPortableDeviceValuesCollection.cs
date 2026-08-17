using System;
using System.Runtime.InteropServices;

namespace MediaDevices.Internal
{
    [Guid("6E3F2D79-4E07-48C4-8208-D8C2E5AF4A99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPortableDeviceValuesCollection
    {
        [PreserveSig]
        int GetCount(
            [In] ref uint pcElems);

        [PreserveSig]
        int GetAt(
            [In] uint dwIndex,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppValues);

        [PreserveSig]
        int Add(
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceValues pValues);

        [PreserveSig]
        int Clear();

        [PreserveSig]
        int RemoveAt(
            [In] uint dwIndex);
    }
}
