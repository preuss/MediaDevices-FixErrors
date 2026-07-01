using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaDevices.Progress;

public static class StreamExtensions {
	/// <summary>
	/// Asynchronously copies the contents from a source stream to a destination stream with optional progress reporting.
	/// </summary>
	/// <param name="sourceStream">The source stream to copy from.</param>
	/// <param name="destinationStream">The destination stream to copy to.</param>
	/// <param name="sourceSize">The total size of the source content.</param>
	/// <param name="progressReporter">The optional progress reporter.</param>
	/// <param name="bufferSize">The buffer size.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	public static async Task CopyToStreamAsync(this Stream sourceStream, Stream destinationStream, ulong sourceSize, IProgress<FileProgressReport>? progressReporter, int bufferSize, CancellationToken cancellationToken) {
		ArgumentNullException.ThrowIfNull(sourceStream);
		ArgumentNullException.ThrowIfNull(destinationStream);

		//ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
		if(bufferSize <= 0) {
			throw new ArgumentOutOfRangeException(nameof(bufferSize));
		}

		if(progressReporter == null) {
			await sourceStream
				.CopyToAsync(destinationStream, bufferSize, cancellationToken)
				.ConfigureAwait(false);

			return;
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

		try {
			DateTime startDateTime = DateTime.Now;
			ulong totalBytesRead = 0;

			progressReporter.Report(new FileProgressReport(
				totalBytesRead,
				sourceSize,
				startDateTime,
				startDateTime,
				TimeSpan.Zero));

			int bytesReadCount;

			while((bytesReadCount = await sourceStream
					  .ReadAsync(buffer, 0, buffer.Length, cancellationToken)
					  .ConfigureAwait(false)) > 0) {
				await destinationStream
					.WriteAsync(buffer, 0, bytesReadCount, cancellationToken)
					.ConfigureAwait(false);

				totalBytesRead += (ulong)bytesReadCount;

				DateTime reportDateTime = DateTime.UtcNow;

				progressReporter.Report(new FileProgressReport(
					totalBytesRead,
					sourceSize,
					startDateTime,
					reportDateTime,
					reportDateTime.Subtract(startDateTime)));
			}
		} finally {
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
}