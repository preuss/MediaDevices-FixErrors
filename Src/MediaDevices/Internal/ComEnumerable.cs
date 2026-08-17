using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MediaDevices.Internal
{
    internal static class ComEnumerable
    {
        public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePair(this IPortableDeviceValues values)
        {
            uint num = 0;
            int err = values.GetCount(ref num);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetCount));
            for (uint i = 0; i < num; i++)
            {
                PropertyKey key = new();
                using (PropVariantFacade val = new())
                {
                    err = values.GetAt(i, ref key, ref val.Value);
                    MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetAt), i.ToString());

                    string fieldName = string.Empty;
                    FieldInfo? propField = ComTrace.FindPropertyKeyField(key);
                    if (propField != null)
                    {
                        fieldName = propField.Name;
                    }
                    else
                    {
                        FieldInfo? guidField = ComTrace.FindGuidField(key.fmtid);
                        if (guidField != null)
                        {
                            fieldName = $"{guidField.Name}, {key.pid}";
                        }
                        else
                        {
                            fieldName = $"{key.fmtid}, {key.pid}";
                        }
                    }
                    string fieldValue;
                    switch (val.VariantType)
                    {
                        case PropVariantType.VT_CLSID:
                            fieldValue = ComTrace.FindGuidField(val.ToGuid())?.Name ?? val.ToString();
                            break;
                        default:
                            fieldValue = val.ToDebugString();
                            break;
                    }

                    yield return new KeyValuePair<string, string>(fieldName, fieldValue);
                }
            }

        }

        public static Guid Guid(this Enum e)
        {
            FieldInfo? fi = e.GetType().GetField(e.ToString()!);
			if (fi == null) throw new InvalidOperationException($"Field not found for enum value {e}");

			// changed for .net framework 4.0
			// EnumGuidAttribute attribute = fi.GetCustomAttribute<EnumGuidAttribute>();

			//EnumGuidAttribute attribute = Attribute.GetCustomAttribute(fi, typeof(EnumGuidAttribute)) as EnumGuidAttribute;
			//return attribute.Guid;

			EnumGuidAttribute? attribute = Attribute.GetCustomAttribute(fi, typeof(EnumGuidAttribute)) as EnumGuidAttribute;
			if (attribute == null)
			{
				throw new InvalidOperationException($"EnumGuidAttribute not found for enum value {e}");
			}

			return attribute.Guid;
		}

        public static IEnumerable<PropertyKey> ToEnum(this IPortableDeviceKeyCollection col) 
        {
            uint count = 0;
            int err = col.GetCount(ref count);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceKeyCollection), nameof(IPortableDeviceKeyCollection.GetCount));
            for (uint i = 0; i < count; i++)
            {
                PropertyKey key = new PropertyKey();
                err = col.GetAt(i, ref key);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceKeyCollection), nameof(IPortableDeviceKeyCollection.GetAt), i.ToString());
                yield return key;
            }
        }

        public static IEnumerable<TEnum> ToEnum<TEnum>(this IPortableDeviceKeyCollection col) where TEnum : struct // enum
        {
            uint count = 0;
            int err = col.GetCount(ref count);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceKeyCollection), nameof(IPortableDeviceKeyCollection.GetCount));
            for (uint i = 0; i < count; i++)
            {
                PropertyKey key = new PropertyKey();
                err = col.GetAt(i, ref key);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceKeyCollection), nameof(IPortableDeviceKeyCollection.GetAt), i.ToString());
                yield return GetEnumFromAttrKey<TEnum>(key);
            }
        }

        public static IEnumerable<TEnum> ToEnum<TEnum>(this IPortableDevicePropVariantCollection col) where TEnum : struct // enum
        {
            uint count = 0;
            int err = col.GetCount(ref count);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDevicePropVariantCollection), nameof(IPortableDevicePropVariantCollection.GetCount));
            for (uint i = 0; i < count; i++)
            {
                using (PropVariantFacade val = new PropVariantFacade())
                {
                    err = col.GetAt(i, ref val.Value);
                    MediaDeviceException.ThrowIfComError(err, nameof(IPortableDevicePropVariantCollection), nameof(IPortableDevicePropVariantCollection.GetAt), i.ToString());
                    yield return GetEnumFromAttrGuid<TEnum>(val.ToGuid());
                }
            }
        }

        public static T GetEnum<T>(this Guid guid) where T : struct
        {
            T en = Enum.GetValues(typeof(T)).Cast<T>().Where(e =>
            {
				// changed for .net framework 4.0
				// EnumGuidAttribute ea = e.GetType().GetField(e.ToString()).GetCustomAttribute<EnumGuidAttribute>();

				//EnumGuidAttribute ea = Attribute.GetCustomAttribute(e.GetType().GetField(e.ToString()), typeof(EnumGuidAttribute)) as EnumGuidAttribute;
				//return ea.Guid == guid;

				FieldInfo? fieldInfo = e.GetType().GetField(e.ToString()!);
				if (fieldInfo == null) return false;
				EnumGuidAttribute? ea = Attribute.GetCustomAttribute(fieldInfo, typeof(EnumGuidAttribute)) as EnumGuidAttribute;
				return ea != null && ea.Guid == guid;
			}).FirstOrDefault();
            return en;
        }

        public static T GetEnumFromAttrKey<T>(this PropertyKey key) where T : struct // enum
        {
            T en = Enum.GetValues(typeof(T)).Cast<T>().Where(e =>
            {
				// changed for .net framework 4.0
				// KeyAttribute attr = e.GetType().GetField(e.ToString()).GetCustomAttribute<KeyAttribute>();
				
				//KeyAttribute attr = Attribute.GetCustomAttribute(e.GetType().GetField(e.ToString()), typeof(KeyAttribute)) as KeyAttribute;

				FieldInfo? fieldInfo = e.GetType().GetField(e.ToString()!);
				if (fieldInfo == null) return false;
				KeyAttribute? attr = Attribute.GetCustomAttribute(fieldInfo, typeof(KeyAttribute)) as KeyAttribute;

				return attr != null && attr.PropertyKey == key;
            }).FirstOrDefault();
            if (en.Equals(default(T)))
            {
                Trace.TraceWarning($"Unknown {typeof(T).Name} Key {key.fmtid}  {key.pid}");
            }
            return en;
        }

        public static T GetEnumFromAttrGuid<T>(this Guid guid) where T : struct // enum
        {
            T en = Enum.GetValues(typeof(T)).Cast<T>().Where(e =>
            {
                // changed for .net framework 4.0
                // return e.GetType().GetField(e.ToString()).GetCustomAttribute<EnumGuidAttribute>().Guid == guid;
                
				//return (Attribute.GetCustomAttribute(e.GetType().GetField(e.ToString()), typeof(EnumGuidAttribute)) as EnumGuidAttribute).Guid == guid;

                FieldInfo? fieldInfo = e.GetType().GetField(e.ToString()!);
                if (fieldInfo == null) return false;
                EnumGuidAttribute? attr = Attribute.GetCustomAttribute(fieldInfo, typeof(EnumGuidAttribute)) as EnumGuidAttribute;
                return attr != null && attr.Guid == guid;
			}).FirstOrDefault();
            if (en.Equals(default(T)))
            {
                Trace.TraceWarning($"Unknown {typeof(T).Name} Guid {guid}");
            }
            return en;
        }

        public static IEnumerable<Guid> ToGuid(this IPortableDevicePropVariantCollection col) 
        {
            uint count = 0;
            int err = col.GetCount(ref count);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDevicePropVariantCollection), nameof(IPortableDevicePropVariantCollection.GetCount));
            for (uint i = 0; i < count; i++)
            {
                using (PropVariantFacade val = new())
                {
                    err = col.GetAt(i, ref val.Value);
                    MediaDeviceException.ThrowIfComError(err, nameof(IPortableDevicePropVariantCollection), nameof(IPortableDevicePropVariantCollection.GetAt), i.ToString());
                    yield return val.ToGuid();
                }
            }
        }

        public static IEnumerable<string> ToStrings(this IPortableDevicePropVariantCollection col)
        {
            uint count = 0;
            int err = col.GetCount(ref count);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDevicePropVariantCollection), nameof(IPortableDevicePropVariantCollection.GetCount));
            for (uint i = 0; i < count; i++)
            {
                using (PropVariantFacade val = new())
                {
                    err = col.GetAt(i, ref val.Value);
                    MediaDeviceException.ThrowIfComError(err, nameof(IPortableDevicePropVariantCollection), nameof(IPortableDevicePropVariantCollection.GetAt), i.ToString());
                    yield return val.ToString();
                }
            }
        }
    }
}
