using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MediaDevices.Internal
{
    internal class Command
    {
        private readonly IPortableDeviceValues _values;
        private IPortableDeviceValues? _result;
		private IPortableDeviceValues Result
		{
			get
			{
				if (_result == null) throw new InvalidOperationException($"Result not initialized of {nameof(_result)}");
				return _result;
			}
		}


		private Command(PropertyKey commandKey)
        {
            _values = ComFactory.CreateDeviceValues();
            _values.SetGuidValue(ref WPD.PROPERTY_COMMON_COMMAND_CATEGORY, ref commandKey.fmtid);
            _values.SetUnsignedIntegerValue(ref WPD.PROPERTY_COMMON_COMMAND_ID, commandKey.pid);
        }

        public static Command Create(PropertyKey commandKey)
        {
            return new Command(commandKey);
        }

        public void Add(PropertyKey key, Guid value)
        {
            _values.SetGuidValue(ref key, ref value);
        }

        public void Add(PropertyKey key, int value)
        {
            _values.SetSignedIntegerValue(ref key, value);
        }

        public void Add(PropertyKey key, uint value)
        {
            _values.SetUnsignedIntegerValue(ref key, value);
        }

        public void Add(PropertyKey key, IPortableDevicePropVariantCollection value)
        {
            _values.SetIPortableDevicePropVariantCollectionValue(ref key, value);
        }
        
        public void Add(PropertyKey key, IEnumerable<int> values)
        {
            IPortableDevicePropVariantCollection col = ComFactory.CreateDevicePropVariantCollection();
            foreach (var value in values)
            {
                var var = PropVariantFacade.IntToPropVariant(value);
                col.Add(ref var.Value);
            }
            _values.SetIPortableDevicePropVariantCollectionValue(ref key, col);
        }

		public void Add(PropertyKey key, IEnumerable<uint> values)
		{
			IPortableDevicePropVariantCollection col = ComFactory.CreateDevicePropVariantCollection();
			foreach (var value in values)
			{
				var var = PropVariantFacade.UIntToPropVariant(value);
				col.Add(ref var.Value);
			}
			_values.SetIPortableDevicePropVariantCollectionValue(ref key, col);
		}

		public void Add(PropertyKey key, string value)
        {
            _values.SetStringValue(ref key, value);
        }

        //public void Add(PropertyKey key, byte[] buffer, int size)
        //{
        //    Marshal..
        //    this.values.SetBufferValue(key, ref buffer, (uint)size);
        //}

        public Guid GetGuid(PropertyKey key)
        {
            Guid value;
            Result.GetGuidValue(ref key, out value);
            return value;
        }

        public int GetInt(PropertyKey key)
        {
            int value;
            Result.GetSignedIntegerValue(ref key, out value);
            return value;
        }

        public uint GetUInt(PropertyKey key)
        {
            uint value;
            Result.GetUnsignedIntegerValue(ref key, out value);
            return value;
        }

        public string GetString(PropertyKey key)
        {
            string value;
            Result.GetStringValue(ref key, out value);
            return value;
        }
        
        public IEnumerable<PropVariantFacade> GetPropVariants(PropertyKey key)
		{
			object? obj = null;
            Result.GetIUnknownValue(ref key, out obj);
            var col = obj as IPortableDevicePropVariantCollection;

			if (col == null)
			{
				yield break;
			}

			uint count = 0;
            col.GetCount(ref count);
            for (uint i = 0; i < count; i++)
            {
                PropVariantFacade val = new PropVariantFacade();
                col.GetAt(i, ref val.Value);
                yield return val;
            }
        }

        public bool Has(PropertyKey key)
        {
			uint count = 0;
            Result.GetCount(ref count);
            for (uint i = 0; i < count; i++)
            {
                PropertyKey k = new PropertyKey();
                PropVariant v = new PropVariant();
                Result.GetAt(i, ref k, ref v);
                if (key == k)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Send(IPortableDevice device)
        {
            device.SendCommand(0, _values, out _result);

			int error = 0;
            Result.GetErrorValue(ref WPD.PROPERTY_COMMON_HRESULT, out error);
            switch ((HResult)error)
            {
            case HResult.S_OK:
                return true;
            case HResult.E_NOT_IMPLEMENTED:
                Debug.WriteLine("Command not implemented!");
                return false;
            default:
                throw new Exception($"Error {error:X}");
            }
        }

        [Conditional("COMTRACE")]
        public void WriteResults()
        {
			if (Result == null) throw new InvalidOperationException("Result not initialized");
			ComTrace.WriteObject(Result);
        }
    }
}
