using System;
using System.Runtime.InteropServices;

namespace MediaDevices.Internal
{
    [Guid("625E2DF8-6392-4CF0-9AD1-3CFA5F17775C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPortableDevice
    {
        [PreserveSig]
        int Open(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPnPDeviceID,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceValues pClientInfo);

        [PreserveSig]
        int SendCommand(
            [In] uint dwFlags,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceValues pParameters,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppResults);

        [PreserveSig]
        int Content(
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceContent ppContent);

        [PreserveSig]
        int Capabilities(
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceCapabilities ppCapabilities);

        [PreserveSig]
        int Cancel();

        [PreserveSig]
        int Close();

        [PreserveSig]
        int Advise(
            [In] uint dwFlags,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceEventCallback pCallback,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceValues? pParameters,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszCookie);

        [PreserveSig]
        int Unadvise(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszCookie);

        [PreserveSig]
        int GetPnPDeviceID(
            [Out, MarshalAs(UnmanagedType.LPWStr)]out string ppszPnPDeviceID);
    }
}
