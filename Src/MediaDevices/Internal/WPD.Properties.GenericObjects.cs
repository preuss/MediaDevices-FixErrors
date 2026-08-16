using System;

namespace MediaDevices.Internal
{
    internal static partial class WPD
    {
        #region Generic

        public static Guid GenericObjectProperties = new Guid("{ef6b490d-5cd8-437a-affc-da8b60ee4a3c}");

        public static PropertyKey ParentId = new PropertyKey()
        {
            fmtid = GenericObjectProperties,
            pid = 3
        };

        public static PropertyKey Name = new PropertyKey()
        {
            fmtid = GenericObjectProperties,
            pid = 4
        };

        public static PropertyKey PUOID = new PropertyKey()
        {
            fmtid = GenericObjectProperties,
            pid = 5
        };

        public static PropertyKey ObjectFormat = new PropertyKey()
        {
            fmtid = GenericObjectProperties,
            pid = 6
        };

        public static PropertyKey ObjectSize = new PropertyKey()
        {
            fmtid = GenericObjectProperties,
            pid = 11
        };

        public static PropertyKey StorageID = new PropertyKey()
        {
            fmtid = GenericObjectProperties,
            pid = 23
        };

        public static PropertyKey LanguageLocale = new PropertyKey()
        {
            fmtid = GenericObjectProperties,
            pid = 27
        };
		#endregion
    }
}
