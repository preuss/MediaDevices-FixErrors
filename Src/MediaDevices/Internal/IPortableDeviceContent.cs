using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MediaDevices.Internal
{
    [ComImport]
    [Guid("6A96ED84-7C73-4480-9938-BF5AF477D426")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPortableDeviceContent
    {
        [PreserveSig]
        int EnumObjects(
            [In] uint dwFlags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszParentObjectID,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceValues? pFilter,
            [Out, MarshalAs(UnmanagedType.Interface)] out IEnumPortableDeviceObjectIDs ppEnum);

        [PreserveSig]
        int Properties(
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceProperties ppProperties);

        [PreserveSig]
        int Transfer(
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDeviceResources ppResources);

        [PreserveSig]
        int CreateObjectWithPropertiesOnly(
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceValues pValues,
            [In, Out, MarshalAs(UnmanagedType.LPWStr)] ref string ppszObjectID);

        [PreserveSig]
        int CreateObjectWithPropertiesAndData(
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDeviceValues pValues,
            [Out, MarshalAs(UnmanagedType.Interface)]out IStream ppData, 
            [In, Out] ref uint pdwOptimalWriteBufferSize, 
            [In, Out, MarshalAs(UnmanagedType.LPWStr)] ref string? ppszCookie);

        [PreserveSig]
        int Delete(
            [In] uint dwOptions,
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDevicePropVariantCollection pObjectIDs,
            [In, Out, MarshalAs(UnmanagedType.Interface)] ref IPortableDevicePropVariantCollection ppResults);

        [PreserveSig]
        int GetObjectIDsFromPersistentUniqueIDs(
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDevicePropVariantCollection pPersistentUniqueIDs,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPortableDevicePropVariantCollection ppObjectIDs);

        [PreserveSig]
        int Cancel();

        [PreserveSig]
        int Move(
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDevicePropVariantCollection pObjectIDs,
            [In, MarshalAs(UnmanagedType.LPWStr)]string pszDestinationFolderObjectID,
            [In, Out, MarshalAs(UnmanagedType.Interface)] ref IPortableDevicePropVariantCollection ppResults);

        [PreserveSig]
        int Copy(
            [In, MarshalAs(UnmanagedType.Interface)] IPortableDevicePropVariantCollection pObjectIDs,
            [In, MarshalAs(UnmanagedType.LPWStr)]string pszDestinationFolderObjectID,
            [In, Out, MarshalAs(UnmanagedType.Interface)] ref IPortableDevicePropVariantCollection ppResults);
    }
}
