using MediaDevices.Internal;

namespace MediaDevices.Internal
{
    internal class EventCallback : IPortableDeviceEventCallback
    {
        private MediaDevice _device;

        public EventCallback(MediaDevice device)
        {
            _device = device;
        }

        public void OnEvent(IPortableDeviceValues pEventParameters)
        {
            _device.CallEvent(pEventParameters);
        }
    }
}
