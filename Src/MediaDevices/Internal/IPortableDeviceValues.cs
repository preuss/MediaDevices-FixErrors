using System;
using System.Runtime.InteropServices;

namespace MediaDevices.Internal
{
    [Guid("6848F6F2-3155-4F86-B6F5-263EEEAB3143")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPortableDeviceValues
    {
        [PreserveSig]
        int GetCount(
             [In] ref uint pcelt);

        [PreserveSig]
        int GetAt(
            [In] uint index,
            [In, Out] ref PropertyKey pKey,
            [In, Out] ref PropVariant pValue);

        [PreserveSig]
        int SetValue(
            [In] ref PropertyKey key,
            [In] ref PropVariant pValue);

        [PreserveSig]
        int GetValue(
            [In] ref PropertyKey key,
            [Out] out PropVariant pValue);

        [PreserveSig]
        int SetStringValue(
            [In] ref PropertyKey key,
            [In, MarshalAs(UnmanagedType.LPWStr)] string value);

        [PreserveSig]
        int GetStringValue(
            [In] ref PropertyKey key, 
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string pValue);

        [PreserveSig]
        int SetUnsignedIntegerValue(
            [In] ref PropertyKey key,
            [In] uint value);

        [PreserveSig]
        int GetUnsignedIntegerValue(
            [In] ref PropertyKey key, 
            [Out] out uint pValue);

        [PreserveSig]
        int SetSignedIntegerValue(
            [In] ref PropertyKey key,
            [In] int value);

        [PreserveSig]
        int GetSignedIntegerValue(
            [In] ref PropertyKey key, 
            [Out] out int pValue);

        [PreserveSig]
        int SetUnsignedLargeIntegerValue(
            [In] ref PropertyKey key,
            [In] ulong value);

        [PreserveSig]
        int GetUnsignedLargeIntegerValue(
            [In] ref PropertyKey key, 
            [Out] out ulong pValue);

        [PreserveSig]
        int SetSignedLargeIntegerValue(
            [In] ref PropertyKey key,
            [In] long value);

        [PreserveSig]
        int GetSignedLargeIntegerValue(
            [In] ref PropertyKey key, 
            [Out] out long pValue);

        [PreserveSig]
        int SetFloatValue(
            [In] ref PropertyKey key,
            [In] float value);

        [PreserveSig]
        int GetFloatValue(
            [In] ref PropertyKey key, 
            [Out] out float pValue);

        [PreserveSig]
        int SetErrorValue(
            [In] ref PropertyKey key,
            [In] int value);

        [PreserveSig]
        int GetErrorValue(
            [In] ref PropertyKey key, 
            [Out] out int pValue);

        [PreserveSig]
        int SetKeyValue(
            [In] ref PropertyKey key,
            [In] ref PropertyKey Value);

        [PreserveSig]
        int GetKeyValue(
            [In] ref PropertyKey key, 
            [Out] out PropertyKey pValue);

        [PreserveSig]
        int SetBoolValue(
            [In] ref PropertyKey key,
            [In] int value);

        [PreserveSig]
        int GetBoolValue(
            [In] ref PropertyKey key, 
            [Out] out int pValue);

        [PreserveSig]
        int SetIUnknownValue(
            [In] ref PropertyKey key,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pValue);

        [PreserveSig]
        int GetIUnknownValue(
            [In] ref PropertyKey key,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppValue);

        [PreserveSig]
        int SetGuidValue(
            [In] ref PropertyKey key,
            [In] ref Guid value);

        [PreserveSig]
        int GetGuidValue(
            [In] ref PropertyKey key, 
            [Out] out Guid pValue);

        [PreserveSig]
        int SetBufferValue(
            [In] ref PropertyKey key,
            [In] ref byte pValue,
            [In] uint cbValue);

        [PreserveSig]
        int GetBufferValue(
            [In] ref PropertyKey key, 
            [Out] IntPtr ppValue, 
            [Out] out uint pcbValue);

        [PreserveSig]
        int SetIPortableDeviceValuesValue(
            [In] ref PropertyKey key,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceValues pValue);

        [PreserveSig]
        int GetIPortableDeviceValuesValue(
            [In] ref PropertyKey key,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValues ppValue);

        [PreserveSig]
        int SetIPortableDevicePropVariantCollectionValue(
            [In] ref PropertyKey key,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDevicePropVariantCollection pValue);

        [PreserveSig]
        int GetIPortableDevicePropVariantCollectionValue(
            [In] ref PropertyKey key,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppValue);

        [PreserveSig]
        int SetIPortableDeviceKeyCollectionValue(
            [In] ref PropertyKey key,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceKeyCollection pValue);

        [PreserveSig]
        int GetIPortableDeviceKeyCollectionValue(
            [In] ref PropertyKey key,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceKeyCollection ppValue);

        [PreserveSig]
        int SetIPortableDeviceValuesCollectionValue(
            [In] ref PropertyKey key,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceValuesCollection pValue);

        [PreserveSig]
        int GetIPortableDeviceValuesCollectionValue(
            [In] ref PropertyKey key,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceValuesCollection ppValue);

        [PreserveSig]
        int RemoveValue(
            [In] ref PropertyKey key);

        [PreserveSig]
        int CopyValuesFromPropertyStore(
            [In, MarshalAs(UnmanagedType.Interface)] IPropertyStore pStore);

        [PreserveSig]
        int CopyValuesToPropertyStore(
            [In, MarshalAs(UnmanagedType.Interface)] IPropertyStore pStore);

        [PreserveSig]
        int Clear();
    }
}