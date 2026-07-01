using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MediaDevices.Internal
{
    internal class StreamWrapper : Stream
    {
        private IStream? _stream;
        private IntPtr _pLength;
        private readonly ulong _size;
        private readonly bool _canRead;
        private readonly bool _canSeek;
        private readonly bool _canWrite;

		/// <summary>
		/// Wraps a COM <see cref="IStream"/> as a managed <see cref="Stream"/>.
		/// <para/>
		/// This implementation is designed to work with device-based streams
		/// such as MTP/WPD (e.g., Android, iPhone, portable devices),
		/// where the underlying COM stream may not fully respect the standard
		/// <see cref="Stream"/> contract.
		/// </summary>
		/// <param name="stream">
		/// The underlying COM <see cref="IStream"/> instance. Must not be null.
		/// </param>
		/// <param name="size">
		/// Optional size of the stream. Some WPD/MTP streams do not report length reliably,
		/// so this can be provided externally.
		/// </param>
		/// <param name="canWrite">
		/// Indicates whether the stream supports writing.
		/// <para/>
		/// If <c>null</c>, access mode is inferred from <see cref="IStream.Stat"/>.
		/// Due to unreliable driver implementations, explicitly specifying this value
		/// is recommended for device streams.
		/// </param>
		/// <param name="canSeek">
		/// Indicates whether the stream supports seeking.
		/// <para/>
		/// For MTP/WPD streams this should normally be set to <c>false</c>.
		/// These streams often report seek support but do not behave correctly
		/// (e.g. incorrect position, partial support, or runtime failures).
		/// <para/>
		/// If <c>null</c>, a best-effort probe is performed, but this is not reliable
		/// for device streams and may produce false positives.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="stream"/> is null.
		/// </exception>
		/// <remarks>
		/// This wrapper takes a defensive approach for device streams (MTP/WPD),
		/// where COM <see cref="IStream"/> implementations are often incomplete
		/// or inconsistent:
		/// <list type="bullet">
		/// <item><description>Seek may report success without working correctly</description></item>
		/// <item><description>Commit may throw even after successful writes</description></item>
		/// <item><description>Read/Write may ignore offsets</description></item>
		/// </list>
		/// 
		/// Because of this, callers are encouraged to explicitly specify capabilities
		/// rather than relying on auto-detection.
		/// </remarks>
		public StreamWrapper(IStream stream, ulong size = 0, bool? canWrite = false, bool? canSeek = false) {
	        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
	        _pLength = Marshal.AllocHGlobal(8);
	        _size = size;

	        // Conservative for MTP/WPD/IPhone streams.
	        if(null == canWrite)
	        {
				(_canRead, _canWrite) = GetAccessMode(stream);
			}
	        else
	        {
		        _canRead = true;
		        _canWrite = canWrite.Value; // Conservative for MTP/WPD
			}

	        // Conservative for MTP/WPD/IPhone streams.
	        // Do not probe Seek unless you really need it.
			if(null == canSeek) {
				_canSeek = GetCanSeek(stream);
			} else {
				//_canSeek = false;  // Conservative for MTP/WPD
				_canSeek = canSeek.Value;
			}
        }

        private void CheckDisposed()
        {
            if (_stream == null)
			{
				throw new ObjectDisposedException("StreamWrapper");
			}
        }

        private IStream Stream
        {
	        get
	        {
				CheckDisposed();
				return _stream!;
	        }
        }

        protected override void Dispose(bool disposing)
        {
	        if(_stream != null) {
		        try
		        {
			        Marshal.ReleaseComObject(_stream);
		        }
		        catch
		        {
					// Ignore exceptions during release, as we are disposing.
				} finally
		        {

			        _stream = null;
		        }
	        }
			if (_pLength != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_pLength);
                _pLength = IntPtr.Zero;
            }
            base.Dispose(disposing);
        }

        public override bool CanRead
        {
            get
            {
                return _canRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return _canSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _canWrite;
            }
        }

        public override void Flush()
        {
            CheckDisposed();

            if (!_canWrite)
            {
                return;
            }
			// Often MTP/WPD gives error, because not always supported.
			try
			{
				Stream.Commit(0);
			}
			catch (COMException ex) when (
				(uint)ex.HResult == 0x80004001 /* E_NOTIMPL */ || 
			    (uint)ex.HResult == 0x80070032 /* ERROR_NOT_SUPPORTED */)
			{
				// Ignore common WPD/MTP failures
				Trace.WriteLine($"Commit failed (ignored): {ex.Message}");
			}
		}

        /// Note: A value of 0 may indicate either an empty stream
        /// or that the size is unknown for device-based streams.
		public override long Length
        {
            get
            {
                CheckDisposed();
                return (long)_size;
            }
        }

        public override long Position {
	        get {
		        if(!_canSeek)
			        throw new NotSupportedException("Position is not supported for this stream.");

		        return Seek(0, SeekOrigin.Current);
	        }
	        set {
		        if(!_canSeek)
			        throw new NotSupportedException("Position is not supported for this stream.");

		        Seek(value, SeekOrigin.Begin);
	        }
        }

		public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            if (!_canRead)
            {
                throw new NotSupportedException("Read is not supported for this stream.");
            }

            ArgumentNullException.ThrowIfNull(buffer);

			ArgumentOutOfRangeException.ThrowIfNegative(count);
			ArgumentOutOfRangeException.ThrowIfNegative(offset);
            if (offset + count > buffer.Length)
            {
				throw new ArgumentOutOfRangeException(nameof(offset), "offset + count exceeds buffer length");
			}

            byte[] localBuffer = buffer;

            if (offset > 0)
            {
                localBuffer = new byte[count];
            }

            try
            {
                Stream.Read(localBuffer, count, _pLength);
                int bytesRead = Marshal.ReadInt32(_pLength);

                if (bytesRead < 0 || bytesRead > count)
                {
                    throw new IOException($"IStream returned invalid byte count: {bytesRead}");
                }

                if (offset > 0)
                {
                    Array.Copy(localBuffer, 0, buffer, offset, bytesRead);
                }

                return bytesRead;
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                throw new IOException("Error reading from IStream.", ex);
            }
        }

        private const int STREAM_SEEK_SET = 0;
        private const int STREAM_SEEK_CUR = 1;
        private const int STREAM_SEEK_END = 2;
        
        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();

            if (!_canSeek)
            {
                throw new NotSupportedException("Seek is not supported for this stream.");
            }

            int dwOrigin;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    dwOrigin = STREAM_SEEK_SET;   // STREAM_SEEK_SET
                    break;

                case SeekOrigin.Current:
                    dwOrigin = STREAM_SEEK_CUR;   // STREAM_SEEK_CUR
                    break;

                case SeekOrigin.End:
                    dwOrigin = STREAM_SEEK_END;   // STREAM_SEEK_END
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }

            Stream.Seek(offset, dwOrigin, _pLength);
            return Marshal.ReadInt64(_pLength);
        }

        public override void SetLength(long value)
        {
            CheckDisposed();

            if (!_canWrite)
            {
                throw new NotSupportedException("SetLength is not supported for this stream.");
            }

            Stream.SetSize(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            if (!_canWrite)
            {
                throw new NotSupportedException("Write is not supported for this stream.");
            }

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            byte[] localBuffer = buffer;

            if (offset > 0)
            {
                localBuffer = new byte[count]; 
                Array.Copy(buffer, offset, localBuffer, 0, count);
            }

            // workaround for Windows 10 Update 1703 problem 
            // https://social.msdn.microsoft.com/Forums/en-US/7f7a045d-9d9d-4ff4-b8e3-de2d7477a177/windows-10-update-1703-problem-with-wpd-and-mtp?forum=csharpgeneral
            Stream.Write(localBuffer, count, _pLength);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            try
            {
                int read = Read(buffer, offset, count);
                return Task.FromResult(read);
            }
            catch (Exception ex)
            {
                return Task.FromException<int>(ex);
            }
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromCanceled<int>(cancellationToken);
            }

            try
            {
                if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> segment))
                {
                    int read = Read(segment.Array!, segment.Offset, segment.Count);
                    return ValueTask.FromResult(read);
                }

                byte[] localBuffer = new byte[buffer.Length];
                int bytesRead = Read(localBuffer, 0, localBuffer.Length);
                localBuffer.AsMemory(0, bytesRead).CopyTo(buffer);
                return ValueTask.FromResult(bytesRead);
            }
            catch (Exception ex)
            {
                return ValueTask.FromException<int>(ex);
            }
        }

        // IStream constants — kept here for future access mode detection
        private const int STATFLAG_NONAME = 1;
        private const int STGM_READ = 0x00000000;
        private const int STGM_WRITE = 0x00000001;
        private const int STGM_READWRITE = 0x00000002;
        private const int STGM_ACCESS_MODE = 0x00000003;        
        
        // Detects read/write capabilities from IStream.grfMode.
		// Commented out — needs more testing on MTP devices.
		private static (bool CanRead, bool CanWrite) GetAccessMode(IStream stream) {
			try {
				stream.Stat(out STATSTG stat, STATFLAG_NONAME);

				int accessMode = stat.grfMode & STGM_ACCESS_MODE;

				bool canRead =
					accessMode == STGM_READ ||
					accessMode == STGM_READWRITE;

				bool canWrite =
					accessMode == STGM_WRITE ||
					accessMode == STGM_READWRITE;

				// Defensive fallback:
				// If grfMode contains something unexpected, prefer read-only.
				if(!canRead && !canWrite) {
					return (true, false);
				}

				return (canRead, canWrite);
			} catch {
				// Conservative fallback for MTP/device streams.
				return (true, false);
			}
		}

		// Probes whether seeking is supported on the stream.
		// Commented out — needs more testing on MTP devices.
		private static bool GetCanSeek(IStream stream) {
			IntPtr pPosition = Marshal.AllocHGlobal(8);

			try {
				stream.Seek(0, STREAM_SEEK_CUR, pPosition); //STREAM_SEEK_CUR
				return true;
			} catch {
				return false;
			} finally {
				Marshal.FreeHGlobal(pPosition);
			}
		}
	}
}
