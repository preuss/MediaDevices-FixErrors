using MediaDevices.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaDevices
{

    // C:\Program Files (x86)\Windows Kits\10\Include\10.0.17763.0\um\propkey.h

    /// <summary>
    /// MediaDevice service class
    /// </summary>
    public class MediaDeviceService : IDisposable
    {
        private readonly MediaDevice _device;
        private IPortableDeviceService? _deviceService;
        //protected IPortableDeviceValues values;
        private readonly IPortableDeviceServiceCapabilities _capabilities;
        internal IPortableDeviceContent2 content;

        internal MediaDeviceService(MediaDevice device, string serviceId)
        {
	        _deviceService = ComFactory.CreateDeviceService();

			this._device = device;
            ServiceId = serviceId;

            //Match match = Regex.Match(serviceId, @".*#(?<service>\{.*\})\\(?<name>\{.*\})");
            //if (match.Success)
            //{
            //    string service = match.Groups["service"].Value;
            //    Guid serviceGuid = new Guid(service);
            //    this.Service = serviceGuid.GetEnum<Services>();
            //    string serviceName = match.Groups["name"].Value;
            //    this.ServiceName = $"{this.Service} : {service} : {serviceName}";
            //}
            //else
            //{
            //    this.ServiceName = "Unknown";
            //}
            //this.ServiceName = serviceId.Substring(serviceId.LastIndexOf(@"\") + 1);

            IPortableDeviceValues values = ComFactory.CreateDeviceValues();
			DeviceService.Open(ServiceId, values);

            DeviceService.GetServiceObjectID(out string serviceObjectID);
            ServiceObjectID = serviceObjectID;

            DeviceService.GetPnPServiceID(out string pnPServiceID);
            PnPServiceID = pnPServiceID;

            DeviceService.Capabilities(out _capabilities);

            DeviceService.Content(out content);

            content.Properties(out IPortableDeviceProperties properties);

            properties.GetSupportedProperties(ServiceObjectID, out IPortableDeviceKeyCollection keyCol);

            properties.GetValues(ServiceObjectID, keyCol, out IPortableDeviceValues deviceValues);

            ComTrace.WriteObject(deviceValues);

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'

            using (PropVariantFacade value = new PropVariantFacade())
            {
				deviceValues.GetValue(ref WPD.OBJECT_NAME, out value.Value);
                Name = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
				deviceValues.GetValue(ref WPD.FUNCTIONAL_OBJECT_CATEGORY, out value.Value);
                
                Guid serviceGuid = new Guid((string)value);
                Service = serviceGuid.GetEnum<MediaDeviceServices>();
                ServiceName = Service != MediaDeviceServices.Unknown ? Service.ToString() : serviceGuid.ToString();
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                deviceValues.GetValue(ref WPD.SERVICE_VERSION, out value.Value);
                ServiceVersion = value;
            }

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression

	        // ReSharper disable once VirtualMemberCallInConstructor
	        Update();

            //var x = GetContent().ToArray();

            
        }

        private IPortableDeviceService DeviceService
        {
	        get
	        {
				if(_deviceService == null) {
					throw new ObjectDisposedException(
						nameof(MediaDeviceService),
						"Service has already been disposed, and is not available."
					);
				}

				return _deviceService;
	        }
        }

		/// <summary>
		/// Dispose service
		/// </summary>
		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose service
        /// </summary>
        /// <param name="disposing">Disposing flag</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_deviceService != null)
                {
                    _deviceService.Close();
                    _deviceService = null;
                }
            }
        }

        /// <summary>
        /// ID of the service
        /// </summary>
        public string ServiceId { get; private set; }

        /// <summary>
        /// Get services
        /// </summary>
        public MediaDeviceServices Service { get; private set; }

        /// <summary>
        /// Name of the service
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// ServiceName
        /// </summary>
        public string ServiceName { get; private set; }

        /// <summary>
        /// Version of the service
        /// </summary>
        public string ServiceVersion { get; private set; }

        /// <summary>
        /// ObjectID of the service
        /// </summary>
        public string ServiceObjectID { get; private set; }

        /// <summary>
        /// PnP service ID
        /// </summary>
        public string PnPServiceID { get; private set; }

        /// <summary>
        /// Info of the service
        /// </summary>
        /// <returns>String with the info</returns>
        public override string ToString()
        {
            return $"{Name} : {ServiceName} : {ServiceVersion}";
        }

        /// <summary>
        /// Get content of the service
        /// </summary>
        /// <returns>List of content services</returns>
        public IEnumerable<MediaDeviceServiceContent> GetContent()
        {
            return GetContent("DEVICE");
        }

        internal IEnumerable<MediaDeviceServiceContent> GetContent(string objectID)
        {
            content.EnumObjects(0, objectID, null, out IEnumPortableDeviceObjectIDs enumerator);

            uint num = 0;
            string[] objectIdArray = new string[20];
            enumerator.Next(20, objectIdArray, ref num);

            return objectIdArray.Take((int)num).Select(o => new MediaDeviceServiceContent(this, o));
        }

        internal IPortableDeviceValues GetAllProperties(string objectID)
        {
            content.Properties(out IPortableDeviceProperties properties);

            properties.GetSupportedProperties(objectID, out IPortableDeviceKeyCollection keyCol);

            properties.GetValues(objectID, keyCol, out IPortableDeviceValues deviceValues);

            return deviceValues;
        }
               
        internal IPortableDeviceValues GetProperties(IPortableDeviceKeyCollection keyCol)
        {
            content.Properties(out IPortableDeviceProperties properties);

            properties.GetValues(ServiceObjectID, keyCol, out IPortableDeviceValues deviceValues);

            return deviceValues;
        }

		/// <summary>
		/// Updates the service state. 
		/// 
		/// <para>
		/// <b>Note:</b> This base implementation is intended for debugging only and writes all properties to the trace output.
		/// It should <b>not</b> be called directly in production code. 
		/// Always override this method in derived classes to provide service-specific update logic.
		/// </para>
		/// </summary>
		/// <remarks>
		/// The base implementation is only for diagnostic purposes and should not be relied upon for actual service updates.
		/// </remarks>
		protected virtual void Update()
		{
			// Debug: Write all properties to trace output for inspection.
			IPortableDeviceValues deviceValues = GetAllProperties(ServiceObjectID);
			ComTrace.WriteObject(deviceValues);
		}

		/// <summary>
		/// Get all properties
		/// </summary>
		/// <returns>List of properties</returns>
		public IEnumerable<KeyValuePair<string,string>> GetAllProperties()
        {
            return GetAllProperties(ServiceObjectID).ToKeyValuePair();
        }

        /// <summary>
        /// Get supported methods
        /// </summary>
        /// <returns>List of supported methods</returns>
        public IEnumerable<Methods> GetSupportedMethods()
        {
            _capabilities.GetSupportedMethods(out IPortableDevicePropVariantCollection methods);
            ComTrace.WriteObject(methods);
            return methods.ToEnum<Methods>();
        }

        /// <summary>
        /// Get supported commands
        /// </summary>
        /// <returns>List of supported commands</returns>
        public IEnumerable<Commands> GetSupportedCommands()
        {
            _capabilities.GetSupportedCommands(out IPortableDeviceKeyCollection commands);
            ComTrace.WriteObject(commands);
            return commands.ToEnum<Commands>();
        }

        /// <summary>
        /// Get supported events
        /// </summary>
        /// <returns>list of supported events</returns>
        public IEnumerable<Events> GetSupportedEvents()
        {
            _capabilities.GetSupportedEvents(out IPortableDevicePropVariantCollection events);
            ComTrace.WriteObject(events);
            return events.ToEnum<Events>();
        }

        /// <summary>
        /// Get supported formats
        /// </summary>
        /// <returns>List of supported formats</returns>
        public IEnumerable<Formats> GetSupportedFormats()
        {
            _capabilities.GetSupportedFormats(out IPortableDevicePropVariantCollection formats);
            ComTrace.WriteObject(formats);
            return formats.ToEnum<Formats>();
        }
       
        /// <summary>
        /// Call a service method
        /// </summary>
        /// <param name="method">Method GUID</param>
        /// <param name="parameters">Method parameters</param>
#pragma warning disable IDE0060 // Remove unused parameter
        public void CallMethod(Guid method, object[] parameters)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            DeviceService.Methods(out IPortableDeviceServiceMethods methods);

            IPortableDeviceValues values = ComFactory.CreateDeviceValues();
            //values.SetStringValue();
            IPortableDeviceValues results = ComFactory.CreateDeviceValues();
            methods.Invoke(ref method, ref values, ref results);
        }

        internal void SendCommand(PropertyKey commandKey)
        {
            IPortableDeviceValues values = ComFactory.CreateDeviceValues();
			values.SetGuidValue(ref WPD.PROPERTY_COMMON_COMMAND_CATEGORY, ref commandKey.fmtid);
			values.SetUnsignedIntegerValue(ref WPD.PROPERTY_COMMON_COMMAND_ID, commandKey.pid);

#pragma warning disable IDE0059 // Unnecessary assignment of a value
            DeviceService.SendCommand(0, ref values, out IPortableDeviceValues results);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }

        
    }
}
