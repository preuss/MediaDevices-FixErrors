using System;
using System.Collections.Generic;
using System.Text;

namespace MediaDevices.Progress
{
    /// <summary>
    /// 
    /// </summary>
    public class FileProgressReport
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytesRead"></param>
        /// <param name="totalBytes"></param>
        /// <param name="startDateTime"></param>
        /// <param name="reportDateTime"></param>
        /// <param name="durationTimeSpan"></param>
        public FileProgressReport(ulong bytesRead, ulong totalBytes, DateTime startDateTime, DateTime reportDateTime, TimeSpan durationTimeSpan)
        {
            BytesRead = bytesRead;
            TotalBytes = totalBytes;
            StartDateTime = startDateTime;
            ReportDateTime = reportDateTime;
            DurationTimeSpan = durationTimeSpan;
        }
        /// <summary>
        /// 
        /// </summary>
        public ulong BytesRead { get; }
        /// <summary>
        /// 
        /// </summary>
        public ulong TotalBytes { get; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime StartDateTime { get; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime ReportDateTime { get; }
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan DurationTimeSpan { get; }

    }
}
