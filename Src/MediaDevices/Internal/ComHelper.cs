using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;


namespace MediaDevices.Internal
{
    internal static class ComHelper
    {
	    public static void Release(this IPortableDeviceConnector? obj) {
		    if(obj != null && Marshal.IsComObject(obj)) {
			    Marshal.ReleaseComObject(obj);
		    }
	    }
	    public static void Release(this IStream? obj) {
		    if(obj != null && Marshal.IsComObject(obj)) {
			    Marshal.ReleaseComObject(obj);
		    }
	    }
		public static bool HasKeyValue(this IPortableDeviceValues values, PropertyKey findKey)
        {
            uint num = 0;
            int err = values.GetCount(ref num);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetCount));
            for (uint i = 0; i < num; i++)
            {
                PropertyKey key = new PropertyKey();
                using (PropVariantFacade val = new PropVariantFacade())
                {
                    err = values.GetAt(i, ref key, ref val.Value);
                    MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetAt), i.ToString());
                    if (key == findKey)
                    {
                        
                        return val.VariantType != PropVariantType.VT_ERROR;
                    }
                }
            }
            
            return false;
        }

        public static PropVariantType GetVarType(this IPortableDeviceValues values, PropertyKey key)
        {
            using (PropVariantFacade val = new PropVariantFacade())
            {
                int err = values.GetValue(ref key, out val.Value);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), key.ToString());
                return val.VariantType;
            }
        }

        internal static bool TryGetValue(this IPortableDeviceValues values, PropertyKey key, out PropVariantFacade value)
        {
            if (values.HasKeyValue(key))
            {
                PropVariantFacade val = new PropVariantFacade();
                int err = values.GetValue(ref key, out val.Value);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), key.ToString());
                value = val;
                return true;
            }
#pragma warning disable CS8625 // Api value is only null if return value is false
			value = null;
#pragma warning restore CS8625 // Api value is only null if return value is false
			return false;
        }

        public static bool TryGetDateTimeValue(this IPortableDeviceValues values, PropertyKey key, out DateTime? value)
		{
            if (values.HasKeyValue(key))
            {
                using (PropVariantFacade val = new PropVariantFacade())
                {
                    int err = values.GetValue(ref key, out val.Value);
                    MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), key.ToString());
                    value = val.ToNullableDate();
                }
                return true;
            }
            value = null;
            return false;
        }

        public static bool TryGetStringValue(this IPortableDeviceValues values, PropertyKey key, out string value)
        {
            if (values.HasKeyValue(key))
            {
                int err = values.GetStringValue(ref key, out value);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetStringValue), key.ToString());
                return true;
            }
            value = string.Empty;
            return false;            
        }

        public static bool TryGetGuidValue(this IPortableDeviceValues values, PropertyKey key, out Guid value)
        {
            if (values.HasKeyValue(key))
            {
                int err = values.GetGuidValue(ref key, out value);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetGuidValue), key.ToString());
                return true;
            }
            value = Guid.Empty;
            return false;
        }

        public static bool TryGetBoolValue(this IPortableDeviceValues values, PropertyKey key, out bool value)
        {
            if (values.HasKeyValue(key))
            {
                int val;
                int err = values.GetBoolValue(ref key, out val);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetBoolValue), key.ToString());
                value = val != 0;
                return true;
            }
            value = false;
            return false;
        }

        public static bool TryGetUnsignedIntegerValue(this IPortableDeviceValues values, PropertyKey key, out uint value)
        {
            if (values.HasKeyValue(key))
            {
                int err = values.GetUnsignedIntegerValue(ref key, out value);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetUnsignedIntegerValue), key.ToString());
                return true;
            }
            value = 0;
            return false;
        }

        public static bool TryGetUnsignedLargeIntegerValue(this IPortableDeviceValues values, PropertyKey key, out ulong value)
        {
            if (values.HasKeyValue(key))
            {
                int err = values.GetUnsignedLargeIntegerValue(ref key, out value);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetUnsignedLargeIntegerValue), key.ToString());
                return true;
            }
            value = 0;
            return false;
        }

        public static bool TryGetSignedIntegerValue(this IPortableDeviceValues values, PropertyKey key, out int value)
        {
            if (values.HasKeyValue(key))
            {
                int err = values.GetSignedIntegerValue(ref key, out value);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetSignedIntegerValue), key.ToString());
                return true;
            }
            value = 0;
            return false;
        }

        public static bool TryGetIUnknownValue(this IPortableDeviceValues values, PropertyKey key, out object? value)
        {
            if (values.HasKeyValue(key))
            {
                int err = values.GetIUnknownValue(ref key, out value);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetIUnknownValue), key.ToString());
                return true;
            }
            value = null;
            return false;
        }

        public static bool TryByteArrayValue(this IPortableDeviceValues values, PropertyKey key, out byte[]? value)
        {
            if (values.HasKeyValue(key))
            {
                using (PropVariantFacade val = new PropVariantFacade())
                {
                    int err = values.GetValue(ref key, out val.Value);
                    MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), key.ToString());
                    value = val.ToByteArray();
                }
                return true;
            }
            value = null;
            return false;
        }

		private static class NativeMethods
		{
			// http://www.pinvoke.net/default.aspx/iprop/PropVariantClear.html
			// https://social.msdn.microsoft.com/Forums/windowsserver/en-US/ec242718-8738-4468-ae9d-9734113d2dea/quotipropdllquot-seems-to-be-missing-in-windows-server-2008-and-x64-systems?forum=winserver2008appcompatabilityandcertification
			[DllImport("ole32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
			public static extern int PropVariantClear(ref PropVariant val);
		}
	}
}
