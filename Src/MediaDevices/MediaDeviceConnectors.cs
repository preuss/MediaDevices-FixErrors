using MediaDevices.Internal;
using System.Collections.Generic;
using System.Linq;

namespace MediaDevices
{
    /// <summary>
    /// MediaDevice connectors
    /// </summary>
    public static class MediaDeviceConnectors
    {

        #region static

        private static IEnumPortableDeviceConnectors _connectors;

        static MediaDeviceConnectors()
        {
            _connectors = ComFactory.CreateDeviceConnectors();
        }

        #endregion

        /// <summary>
        /// Get connextors.
        /// </summary>
        /// <returns>List of connectors</returns>
        public static IEnumerable<MediaDeviceConnector> Connectors()
        {

            //connectors.Reset();

            IPortableDeviceConnector connector;
            uint num = 1;
            _connectors.Next(1, out connector, ref num);

            return new List<MediaDeviceConnector>() { new MediaDeviceConnector(connector) };

            //IPortableDeviceConnector[] connectorArray = new IPortableDeviceConnector[10]; 
            //uint num = 10;
            //connectors.Next(10, ref connectorArray, ref num);


            //connectors.Clone(out IEnumPortableDeviceConnectors test);

            //connectorArray = new IPortableDeviceConnector[10];
            //num = 10;
            //test.Next(10, ref connectorArray, ref num);

            //return connectorArray?.Select(c => new MediaDeviceConnector(c));
        }
    }
}
