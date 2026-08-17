using MediaDevices.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MediaDevices;

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
			int errService = DeviceService.Open(ServiceId, values);
			MediaDeviceException.ThrowIfComError(errService, nameof(IPortableDeviceService), nameof(IPortableDeviceService.Open), ServiceId);

			errService = DeviceService.GetServiceObjectID(out string serviceObjectID);
			MediaDeviceException.ThrowIfComError(errService, nameof(IPortableDeviceService), nameof(IPortableDeviceService.GetServiceObjectID));
			ServiceObjectID = serviceObjectID;

			errService = DeviceService.GetPnPServiceID(out string pnPServiceID);
			MediaDeviceException.ThrowIfComError(errService, nameof(IPortableDeviceService), nameof(IPortableDeviceService.GetPnPServiceID));
			PnPServiceID = pnPServiceID;

			errService = DeviceService.Capabilities(out _capabilities);
			MediaDeviceException.ThrowIfComError(errService, nameof(IPortableDeviceService), nameof(IPortableDeviceService.Capabilities));

			errService = DeviceService.Content(out content);
			MediaDeviceException.ThrowIfComError(errService, nameof(IPortableDeviceService), nameof(IPortableDeviceService.Content));

            int errProperties = content.Properties(out IPortableDeviceProperties properties);
            MediaDeviceException.ThrowIfComError(errProperties, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.Properties), ServiceObjectID);

            int err = properties.GetSupportedProperties(ServiceObjectID, out IPortableDeviceKeyCollection keyCol);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceProperties), nameof(IPortableDeviceProperties.GetSupportedProperties), ServiceObjectID);

            err = properties.GetValues(ServiceObjectID, keyCol, out IPortableDeviceValues deviceValues);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceProperties), nameof(IPortableDeviceProperties.GetValues), ServiceObjectID);

            ComTrace.WriteObject(deviceValues);

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'

            using (PropVariantFacade value = new PropVariantFacade())
            {
				int errValue = deviceValues.GetValue(ref WPD.OBJECT_NAME, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.OBJECT_NAME));
                Name = value;
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
				int errValue = deviceValues.GetValue(ref WPD.FUNCTIONAL_OBJECT_CATEGORY, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.FUNCTIONAL_OBJECT_CATEGORY));
                
                Guid serviceGuid = new Guid((string)value);
                Service = serviceGuid.GetEnum<MediaDeviceServices>();
                ServiceName = Service != MediaDeviceServices.Unknown ? Service.ToString() : serviceGuid.ToString();
            }

            using (PropVariantFacade value = new PropVariantFacade())
            {
                int errValue = deviceValues.GetValue(ref WPD.SERVICE_VERSION, out value.Value);
                MediaDeviceException.ThrowIfComError(errValue, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.GetValue), nameof(WPD.SERVICE_VERSION));
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
                    int err = _deviceService.Close();
                    MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceService), nameof(IPortableDeviceService.Close));
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
            int errEnum = content.EnumObjects(0, objectID, null, out IEnumPortableDeviceObjectIDs enumerator);
            MediaDeviceException.ThrowIfComError(errEnum, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.EnumObjects), ServiceObjectID);

            uint num = 0;
            string[] objectIdArray = new string[20];
            errEnum = enumerator.Next(20, objectIdArray, ref num);
            MediaDeviceException.ThrowIfComError(errEnum, nameof(IEnumPortableDeviceObjectIDs), nameof(IEnumPortableDeviceObjectIDs.Next));

            return objectIdArray.Take((int)num).Select(o => new MediaDeviceServiceContent(this, o));
        }

        internal IPortableDeviceValues GetAllProperties(string objectID)
        {
            int errProperties = content.Properties(out IPortableDeviceProperties properties);
            MediaDeviceException.ThrowIfComError(errProperties, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.Properties), objectID);

            int err = properties.GetSupportedProperties(objectID, out IPortableDeviceKeyCollection keyCol);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceProperties), nameof(IPortableDeviceProperties.GetSupportedProperties), objectID);

            err = properties.GetValues(objectID, keyCol, out IPortableDeviceValues deviceValues);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceProperties), nameof(IPortableDeviceProperties.GetValues), objectID);

            return deviceValues;
        }
               
        internal IPortableDeviceValues GetProperties(IPortableDeviceKeyCollection keyCol)
        {
            int errProperties = content.Properties(out IPortableDeviceProperties properties);
            MediaDeviceException.ThrowIfComError(errProperties, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.Properties), ServiceObjectID);

            int err = properties.GetValues(ServiceObjectID, keyCol, out IPortableDeviceValues deviceValues);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceProperties), nameof(IPortableDeviceProperties.GetValues), ServiceObjectID);

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
            int err = _capabilities.GetSupportedMethods(out IPortableDevicePropVariantCollection methods);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceServiceCapabilities), nameof(IPortableDeviceServiceCapabilities.GetSupportedMethods));
            ComTrace.WriteObject(methods);
            return methods.ToEnum<Methods>();
        }

        /// <summary>
        /// Get supported commands
        /// </summary>
        /// <returns>List of supported commands</returns>
        public IEnumerable<Commands> GetSupportedCommands()
        {
            int err = _capabilities.GetSupportedCommands(out IPortableDeviceKeyCollection commands);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceServiceCapabilities), nameof(IPortableDeviceServiceCapabilities.GetSupportedCommands));
            ComTrace.WriteObject(commands);
            return commands.ToEnum<Commands>();
        }

        /// <summary>
        /// Get supported events
        /// </summary>
        /// <returns>list of supported events</returns>
        public IEnumerable<Events> GetSupportedEvents()
        {
            int err = _capabilities.GetSupportedEvents(out IPortableDevicePropVariantCollection events);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceServiceCapabilities), nameof(IPortableDeviceServiceCapabilities.GetSupportedEvents));
            ComTrace.WriteObject(events);
            return events.ToEnum<Events>();
        }

        /// <summary>
        /// Get supported formats
        /// </summary>
        /// <returns>List of supported formats</returns>
        public IEnumerable<Formats> GetSupportedFormats()
        {
            int err = _capabilities.GetSupportedFormats(out IPortableDevicePropVariantCollection formats);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceServiceCapabilities), nameof(IPortableDeviceServiceCapabilities.GetSupportedFormats));
            ComTrace.WriteObject(formats);
            return formats.ToEnum<Formats>();
        }
       
        /// <summary>
        /// Call a service method
        /// </summary>
        /// <param name="method">Method GUID</param>
        /// <param name="parameters">Method parameters</param>
