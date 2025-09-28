using MediaDevices.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaDevices
{
    /// <summary>
    /// MediaDive connector
    /// </summary>
    public class MediaDeviceConnector : IConnectionRequestCallback, IDisposable
    {
        private IPortableDeviceConnector? _connector;
		private TaskCompletionSource<int>? _tcs;

		/// <summary>
		/// Event signals if complete
		/// </summary>
		[Obsolete("Use ConnectAsync instead", false)]
		public event EventHandler<CompleteEventArgs>? Complete;

        internal MediaDeviceConnector(IPortableDeviceConnector connector)
        {
            _connector = connector;
        }

		/// <summary>
		/// Connect to service
		/// </summary>
		[Obsolete("Use ConnectAsync instead", false)]
		public void Connect()
        {
            _connector?.Connect(this);
		}
		/// <summary>
		/// Connect to service
		/// </summary>
		public Task<int> ConnectAsync()
		{
			_tcs = new TaskCompletionSource<int>();
			_connector?.Connect(this);
			return _tcs.Task;
		}

		/// <summary>
		/// Disconnect from service
		/// </summary>
		public void Disconnect()
        {
            _connector?.Disconnect(this);
        }

        /// <inheritdoc/>
        public void Dispose() {
	        if(_connector != null) {
		        Marshal.ReleaseComObject(_connector);
		        _connector = null;
	        }
	        GC.SuppressFinalize(this);
        }

        ~MediaDeviceConnector() {
	        Dispose();
        }

		/// <summary>
		/// On completed
		/// </summary>
		/// <param name="hrStatus">Status</param>
		public void OnComplete([In, MarshalAs(UnmanagedType.Error)] int hrStatus)
        {
            this.Complete?.Invoke(this, new CompleteEventArgs(hrStatus));
			this._tcs?.SetResult(hrStatus);
		}
    }
}
