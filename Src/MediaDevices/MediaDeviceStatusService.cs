using MediaDevices.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaDevices;

namespace MediaDevices
{
    /// <summary>
    /// Status service class
    /// </summary>
    public class MediaDeviceStatusService : MediaDeviceService
    {
        internal MediaDeviceStatusService(MediaDevice device, string serviceId) : base(device, serviceId)
        {

        }

        /// <summary>
        /// Update service
        /// </summary>
        protected override void Update()
        {
            IPortableDeviceKeyCollection keyCol = ComFactory.CreateDeviceKeyCollection();
            AddKey(keyCol, ref WPD.SignalStrength);
            AddKey(keyCol, ref WPD.TextMessages);
            AddKey(keyCol, ref WPD.NewPictures);
            AddKey(keyCol, ref WPD.MissedCalls);
            AddKey(keyCol, ref WPD.VoiceMail);
            AddKey(keyCol, ref WPD.NetworkName);
            AddKey(keyCol, ref WPD.NetworkType);
            AddKey(keyCol, ref WPD.Roaming);
            AddKey(keyCol, ref WPD.BatteryLife);
            AddKey(keyCol, ref WPD.ChargingState);
            AddKey(keyCol, ref WPD.StorageCapacity);
            AddKey(keyCol, ref WPD.StorageFreeSpace);
            AddKey(keyCol, ref WPD.InternetConnected);
            IPortableDeviceValues values = GetProperties(keyCol);

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.SignalStrength, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.SignalStrength));
				SignalStrength = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.TextMessages, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.TextMessages));
                TextMessages = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.NewPictures, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.NewPictures));
                NewPictures = value;
            }


            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.MissedCalls, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.MissedCalls));
                MissedCalls = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.VoiceMail, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.VoiceMail));
                VoiceMail = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.NetworkName, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.NetworkName));
                NetworkName = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.NetworkType, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.NetworkType));
                NetworkType = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.Roaming, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.Roaming));
                Roaming = (Roaming)(byte)value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.BatteryLife, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.BatteryLife));
                BatteryLife = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.ChargingState, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.ChargingState));
                ChargingState = (ChargingState)(byte)value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.StorageCapacity, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.StorageCapacity));
                StorageCapacity = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.StorageFreeSpace, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.StorageFreeSpace));
                StorageFreeSpace = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.InternetConnected, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.InternetConnected));
                InternetConnected = value;
            }
        }

        private static void AddKey(IPortableDeviceKeyCollection keyCollection, ref PropertyKey key)
        {
            int err = keyCollection.Add(ref key);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceKeyCollection), nameof(IPortableDeviceKeyCollection.Add));
        }

        /// <summary>
        /// Signal strength, from 0 to 4.
        /// </summary>
        public byte SignalStrength { get; private set; }

        /// <summary>
        /// Number of unread text messages.
        /// </summary>
        public byte TextMessages { get; private set; }

        /// <summary>
        /// Total number of pictures on the device.
        /// </summary>
        public ushort NewPictures { get; private set; }

        /// <summary>
        /// Total number of missed calls on the device.
        /// </summary>
        public byte MissedCalls { get; private set; }

        /// <summary>
        /// Total number of new voicemail messages on the device/service. 
        /// For devices that have only a binary state, 0 represents no new voicemail messages and 0xFF represents new messages.
        /// </summary>
        public byte VoiceMail { get; private set; }

        /// <summary>
        /// Human-readable name of the current mobile network (for example, “Microsoft Cellular”).
        /// </summary>
        public string NetworkName { get; private set; } = string.Empty;

        /// <summary>
        /// Type of mobile network that the device is currently using (for example, “E” for EDGE, “U” for UMTS, or “1x” for 1xRTT).
        /// </summary>
        public string NetworkType { get; private set; } = string.Empty;

        /// <summary>
        /// Roaming type.
        /// </summary>
        public Roaming Roaming { get; private set; }

        /// <summary>
        /// Remaining battery life of the device, as an integer from 0 to 100.
        /// </summary>
        public byte BatteryLife { get; private set; }

        /// <summary>
        /// Charging state
        /// </summary>
        public ChargingState ChargingState { get; private set; }

        /// <summary>
        /// Total usable storage capacity of the device, in bytes, across all storage locations.
        /// </summary>
        public ulong StorageCapacity { get; private set; }

        /// <summary>
        /// Total usable free space on the device, in bytes, across all storage locations.
        /// </summary>
        public ulong StorageFreeSpace { get; private set; }

        /// <summary>
        /// Boolean value that indicates whether the mobile device is connected to an outside data network (such as the Internet).
        /// </summary>
        public bool InternetConnected { get; private set; }

    }
}
