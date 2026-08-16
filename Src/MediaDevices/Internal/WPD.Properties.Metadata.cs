using System;

namespace MediaDevices.Internal
{
    internal static partial class WPD
    {
	    #region Metadata

	    public static Guid MetadataServiceProperties = new Guid("{68bb7eeb-9eef-45bd-8de6-3b92a57cae1e}");

	    public static PropertyKey ContentID = new PropertyKey() {
		    fmtid = MetadataServiceProperties,
		    pid = 3
	    };

	    public static PropertyKey DefaultCAB = new PropertyKey() {
		    fmtid = MetadataServiceProperties,
		    pid = 4
	    };

	    #endregion
	}
}
