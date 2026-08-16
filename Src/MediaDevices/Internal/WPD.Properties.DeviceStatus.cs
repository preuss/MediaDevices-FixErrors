using System;

namespace MediaDevices.Internal
{
    internal static partial class WPD
    {
		#region Status

		/// Device Service (DEVSVC) Devices property set (DEVSVC_PROPERTY_SET_Devices).
		/// Renamed from STATUSSVC_SERVICE_PROPERTIES.
		/// Alias: DEVSVC_NAMESPACE_StatusSvc, STATUSSVC_SERVICE_PROPERTIES.
		public static Guid DEVSVC_PROPERTY_SET_Devices = new Guid("{49cd1f76-5626-4b17-a4e8-18b4aa1a2213}");

        /// Alias: DEVSVC_PKEY_StatusSvc_SignalStrength.
        public static PropertyKey SignalStrength = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 2
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_TextMessages.
        public static PropertyKey TextMessages = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 3
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_NewPictures.
        public static PropertyKey NewPictures = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 4
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_MissedCalls.
        public static PropertyKey MissedCalls = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 5
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_VoiceMail.
        public static PropertyKey VoiceMail = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 6
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_NetworkName.
        public static PropertyKey NetworkName = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 7
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_NetworkType.
        public static PropertyKey NetworkType = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 8
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_Roaming.
        public static PropertyKey Roaming = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 9
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_BatteryLife.
        public static PropertyKey BatteryLife = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 10
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_ChargingState.
        public static PropertyKey ChargingState = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 11
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_StorageCapacity.
        public static PropertyKey StorageCapacity = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 12
        };

        /// Alias: DEVSVC_PKEY_StatusSvc_StorageFreeSpace.
        public static PropertyKey StorageFreeSpace = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 13
        };


        public static PropertyKey InternetConnected = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 15
        };

		#endregion
	}
}
