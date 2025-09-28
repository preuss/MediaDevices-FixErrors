using System;
using System.Diagnostics;

namespace MediaDevices.Internal
{
	internal sealed class Profiler : IDisposable
    {
		private Stopwatch _stopwatch;
        private string _title;

		private bool _showTitle = false;

		// Set your preferred format here
		private static OutputFormat _format = OutputFormat.Milliseconds;
		public enum OutputFormat
		{
			Milliseconds,
			Nanoseconds,
			Detailed
		}

		public Profiler(string title)
        {
		#if !PROFILING
			_title = title;
			_stopwatch = new Stopwatch();
		#endif
			Start(title);
        }

        public void Dispose()
        {
            Stop();
        }

		[Conditional("PROFILING")]
		private void Start(string title)
        {
            _title = title;
			if(_showTitle)
			{
				Trace.WriteLine($"Profiler {_title} start");
			}
			_stopwatch = Stopwatch.StartNew();
        }

        [Conditional("PROFILING")]
        private void Stop()
        {
			_stopwatch.Stop();

			string time;
			switch(_format)
			{
				case OutputFormat.Milliseconds:
					double milliseconds = ((double)_stopwatch.ElapsedTicks / Stopwatch.Frequency) * 1000;
					time = $"{milliseconds} ms";
					break;
				case OutputFormat.Nanoseconds:
					double nanoseconds = ((double)_stopwatch.ElapsedTicks / Stopwatch.Frequency) * 1_000_000_000;
					time = $"{nanoseconds} ns";
					break;
				case OutputFormat.Detailed:
					string detailed = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fffffff").Insert(12, ".");
					time = detailed;
					break;
				default:
					double defaultMs = ((double)_stopwatch.ElapsedTicks / Stopwatch.Frequency) * 1000;
					time = $"{defaultMs} ms";
					break;
			}

			Trace.WriteLine($"Profiler {_title} : {time}");
		}
    }
}

