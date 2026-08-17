using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaDevices.Internal
{
    [Guid("A1567595-4C2F-4574-A6FA-ECEF917B9A40")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPortableDeviceManager
    {
        [PreserveSig]
        int GetDevices(
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[]? pPnPDeviceIDs,
            [In, Out] ref uint pcPnPDeviceIDs);

        [PreserveSig]
        int RefreshDeviceList();

        [PreserveSig]
        int GetDeviceFriendlyName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPnPDeviceID,
            [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pDeviceFriendlyName,
            [In, Out] ref uint pcchDeviceFriendlyName);

        [PreserveSig]
        int GetDeviceDescription(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPnPDeviceID, 
            [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pDeviceDescription,
            [In, Out] ref uint pcchDeviceDescription);

        [PreserveSig]
        int GetDeviceManufacturer(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPnPDeviceID,
            [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pDeviceManufacturer,
            [In, Out]ref uint pcchDeviceManufacturer);

        [PreserveSig]
        int GetDeviceProperty(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPnPDeviceID,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszDevicePropertyName,
            [In, Out] ref byte pData,
            [In, Out] ref uint pcbData,
            [In, Out] ref uint pdwType);

        [PreserveSig]
        int GetPrivateDevices(
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]string[]? pPnPDeviceIDs,
            [In, Out] ref uint pcPnPDeviceIDs);
    }
}
