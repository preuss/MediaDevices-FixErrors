using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;

namespace MediaDevices.Internal {
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// The Facade is necessary because structs used in using are readonly and can not be filled with ref or out.
	/// </remarks>
	internal sealed class PropVariantFacade : IDisposable {
		// cannot be a property because it will be filled by reference
		public PropVariant Value = new();

		public void Dispose() {
			// clear only if filled
			if(Value.vt != 0) {
				try {
					// clear propVariant clears also included objects like strings
					NativeMethods.PropVariantClear(ref Value);
				} catch(Exception ex) {
					Trace.TraceError(ex.ToString());
				}
			}
		}

		public PropVariantType VariantType {
			get { return Value.vt; }
		}

		public string ToDebugString() {
			if(Value.vt == PropVariantType.VT_ERROR) {
				int error = ToError();
				string name = Enum.GetName(typeof(HResult), error) ?? error.ToString("X");
				return $"Error: {name}";
			}
			return ToString();
		}

		public override string ToString()
		{
			try
			{
				switch (Value.vt)
				{
					case PropVariantType.VT_LPSTR:
						return Marshal.PtrToStringAnsi(Value.ptrVal) ?? "null";

					case PropVariantType.VT_LPWSTR:
						return Marshal.PtrToStringUni(Value.ptrVal) ?? "null";

					case PropVariantType.VT_BSTR:
						return Marshal.PtrToStringBSTR(Value.ptrVal);

					case PropVariantType.VT_CLSID:
						return ToGuid().ToString();

					case PropVariantType.VT_DATE:
						return ToNullableDate()?.ToString(CultureInfo.InvariantCulture) ?? "null";

					case PropVariantType.VT_BOOL:
						return ToBool().ToString();

					case PropVariantType.VT_INT:
					case PropVariantType.VT_I1:
					case PropVariantType.VT_I2:
					case PropVariantType.VT_I4:
						return ToInt().ToString();

					case PropVariantType.VT_UINT:
					case PropVariantType.VT_UI1:
					case PropVariantType.VT_UI2:
					case PropVariantType.VT_UI4:
						return ToUInt().ToString();

					case PropVariantType.VT_I8:
						return ToLong().ToString();

					case PropVariantType.VT_UI8:
						return ToUlong().ToString();

					case PropVariantType.VT_ERROR:
						Debug.WriteLine($"VT_ERROR: 0x{Value.errorCode:X}");
						return "";

					default:
						Debug.WriteLine($"Unknown PropVariantType: {Value.vt} (raw: {Value.vt})");
						return "";
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Exception in ToString for PropVariantType {Value.vt}: {ex}");
				return "null";
			}
		}

		public int ToInt() {
			if(Value.vt == PropVariantType.VT_ERROR) {
				Debug.WriteLine($"VT_ERROR_int: 0x{Value.errorCode:X}");
				return 0;
			}

			if(Value.vt != PropVariantType.VT_INT
			 && Value.vt != PropVariantType.VT_I1
			 && Value.vt != PropVariantType.VT_I2
			 && Value.vt != PropVariantType.VT_I4) {
				throw new InvalidOperationException($"ToInt does not work for value type {Value.vt}");
			}

			return Value.intVal;
		}

		public uint ToUInt() {
			if(Value.vt == PropVariantType.VT_ERROR) {
				Debug.WriteLine($"VT_ERROR_uint: 0x{Value.errorCode:X}");
				return 0;
			}

			if(Value.vt != PropVariantType.VT_UINT
			 && Value.vt != PropVariantType.VT_UI1
			 && Value.vt != PropVariantType.VT_UI2
			 && Value.vt != PropVariantType.VT_UI4) {
				throw new InvalidOperationException($"ToUInt does not work for value type {Value.vt}");
			}

			return Value.uintVal;
		}

		public long ToLong() {
			if(Value.vt == PropVariantType.VT_ERROR) {
				Debug.WriteLine($"VT_ERROR_long: 0x{Value.errorCode:X}");
				return 0;
			}

			if(Value.vt != PropVariantType.VT_INT
			 && Value.vt != PropVariantType.VT_I1
			 && Value.vt != PropVariantType.VT_I2
			 && Value.vt != PropVariantType.VT_I4
			 && Value.vt != PropVariantType.VT_I8) {
				throw new InvalidOperationException($"ToLong does not work for value type {Value.vt}");
			}

			return Value.longVal;
		}

		public ulong ToUlong() {
			if(Value.vt == PropVariantType.VT_ERROR) {
				Debug.WriteLine($"VT_ERROR_ulong: 0x{Value.errorCode:X}");
				return 0;
			}

			if(Value.vt != PropVariantType.VT_UINT
			 && Value.vt != PropVariantType.VT_UI1
			 && Value.vt != PropVariantType.VT_UI2
			 && Value.vt != PropVariantType.VT_UI4
			 && Value.vt != PropVariantType.VT_UI8) {
				throw new InvalidOperationException($"ToUlong does not work for value type {Value.vt}");
			}

			return Value.ulongVal;
		}
		/// <summary>
		/// Converts the underlying PROPVARIANT value to a <see cref="DateTime"/>.
		/// </summary>
		/// <returns>
		/// The corresponding <see cref="DateTime"/> if the value is valid.
		/// </returns>
		/// <remarks>
		/// If the PROPVARIANT type is <c>VT_ERROR</c>, returns a default <see cref="DateTime"/>.
		/// If the value is not of type <c>VT_DATE</c>, throws <see cref="InvalidOperationException"/>.
		/// If the underlying OLE Automation date is invalid, <see cref="DateTime.FromOADate"/> will throw an exception.
		/// </remarks>
		public DateTime ToDate() {
			if(Value.vt == PropVariantType.VT_ERROR) {
				Debug.WriteLine($"VT_ERROR_date: 0x{Value.errorCode:X}");
				return new DateTime();
			}

			if(Value.vt != PropVariantType.VT_DATE) {
				throw new InvalidOperationException($"ToDate does not work for value type {Value.vt}");
			}

			return DateTime.FromOADate(Value.dateVal);
		}

		/// <summary>
		/// Converts the underlying PROPVARIANT value to a nullable <see cref="DateTime"/>.
		/// </summary>
		/// <returns>
		/// The corresponding <see cref="DateTime"/> if the value is valid; otherwise <c>null</c> if the value is empty, invalid, or represents "no date".
		/// </returns>
		/// <remarks>
		/// Returns <c>null</c> for typical "no value" OLE Automation dates (e.g. 0.0, 1.0, NaN, Infinity, or out-of-range values).
		/// Throws <see cref="InvalidOperationException"/> if the PROPVARIANT type is not <c>VT_DATE</c>.
		/// </remarks>
		public DateTime? ToNullableDate() {
			if(Value.vt == PropVariantType.VT_ERROR) {
				Debug.WriteLine($"VT_ERROR_nullabledate: 0x{Value.errorCode:X}");
				return null;
			}

			if(Value.vt != PropVariantType.VT_DATE) {
				throw new InvalidOperationException($"ToDate does not work for value type {Value.vt}");
			}

			double rawDateTime = Value.dateVal;
			// To catch typical "no date time" values from different devices
			if(rawDateTime == 0.0
			   // ReSharper disable once CompareOfFloatsByEqualityOperator
			   || rawDateTime == 1.0
			   || double.IsNaN(rawDateTime)
			   || double.IsInfinity(rawDateTime)
			   || rawDateTime < -657434.0 // before year 1000
			   || rawDateTime > 2958465.0 // after year 9999
			) {
				return null;
			}

			DateTime dateTime = DateTime.FromOADate(Value.dateVal);
			// If the date time is the default value, return null for no value
			return DateTime.MinValue.Equals(dateTime) ? null : (DateTime?)dateTime;
		}

		public bool ToBool() {
			if(Value.vt == PropVariantType.VT_ERROR) {
				Debug.WriteLine($"VT_ERROR_bool: 0x{Value.errorCode:X}");
				return false;
			}

			if(Value.vt != PropVariantType.VT_BOOL) {
				throw new InvalidOperationException($"ToBool does not work for value type {Value.vt}");
			}

			return Value.boolVal != 0;
		}

		public Guid ToGuid() {
			if(Value.vt == PropVariantType.VT_ERROR) {
				Debug.WriteLine($"VT_ERROR_guid: 0x{Value.errorCode:X}");
				return Guid.Empty;
			}

			if(Value.vt != PropVariantType.VT_CLSID) {
				throw new InvalidOperationException($"ToGuid does not work for value type {Value.vt}");
			}

			if(Value.ptrVal == IntPtr.Zero) {
				Debug.WriteLine("ToGuid: ptrVal is IntPtr.Zero");
				return Guid.Empty;
			}

			try
			{
				object? guidObj = Marshal.PtrToStructure(Value.ptrVal, typeof(Guid));
				if (guidObj is not Guid guid)
				{
					Debug.WriteLine("ToGuid: Marshal.PtrToStructure returned null or wrong type");
					return Guid.Empty;
				}
				return guid;
			} catch(Exception ex)
			{
				Debug.WriteLine($"ToGuid: Exception during Marshal.PtrToStructure: {ex}");
				return Guid.Empty;
			}
		}

#if !NETCOREAPP
        [HandleProcessCorruptedStateExceptions]
#endif
		[SecurityCritical]
		public byte[]? ToByteArray() {
			if(Value.vt == PropVariantType.VT_ERROR) {
				Debug.WriteLine($"VT_ERROR_bytearray: 0x{Value.errorCode:X}");
				return null;
			}

			if(Value.vt != (PropVariantType.VT_VECTOR | PropVariantType.VT_UI1)) {
				throw new InvalidOperationException($"ToByteArray does not work for value type {Value.vt}");
			}

			int size = (int)Value.dataVal.cData;
			if(size < 0 || Value.dataVal.pData == IntPtr.Zero)
			{
				Debug.WriteLine($"ToByteArray: Invalid size ({size}) or pData is null");
				return null;
			}

			try
			{
				byte[] managedArray = new byte[size];

				// bug fixed with manual COM wrapper classes
				Marshal.Copy(Value.dataVal.pData, managedArray, 0, size);
				return managedArray;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"ToByteArray: Exception during Marshal.Copy: {ex}");
				return null;
			}
		}

