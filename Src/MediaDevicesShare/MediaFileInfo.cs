using MediaDevices.Internal;
using System.IO;
using MediaDevices.Progress;
using System;
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
		/// <param name="destFileName">The name of the new file to copy to.</param>
		/// <param name="overwrite">true to allow an existing file to be overwritten; otherwise, false. </param>
		/// <exception cref="System.IO.IOException">An error occurs, or the destination file already exists and overwrite is false. </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public void CopyTo(string destFileName, bool overwrite = true)
		{
			CopyToAsync(destFileName, overwrite).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Asynchronously copies an existing file to a new file, allowing the overwriting of the existing file.
		/// </summary>
		/// <param name="destFileName">The name of the new file to copy to.</param>
		/// <param name="overwrite">true to allow an existing file to be overwritten; otherwise, false. </param>
		public async Task CopyToAsync(string destFileName, bool overwrite = true)
		{
			if(!this._device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			using(FileStream fileStream = File.Open(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew))
			{
				using(Stream sourceStream = Item.OpenRead())
				{
					await sourceStream.CopyToAsync(fileStream);
				}
			}
		}

		/// <summary>
		/// Copies an existing file to a new file, allowing the overwriting of the existing file.
		/// </summary>
		/// <param name="destFileName">The name of the new file to copy to.</param>
		/// <param name="progress">The progress reporter.</param>
		/// <param name="overwrite">true to allow an existing file to be overwritten; otherwise, false. </param>
		/// <param name="readBufferSize">The buffer size, default is 8192 bytes.</param>
		/// <exception cref="System.IO.IOException">An error occurs, or the destination file already exists and overwrite is false. </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public void CopyTo(string destFileName, IProgress<FileProgressReport> progress, bool overwrite = true, int readBufferSize = 8192)
		{
			CopyToAsync(destFileName, progress, overwrite, readBufferSize).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Asynchronously copies an existing file to a new file, allowing the overwriting of the existing file.
		/// </summary>
		/// <param name="destFileName">The name of the new file to copy to.</param>
		/// <param name="progress">The progress reporter.</param>
		/// <param name="overwrite">true to allow an existing file to be overwritten; otherwise, false. </param>
		/// <param name="readBufferSize">The buffer size, default is 8192 bytes.</param>
		public async Task CopyToAsync(string destFileName, IProgress<FileProgressReport> progress, bool overwrite = true, int readBufferSize = 8192)
		{
			if(!this._device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			using(FileStream fileStream = File.Open(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew))
			{
				using(Stream sourceStream = Item.OpenRead())
				{
					DateTime startDateTime = System.DateTime.Now;

					byte[] buffer = new byte[readBufferSize];
					int bytesRead;
					ulong totalBytesRead = 0;

					DateTime reportDateTime;
					while((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
					{
						reportDateTime = System.DateTime.Now;
						await fileStream.WriteAsync(buffer, 0, bytesRead);
						totalBytesRead += (ulong)bytesRead;

						// Report progress
						progress.Report(new FileProgressReport(totalBytesRead, Item.Size, startDateTime, reportDateTime, reportDateTime.Subtract(startDateTime)));
					}
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
			if(!this._device.IsConnected)
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
			if(!this._device.IsConnected)
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
			if(!this._device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			return this.Item.OpenRead();
		}

		/// <summary>
		/// Creates a read-only FileStream of the icon.
		/// </summary>
		/// <returns>A new read-only FileStream object.</returns>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public Stream OpenIcon()
		{
			if(!this._device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			return this.Item.OpenReadIcon();
		}

		/// <summary>
		/// Creates a read-only FileStream of the thumbnail.
		/// </summary>
		/// <returns>A new read-only FileStream object.</returns>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public Stream OpenThumbnail()
		{
			if(!this._device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			return this.Item.OpenReadThumbnail();
		}
		/// <summary>
		/// Creates a StreamReader with UTF8 encoding that reads from an existing text file.
		/// </summary>
		/// <returns>A new StreamReader with UTF8 encoding.</returns>
		/// <exception cref="System.IO.DirectoryNotFoundException">path is invalid.</exception>
		/// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
		public StreamReader OpenText()
		{
			if(!this._device.IsConnected)
			{
				throw new NotConnectedException("Not connected");
			}
			return new StreamReader(this.Item.OpenRead());
		}
	}
}