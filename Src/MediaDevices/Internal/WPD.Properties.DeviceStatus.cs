using System;

namespace MediaDevices.Internal
{
    internal static partial class WPD
    {
		#region Status

		/// Device Service (DEVSVC) Devices property set (DEVSVC_PROPERTY_SET_Devices).
		/// Renamed from STATUSSVC_SERVICE_PROPERTIES.
		/// Alias: DEVSVC_NAMESPACE_StatusSvc.
		public static Guid DEVSVC_PROPERTY_SET_Devices = new Guid("{49cd1f76-5626-4b17-a4e8-18b4aa1a2213}");

        public static PropertyKey SignalStrength = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 2
        };

        public static PropertyKey TextMessages = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 3
        };

        public static PropertyKey NewPictures = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 4
        };

        public static PropertyKey MissedCalls = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 5
        };

        public static PropertyKey VoiceMail = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 6
        };

        public static PropertyKey NetworkName = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 7
        };

        public static PropertyKey NetworkType = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 8
        };

        public static PropertyKey Roaming = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 9
        };

        public static PropertyKey BatteryLife = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 10
        };

        public static PropertyKey ChargingState = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 11
        };

        public static PropertyKey StorageCapacity = new PropertyKey()
        {
            fmtid = DEVSVC_PROPERTY_SET_Devices,
            pid = 12
        };

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