		public int ToError() {
			if(Value.vt != PropVariantType.VT_ERROR) {
				return 0;
			}

			return Value.errorCode;
		}

		public static PropVariantFacade StringToPropVariant(string value) {
			PropVariantFacade pv = new PropVariantFacade();
			pv.Value.vt = PropVariantType.VT_LPWSTR;
			// Hack, see GetString
			pv.Value.ptrVal = Marshal.StringToCoTaskMemUni(value);
			return pv;
		}

		public static PropVariantFacade UIntToPropVariant(uint value) {
			PropVariantFacade pv = new PropVariantFacade();
			pv.Value.vt = PropVariantType.VT_UI4;
			pv.Value.uintVal = value;
			return pv;
		}

		public static PropVariantFacade IntToPropVariant(int value) {
			PropVariantFacade pv = new PropVariantFacade();
			pv.Value.vt = PropVariantType.VT_INT;
			pv.Value.intVal = value;
			return pv;
		}

		public static PropVariantFacade DateTimeToPropVariant(DateTime value) {
			PropVariantFacade pv = new PropVariantFacade();
			pv.Value.vt = PropVariantType.VT_DATE;
			pv.Value.dateVal = value.ToOADate();
			return pv;
		}

		public static PropVariantFacade DateTimeToPropVariant(DateTime? value) {
			PropVariantFacade pv = new PropVariantFacade();
			pv.Value.vt = PropVariantType.VT_DATE;
			pv.Value.dateVal = value?.ToOADate() ?? DateTime.MinValue.ToOADate();
			return pv;
		}

		public static implicit operator string(PropVariantFacade val) {
			return val.ToString();
		}

		public static implicit operator bool(PropVariantFacade val) {
			return val.ToBool();
		}

		public static implicit operator DateTime(PropVariantFacade val) {
			return val.ToDate();
		}

		public static implicit operator DateTime?(PropVariantFacade val) {
			return val.ToNullableDate();
		}

		public static implicit operator Guid(PropVariantFacade val) {
			return val.ToGuid();
		}

		public static implicit operator int(PropVariantFacade val) {
			return val.ToInt();
		}

		public static implicit operator byte(PropVariantFacade val) {
			return (byte)val.ToUInt();
		}

		public static implicit operator ulong(PropVariantFacade val) {
			return val.ToUlong();
		}

		public static implicit operator byte[](PropVariantFacade val) {
			return val.ToByteArray() ?? Array.Empty<byte>();
		}

		private static class NativeMethods {
			[DllImport("ole32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
			public static extern int PropVariantClear(ref PropVariant val);
		}
	}
}
