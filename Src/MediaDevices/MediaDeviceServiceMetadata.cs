using MediaDevices.Internal;
using System;

namespace MediaDevices
{
    /// <summary>
    /// Metadata service class
    /// </summary>
    public class MediaDeviceServiceMetadata : MediaDeviceService
    {
        internal MediaDeviceServiceMetadata(MediaDevice device, string serviceId) : base(device, serviceId)
        {

        }

        /// <summary>
        /// Update service 
        /// </summary>
        protected override void Update()
        {
            IPortableDeviceKeyCollection keyCol = ComFactory.CreateDeviceKeyCollection();
            AddKey(keyCol, ref WPD.ParentId);
            AddKey(keyCol, ref WPD.Name);
            AddKey(keyCol, ref WPD.PUOID);
            AddKey(keyCol, ref WPD.ObjectFormat);
            AddKey(keyCol, ref WPD.ObjectSize);
            AddKey(keyCol, ref WPD.StorageID);
            AddKey(keyCol, ref WPD.LanguageLocale);
            AddKey(keyCol, ref WPD.ContentID);
            AddKey(keyCol, ref WPD.DefaultCAB);
            
            IPortableDeviceValues values = GetProperties(keyCol);

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.ParentId, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.ParentId));
                this.ParentId = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.Name, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.Name));
                this.Name = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.PUOID, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.PUOID));
                this.PUOID = value;
            }


            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.ObjectFormat, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.ObjectFormat));
                this.ObjectFormat = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.ObjectSize, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.ObjectSize));
                this.ObjectSize = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.StorageID, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.StorageID));
                this.StorageID = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.LanguageLocale, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.LanguageLocale));
                this.LanguageLocale = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.ContentID, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.ContentID));
                this.ContentID = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = values.GetValue(ref WPD.DefaultCAB, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.DefaultCAB));
                this.DefaultCAB = value;
            }

            
        }

        private static void AddKey(IPortableDeviceKeyCollection keyCollection, ref PropertyKey key)
        {
            int err = keyCollection.Add(ref key);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceKeyCollection), nameof(IPortableDeviceKeyCollection.Add));
        }

        /// <summary>
        /// Parent ID.
        /// </summary>
        public string ParentId { get; private set; }

        /// <summary>
        /// Display name for this object.
        /// </summary>
        public new string Name { get; private set; }

        /// <summary>
        /// Persistent object unique ID. This must be a GUID.
        /// </summary>
        public string PUOID { get; private set; }

        /// <summary>
        /// MTP format code that this object represents.
        /// </summary>
        public Guid ObjectFormat { get; private set; }

        /// <summary>
        /// Size of this object in bytes.
        /// </summary>
        public ulong ObjectSize { get; private set; }

        /// <summary>
        /// Storage ID for this object.
        /// </summary>
        public string StorageID { get; private set; }

        /// <summary>
        /// Locale of the CAB contents. The locale must be composed of valid RFC4646 subtags (for example, “en-US”).
        /// </summary>
        public string LanguageLocale { get; private set; }

        /// <summary>
        /// ID that uniquely identifies the CAB contents. This ID is a GUID that is assigned by the Windows logo signing process.
        /// </summary>
        public string ContentID { get; private set; }

        /// <summary>
        /// Boolean value that indicates whether the object is the default Device Metadata CAB object. The Device Metadata service must have only one object that is marked as default.
        /// </summary>
        public bool DefaultCAB { get; private set; }
    }
}
