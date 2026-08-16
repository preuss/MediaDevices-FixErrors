using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MediaDevices.Internal
{
    [ComImport]
    [Guid("9B4ADD96-F6BF-4034-8708-ECA72BF10554")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPortableDeviceContent2 : IPortableDeviceContent
    {
        [PreserveSig]
        int UpdateObjectWithPropertiesAndData(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszObjectID,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceValues pProperties,
            [Out, MarshalAs(UnmanagedType.Interface)] out IStream ppData,
            [In, Out] ref uint pdwOptimalWriteBufferSize);
    }
}
