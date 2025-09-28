using MediaDevices.Internal;
using System;
using MediaDevices;

namespace MediaDevices
{
    /// <summary>
    /// Event argument class for media device events
    /// </summary>
    public class MediaDeviceEventArgs : EventArgs
    {
        internal MediaDeviceEventArgs(Events eventEnum, MediaDevice mediaDevice, IPortableDeviceValues eventParameters)
        {
			ArgumentNullException.ThrowIfNull(eventEnum);
			ArgumentNullException.ThrowIfNull(mediaDevice);
			ArgumentNullException.ThrowIfNull(eventParameters);

			MediaDevice = mediaDevice;
			Event = eventEnum;

			PnpDeviceId = eventParameters.TryGetStringValue(WPD.EVENT_PARAMETER_PNP_DEVICE_ID, out string pnpDeviceId)
				? pnpDeviceId : string.Empty;

			OperationState = eventParameters.TryGetUnsignedIntegerValue(WPD.EVENT_PARAMETER_OPERATION_STATE, out uint operationState)
				? (OperationState)operationState : OperationState.Unspecified;

			OperationProgress = eventParameters.TryGetUnsignedIntegerValue(WPD.EVENT_PARAMETER_OPERATION_PROGRESS, out uint operationProgress)
				? operationProgress : 0;

			ObjectParentPersistanceUniqueId = eventParameters.TryGetStringValue(WPD.EVENT_PARAMETER_OBJECT_PARENT_PERSISTENT_UNIQUE_ID, out string objectParentPersistanceUniqueId)
				? objectParentPersistanceUniqueId : string.Empty;

			ObjectCreationCookie = eventParameters.TryGetStringValue(WPD.EVENT_PARAMETER_OBJECT_CREATION_COOKIE, out string objectCreationCookie)
				? objectCreationCookie : string.Empty;

			ChildHierarchyChanged = eventParameters.TryGetBoolValue(WPD.EVENT_PARAMETER_CHILD_HIERARCHY_CHANGED, out bool childHierarchyChanged)
				? childHierarchyChanged : false;

			ServiceMethodContext = eventParameters.TryGetStringValue(WPD.EVENT_PARAMETER_SERVICE_METHOD_CONTEXT, out string serviceMethodContext)
				? serviceMethodContext : string.Empty;
		}

        /// <summary>
        /// Corresponding media device
        /// </summary>
        public MediaDevice MediaDevice { get; private set; }

        /// <summary>
        /// Indicates the device that originated the event.
        /// </summary>
        public string PnpDeviceId { get; private set; }

        /// <summary>
        /// Indicates the event sent.
        /// </summary>
        public Events Event { get; private set; }

        /// <summary>
        /// Indicates the current state of the operation (e.g. started, running, stopped etc.).
        /// </summary>
        public OperationState OperationState { get; private set; }

        /// <summary>
        /// Indicates the progress of a currently executing operation. Value is from 0 to 100, with 100 indicating that the operation is complete.
        /// </summary>
        public uint OperationProgress { get; private set; }

        /// <summary>
        /// Uniquely identifies the parent object, similar to WPD_OBJECT_PARENT_ID, but this ID will not change between sessions.
        /// </summary>
        public string ObjectParentPersistanceUniqueId { get; private set; }

        /// <summary>
        /// This is the cookie handed back to a client when it requested an object creation using the IPortableDeviceContent::CreateObjectWithPropertiesAndData method.
        /// </summary>
        public string ObjectCreationCookie { get; private set; }

        /// <summary>
        /// Indicates that the child hierarchy for the object has changed.
        /// </summary>
        public bool ChildHierarchyChanged { get; private set; }

        /// <summary>
        /// Indicates the service method invocation context.
        /// </summary>
        public string ServiceMethodContext { get; private set; }
    }
}
