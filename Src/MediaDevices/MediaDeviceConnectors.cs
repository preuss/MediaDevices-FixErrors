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
        /// Get connectors.
        /// </summary>
        /// <returns>List of connectors</returns>
        public static IEnumerable<MediaDeviceConnector> Connectors()
        {
            var result = new List<MediaDeviceConnector>();
            IPortableDeviceConnector connector;
            uint num = 1;

            while (num > 0)
            {
                num = 1;
                _connectors.Next(1, out connector, ref num);
                if (num > 0)
                {
                    result.Add(new MediaDeviceConnector(connector));
                }
            }

            return result;
        }
    }
}
