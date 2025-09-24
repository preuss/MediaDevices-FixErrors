using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Xml;

namespace MediaDevices.Internal
{
    internal class StreamWrapper : Stream
    {
        private IStream? _stream;
        private IntPtr _pLength;
        private readonly ulong _size;

        public StreamWrapper(IStream stream, ulong size = 0) {
	        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
	        _pLength = Marshal.AllocHGlobal(16);
	        _size = size;
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
            if (_stream != null)
            {
                Marshal.ReleaseComObject(_stream);
                _stream = null;
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
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                //return false;
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override void Flush()
        {
            Stream.Commit(0);
        }

        public override long Length
        {
            get
            {
                CheckDisposed();
                return (long)_size;
            }
        }

        public override long Position
        {
            get
            {
                return Seek(0, SeekOrigin.Current);
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
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

                if (offset > 0)
                {
                    Array.Copy(localBuffer, 0, buffer, offset, bytesRead);
                }

                return bytesRead;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                throw new IOException("Error reading from IStream.", ex);
            }
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();
            int dwOrigin;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    dwOrigin = 0;   // STREAM_SEEK_SET
                    break;

                case SeekOrigin.Current:
                    dwOrigin = 1;   // STREAM_SEEK_CUR
                    break;

                case SeekOrigin.End:
                    dwOrigin = 2;   // STREAM_SEEK_END
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }

            Stream.Seek(offset, dwOrigin, _pLength);
            return Marshal.ReadInt64(_pLength);

            //throw new NotImplementedException("Seek not implemented");
        }

        public override void SetLength(long value)
        {
            CheckDisposed();

            Stream.SetSize(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

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
    }
}
