using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MediaDevices.Internal
{
	// to enable COM traces add "COMTRACE" to the Build Conditional compilation symbols of the MediaDevice project.

	internal static class ComTrace
	{
		private static readonly List<FieldInfo> _pKeyFields;
		private static readonly List<FieldInfo> _guidFields;

		static ComTrace()
		{
			_pKeyFields = typeof(WPD).GetFields().Where(f => f.FieldType == typeof(PropertyKey)).ToList();
			_guidFields = typeof(WPD).GetFields().Where(f => f.FieldType == typeof(Guid)).ToList();
		}

		public static FieldInfo? FindPropertyKeyField(PropertyKey key)
		{
			//return pkeyFields.SingleOrDefault(i => ((PropertyKey)i.GetValue(null)) == key);
			//return _pKeyFields.FirstOrDefault(i => ((PropertyKey)i.GetValue(null)) == key);

			// Fix CS8605: Unboxing a possibly null value.
			// i.GetValue(null) may return null, so check for null before unboxing.
			return _pKeyFields.FirstOrDefault(i =>
			{
				var value = i.GetValue(null);
				return value != null && ((PropertyKey)value) == key;
			});
		}

		public static FieldInfo? FindGuidField(Guid guid)
		{
			//return guidFields.SingleOrDefault(i => ((Guid)i.GetValue(null)) == guid);
			//return _guidFields.FirstOrDefault(i => ((Guid)i.GetValue(null)) == guid);

			// Fix CS8605: Unboxing a possibly null value.
			// i.GetValue(null) may return null, so check for null before unboxing.
			return _guidFields.FirstOrDefault(i =>
			{
				var value = i.GetValue(null);
				return value != null && ((Guid)value) == guid;
			});
		}

		[Conditional("COMTRACE")]
		public static void WriteObject(IPortableDeviceValues values)
		{
			InternalWriteObject(values);
		}

		[Conditional("COMTRACE")]
		public static void WriteObject(IPortableDeviceProperties deviceProperties, string objectId)
		{
			IPortableDeviceKeyCollection keys;
			deviceProperties.GetSupportedProperties(objectId, out keys);

			IPortableDeviceValues values;
			deviceProperties.GetValues(objectId, keys, out values);

			InternalWriteObject(values);
		}

		[Conditional("COMTRACE")]
		private static void InternalWriteObject(IPortableDeviceValues values)
		{
			string? funcName = new StackTrace().GetFrame(2)?.GetMethod()?.Name ?? "Unknown";
			Trace.WriteLine($"############################### {funcName}");

			foreach(var kvp in values.ToKeyValuePair())
			{
				Trace.WriteLine($"##### {kvp.Key} = {kvp.Value}");
			}
		}

		[Conditional("COMTRACE")]
		public static void WriteObject(IPortableDevicePropVariantCollection collection)
		{
			Trace.WriteLine("###############################");
			uint num = 0;
			collection.GetCount(ref num);
			for (uint index = 0; index < num; index++)
			{
				using (PropVariantFacade val = new PropVariantFacade())
				{
					collection.GetAt(index, ref val.Value);

					Trace.WriteLine($"##### {val.ToDebugString()}");
				}
			}
		}

		[Conditional("COMTRACE")]
		public static void WriteObject(IPortableDeviceKeyCollection collection)
		{
			Trace.WriteLine("###############################");
			uint num = 0;
			collection.GetCount(ref num);
			for (uint index = 0; index < num; index++)
			{
				PropertyKey key = new PropertyKey();
				collection.GetAt(index, ref key);

				PropertyKeys propertyKey = key.GetEnumFromAttrKey<PropertyKeys>();
				Trace.WriteLine($"##### {propertyKey}");
			}
		}
	}
}
