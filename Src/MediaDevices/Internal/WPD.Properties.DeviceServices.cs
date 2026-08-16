using System;

namespace MediaDevices.Internal
{
    internal static partial class WPD
    {
        #region DeviceServices

        /// Device Service (DEVSVC) common service property set.
        /// Alias: DEVSVC_NAMESPACE_Services.
        public static Guid DEVSVC_NAMESPACE_Services = new Guid(0x14fa7268, 0x0b6c, 0x4214, 0x94, 0x87, 0x43, 0x5b, 0x48, 0x0a, 0x8c, 0x4f);

        /// Alias: DEVSVC_PKEY_Services_ServiceDisplayName.
        public static PropertyKey ServiceDisplayName = new PropertyKey()
        {
            fmtid = DEVSVC_NAMESPACE_Services,
            pid = 2
        };

        /// Alias: DEVSVC_PKEY_Services_ServiceIcon.
        public static PropertyKey ServiceIcon = new PropertyKey()
        {
            fmtid = DEVSVC_NAMESPACE_Services,
            pid = 3
        };

        /// Alias: DEVSVC_PKEY_Services_ServiceLocale.
        public static PropertyKey ServiceLocale = new PropertyKey()
        {
            fmtid = DEVSVC_NAMESPACE_Services,
            pid = 4
        };

        /// Status service type.
        /// Alias: DEVSVC_SERVICE_Status.
        public static Guid DEVSVC_SERVICE_Status = new Guid(0x0b9f1048, 0xb94b, 0xdc9a, 0x4e, 0xd7, 0xfe, 0x4f, 0xed, 0x3a, 0x0d, 0xeb);

        #endregion
    }
}
