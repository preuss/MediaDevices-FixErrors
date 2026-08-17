using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MediaDevices.Internal
{
    [ComImport]
    [Guid("24DBD89D-413E-43E0-BD5B-197F3C56C886")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPortableDeviceServiceCapabilities
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetSupportedMethods([MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppMethods);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetSupportedMethodsByFormat([In] ref Guid Format, [MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppMethods);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetMethodAttributes([In] ref Guid Method, [MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppAttributes);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetMethodParameterAttributes([In] ref Guid Method, [In] ref PropertyKey Parameter, [MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppAttributes);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetSupportedFormats([MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppFormats);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetFormatAttributes([In] ref Guid Format, [MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppAttributes);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetSupportedFormatProperties([In] ref Guid Format, [MarshalAs(UnmanagedType.Interface)] out IPortableDeviceKeyCollection ppKeys);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetFormatPropertyAttributes([In] ref Guid Format, [In] ref PropertyKey Property, [MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppAttributes);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetSupportedEvents([MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppEvents);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetEventAttributes([In] ref Guid Event, [MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppAttributes);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetEventParameterAttributes([In] ref Guid Event, [In] ref PropertyKey Parameter, [MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppAttributes);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetInheritedServices([In] uint dwInheritanceType, [MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppServices);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetFormatRenderingProfiles([In] ref Guid Format, [MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValuesCollection ppRenderingProfiles);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetSupportedCommands([MarshalAs(UnmanagedType.Interface)] out IPortableDeviceKeyCollection ppCommands);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int GetCommandOptions([In] ref PropertyKey Command, [MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppOptions);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        int Cancel();
    }

}
