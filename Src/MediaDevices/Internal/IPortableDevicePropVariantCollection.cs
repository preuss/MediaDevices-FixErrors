using System;
using System.Runtime.InteropServices;

namespace MediaDevices.Internal
{
    [Guid("89B2E422-4F1B-4316-BCEF-A44AFEA83EB3")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPortableDevicePropVariantCollection
    {
        [PreserveSig]
        int GetCount(
            [In] ref uint pcElems);

        [PreserveSig]
        int GetAt(
            [In] uint dwIndex, 
            [In] ref PropVariant pValue);

        [PreserveSig]
        int Add(
            [In] ref PropVariant pValue);

        [PreserveSig]
        int GetType(
            [Out] out ushort pvt);

        [PreserveSig]
        int ChangeType(
            [In] ushort vt);
         
        [PreserveSig]
        int Clear();

        [PreserveSig]
        int RemoveAt(
            [In] uint dwIndex);
    }
}
