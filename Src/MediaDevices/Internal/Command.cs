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
            int err = _values.SetGuidValue(ref WPD.PROPERTY_COMMON_COMMAND_CATEGORY, ref commandKey.fmtid);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetGuidValue), nameof(WPD.PROPERTY_COMMON_COMMAND_CATEGORY));
            err = _values.SetUnsignedIntegerValue(ref WPD.PROPERTY_COMMON_COMMAND_ID, commandKey.pid);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetUnsignedIntegerValue), nameof(WPD.PROPERTY_COMMON_COMMAND_ID));
        }

        public static Command Create(PropertyKey commandKey)
        {
            return new Command(commandKey);
        }

        public void Add(PropertyKey key, Guid value)
        {
            int err = _values.SetGuidValue(ref key, ref value);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetGuidValue));
        }

        public void Add(PropertyKey key, int value)
        {
            int err = _values.SetSignedIntegerValue(ref key, value);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetSignedIntegerValue));
        }

        public void Add(PropertyKey key, uint value)
        {
            int err = _values.SetUnsignedIntegerValue(ref key, value);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetUnsignedIntegerValue));
        }

        public void Add(PropertyKey key, IPortableDevicePropVariantCollection value)
        {
            int err = _values.SetIPortableDevicePropVariantCollectionValue(ref key, value);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetIPortableDevicePropVariantCollectionValue));
        }
        
        public void Add(PropertyKey key, IEnumerable<int> values)
        {
            IPortableDevicePropVariantCollection col = ComFactory.CreateDevicePropVariantCollection();
            foreach (var value in values)
            {
                var var = PropVariantFacade.IntToPropVariant(value);
                int errAdd = col.Add(ref var.Value);
                MediaDeviceException.ThrowIfComError(errAdd, nameof(IPortableDevicePropVariantCollection), nameof(IPortableDevicePropVariantCollection.Add));
            }
            int err = _values.SetIPortableDevicePropVariantCollectionValue(ref key, col);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetIPortableDevicePropVariantCollectionValue));
        }

		public void Add(PropertyKey key, IEnumerable<uint> values)
		{
			IPortableDevicePropVariantCollection col = ComFactory.CreateDevicePropVariantCollection();
			foreach (var value in values)
			{
				var var = PropVariantFacade.UIntToPropVariant(value);
				int errAdd = col.Add(ref var.Value);
				MediaDeviceException.ThrowIfComError(errAdd, nameof(IPortableDevicePropVariantCollection), nameof(IPortableDevicePropVariantCollection.Add));
			}
			int err = _values.SetIPortableDevicePropVariantCollectionValue(ref key, col);
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetIPortableDevicePropVariantCollectionValue));
		}

		public void Add(PropertyKey key, string value)
        {
            int err = _values.SetStringValue(ref key, value);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetStringValue));
        }

        //public void Add(PropertyKey key, byte[] buffer, int size)
        //{
        //    Marshal..
        //    this.values.SetBufferValue(key, ref buffer, (uint)size);
        //}

        public Guid GetGuid(PropertyKey key)
        {
            Guid value;
            int err = Result.GetGuidValue(ref key, out value);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetGuidValue));
            return value;
        }

        public int GetInt(PropertyKey key)
        {
            int value;
            int err = Result.GetSignedIntegerValue(ref key, out value);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetSignedIntegerValue));
            return value;
        }

        public uint GetUInt(PropertyKey key)
        {
            uint value;
            int err = Result.GetUnsignedIntegerValue(ref key, out value);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetUnsignedIntegerValue));
            return value;
        }

        public string GetString(PropertyKey key)
        {
            string value;
            int err = Result.GetStringValue(ref key, out value);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetStringValue));
            return value;
        }
        
        public IEnumerable<PropVariantFacade> GetPropVariants(PropertyKey key)
		{
			object? obj = null;
            int err = Result.GetIUnknownValue(ref key, out obj);
            if (err == (int)ErrorCodes.NotFound)
            {
                yield break;
            }
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetIUnknownValue));
            var col = obj as IPortableDevicePropVariantCollection;

			if (col == null)
			{
				yield break;
			}

			uint count = 0;
            int errCount = col.GetCount(ref count);
            MediaDeviceException.ThrowIfComError(errCount, nameof(IPortableDevicePropVariantCollection), nameof(IPortableDevicePropVariantCollection.GetCount));
            for (uint i = 0; i < count; i++)
            {
                PropVariantFacade val = new PropVariantFacade();
                errCount = col.GetAt(i, ref val.Value);
                MediaDeviceException.ThrowIfComError(errCount, nameof(IPortableDevicePropVariantCollection), nameof(IPortableDevicePropVariantCollection.GetAt), i.ToString());
                yield return val;
            }
        }

        public bool Has(PropertyKey key)
        {
			uint count = 0;
            int err = Result.GetCount(ref count);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetCount));
            for (uint i = 0; i < count; i++)
            {
                PropertyKey k = new PropertyKey();
                PropVariant v = new PropVariant();
                err = Result.GetAt(i, ref k, ref v);
                MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetAt), i.ToString());
                if (key == k)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Send(IPortableDevice device)
        {
            int err = device.SendCommand(0, _values, out _result);
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDevice), nameof(IPortableDevice.SendCommand));

			int error = 0;
            err = Result.GetErrorValue(ref WPD.PROPERTY_COMMON_HRESULT, out error);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetErrorValue), nameof(WPD.PROPERTY_COMMON_HRESULT));
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
			//if (Result == null) throw new InvalidOperationException("Result not initialized");
			ComTrace.WriteObject(Result);
        }
    }
}