#pragma warning disable IDE0060 // Remove unused parameter
        // TODO: Complete service-method parameter/result mapping. Current implementation ignores object[] parameters, invokes with empty IPortableDeviceValues, and discards results.
        public void CallMethod(Guid method, object[] parameters)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            int err = DeviceService.Methods(out IPortableDeviceServiceMethods methods);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceService), nameof(IPortableDeviceService.Methods));

            IPortableDeviceValues values = ComFactory.CreateDeviceValues();
            //values.SetStringValue();
            IPortableDeviceValues results = ComFactory.CreateDeviceValues();
            err = methods.Invoke(ref method, ref values, ref results);
            MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceServiceMethods), nameof(IPortableDeviceServiceMethods.Invoke));
        }

        internal void SendCommand(PropertyKey commandKey)
        {
            IPortableDeviceValues values = ComFactory.CreateDeviceValues();
			int err = values.SetGuidValue(ref WPD.PROPERTY_COMMON_COMMAND_CATEGORY, ref commandKey.fmtid);
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetGuidValue), nameof(WPD.PROPERTY_COMMON_COMMAND_CATEGORY));
			err = values.SetUnsignedIntegerValue(ref WPD.PROPERTY_COMMON_COMMAND_ID, commandKey.pid);
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceValues), nameof(IPortableDeviceValues.SetUnsignedIntegerValue), nameof(WPD.PROPERTY_COMMON_COMMAND_ID));


#pragma warning disable IDE0059 // Unnecessary assignment of a value
			err = DeviceService.SendCommand(0, ref values, out IPortableDeviceValues results);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceService), nameof(IPortableDeviceService.SendCommand));
        }

        
    }
}
