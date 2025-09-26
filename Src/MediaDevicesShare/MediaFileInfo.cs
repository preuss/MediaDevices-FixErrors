using MediaDevices.Internal;
using MediaDevices.Progress;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace MediaDevices
{
	/// <summary>
	/// Provides properties for files, directories and objects.
	/// </summary>
	public class MediaFileInfo : MediaFileSystemInfo
	{
		internal MediaFileInfo(MediaDevice device, Item item) : base(device, item)
		{ }

		/// <summary>
		/// Refreshes the state of the object.
		/// </summary>
		public override void Refresh()
		{
			base.Refresh();
		}

		/// <summary>
		/// Gets an instance of the parent directory.
		/// </summary>
		public MediaDirectoryInfo? Directory
		{
			get
			{
				return ParentDirectoryInfo;
			}
		}

		/// <summary>
		/// Copies an existing file to a new file, allowing the overwriting of the existing file.
		/// </summary>
		/// <param name="destinationFileName">The name of the new file to copy to.</param>
		/// <param name="overwriteExistingFile">true to allow an existing file to be overwritten; otherwise, false. </param>
		/// <param name="progressReporter">The progress reporter.</param>
		/// <param name="bufferSize">The buffer size, default is 8192 bytes.</param>
		/// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
		/// <exception cref="System.IO.IOException">An error occurs, or the destination file already exists and overwrite is false. </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public void CopyTo(string destinationFileName, bool overwriteExistingFile = true, IProgress<FileProgressReport>? progressReporter = null, int bufferSize = 8192, CancellationToken cancellationToken = default) {
			CopyToAsync(destinationFileName, overwriteExistingFile, progressReporter, bufferSize, cancellationToken).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Asynchronously copies an existing file to a new file, allowing the overwriting of the existing file.
		/// </summary>
		/// <param name="destinationFileName">The name of the new file to copy to.</param>
		/// <param name="overwriteExistingFile">true to allow an existing file to be overwritten; otherwise, false. </param>
		/// <param name="progressReporter">The progress reporter.</param>
		/// <param name="bufferSize">The buffer size, default is 20*4096 bytes (81920), that is just below the large object heap threshold (85K).</param>
		/// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
		public async Task CopyToAsync(string destinationFileName, bool overwriteExistingFile = true, IProgress<FileProgressReport>? progressReporter = null, int bufferSize = 20 * 4096, CancellationToken cancellationToken = default) {
			if(!_device.IsConnected) {
				throw new NotConnectedException("Not connected");
			}

			using(FileStream destinationFileStream = File.Open(destinationFileName, overwriteExistingFile ? FileMode.Create : FileMode.CreateNew)) {
				using(Stream sourceStream = Item.OpenRead()) {
					await CoreCopyAsync(sourceStream, destinationFileStream, Item.Size, progressReporter, bufferSize, cancellationToken);
				}
			}
		}

		/// <summary>
		/// Copies the contents of the file to a stream.
		/// </summary>
		/// <param name="destinationStream">The destination stream.</param>
		/// <param name="progressReporter">The progress reporter.</param>
		/// <param name="bufferSize">The buffer size.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public void CopyTo(Stream destinationStream, IProgress<FileProgressReport>? progressReporter = null, int bufferSize = 8192, CancellationToken cancellationToken = default) {
			CopyToAsync(destinationStream, progressReporter, bufferSize, cancellationToken).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Asynchronously copies the contents of the file to a stream.
		/// </summary>
		/// <param name="destinationStream">The destination stream.</param>
		/// <param name="progressReporter">The progress reporter.</param>
		/// <param name="bufferSize">The buffer size.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		public async Task CopyToAsync(Stream destinationStream, IProgress<FileProgressReport>? progressReporter = null, int bufferSize = 20 * 4096, CancellationToken cancellationToken = default) {
			if(!_device.IsConnected) {
				throw new NotConnectedException("Not connected");
			}

			using(Stream sourceStream = Item.OpenRead()) {
				await CoreCopyAsync(sourceStream, destinationStream, Item.Size, progressReporter, bufferSize, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously copies the contents from a source stream to a destination stream.
		/// </summary>
		/// <param name="sourceStream">The source stream to copy from.</param>
		/// <param name="destinationStream">The destination stream to copy to.</param>
		/// <param name="sourceSize">The total size of the source content.</param>
		/// <param name="progressReporter">The progress reporter.</param>
		/// <param name="bufferSize">The buffer size.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		private static async Task CoreCopyAsync(Stream sourceStream, Stream destinationStream, ulong sourceSize, IProgress<FileProgressReport>? progressReporter, int bufferSize, CancellationToken cancellationToken) 
		{
			if (progressReporter == null)
			{
				await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
				try
				{
					DateTime startDateTime = DateTime.Now;
					int bytesReadCount;
					ulong totalBytesRead = 0;
					DateTime reportDateTime;

					while ((bytesReadCount = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
						       .ConfigureAwait(false)) > 0)
					{
						await destinationStream.WriteAsync(buffer, 0, bytesReadCount, cancellationToken)
							.ConfigureAwait(false);

						totalBytesRead += (ulong)bytesReadCount;
						reportDateTime = DateTime.Now;
						progressReporter.Report(new FileProgressReport(totalBytesRead, sourceSize, startDateTime,
							reportDateTime, reportDateTime.Subtract(startDateTime)));
					}
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(buffer);
				}
			}
		}

		/// <summary>
		/// Copies an icon of an existing file to a new file, allowing the overwriting of the existing file.
		/// </summary>
		/// <param name="destFileName">The name of the new file to copy to.</param>
		/// <param name="overwrite">true to allow an existing file to be overwritten; otherwise, false. </param>
		/// <exception cref="System.IO.IOException">An error occurs, or the destination file already exists and overwrite is false. </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public void CopyIconTo(string destFileName, bool overwrite = true)
		{
			if(!_device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			using(FileStream file = File.Open(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew))
			{
				using(Stream sourceStream = Item.OpenReadIcon())
				{
					sourceStream.CopyTo(file);
				}
			}
		}

		/// <summary>
		/// Copies an thumbnail of an existing file to a new file, allowing the overwriting of the existing file.
		/// </summary>
		/// <param name="destFileName">The name of the new file to copy to.</param>
		/// <param name="overwrite">true to allow an existing file to be overwritten; otherwise, false. </param>
		/// <exception cref="System.IO.IOException">An error occurs, or the destination file already exists and overwrite is false. </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public void CopyThumbnail(string destFileName, bool overwrite = true)
		{
			if(!_device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			using(FileStream file = File.Open(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew))
			{
				using(Stream sourceStream = Item.OpenReadThumbnail())
				{
					sourceStream.CopyTo(file);
				}
			}
		}

		/// <summary>
		/// Creates a read-only FileStream.
		/// </summary>
		/// <returns>A new read-only FileStream object.</returns>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public Stream OpenRead()
		{
			if(!_device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			return Item.OpenRead();
		}

		/// <summary>
		/// Creates a read-only FileStream of the icon.
		/// </summary>
		/// <returns>A new read-only FileStream object.</returns>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public Stream OpenIcon()
		{
			if(!_device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			return Item.OpenReadIcon();
		}

		/// <summary>
		/// Creates a read-only FileStream of the thumbnail.
		/// </summary>
		/// <returns>A new read-only FileStream object.</returns>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public Stream OpenThumbnail()
		{
			if(!_device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			return Item.OpenReadThumbnail();
		}
		/// <summary>
		/// Creates a StreamReader with UTF8 encoding that reads from an existing text file.
		/// </summary>
		/// <returns>A new StreamReader with UTF8 encoding.</returns>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public StreamReader OpenText()
		{
			if(!_device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			return new StreamReader(Item.OpenRead());
		}
	}
}