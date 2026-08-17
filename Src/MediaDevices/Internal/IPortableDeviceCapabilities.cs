using System;
using System.Runtime.InteropServices;

namespace MediaDevices.Internal
{
    [Guid("2C8C6DBF-E3DC-4061-BECC-8542E810D126")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPortableDeviceCapabilities
    {
        [PreserveSig]
        int GetSupportedCommands(
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceKeyCollection ppCommands);

        [PreserveSig]
        int GetCommandOptions(
            [In] ref PropertyKey Command,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppOptions);

        [PreserveSig]
        int GetFunctionalCategories(
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppCategories);

        [PreserveSig]
        int GetFunctionalObjects(
            [In] ref Guid Category,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppObjectIDs);

        [PreserveSig]
        int GetSupportedContentTypes(
            [In] ref Guid Category,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppContentTypes);

        [PreserveSig]
        int GetSupportedFormats(
            [In] ref Guid ContentType,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppFormats);

        [PreserveSig]
        int GetSupportedFormatProperties(
            [In] ref Guid Format,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceKeyCollection ppKeys);

        [PreserveSig]
        int GetFixedPropertyAttributes(
            [In] ref Guid Format,
            [In] ref PropertyKey key,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppAttributes);

        [PreserveSig]
        int Cancel();

        [PreserveSig]
        int GetSupportedEvents(
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppEvents);

        [PreserveSig]
        int GetEventOptions(
            [In] ref Guid Event,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppOptions);
    }
}
