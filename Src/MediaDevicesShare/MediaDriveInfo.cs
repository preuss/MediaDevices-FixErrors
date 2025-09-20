using System;
using MediaDevices.Internal;
using System.IO;

namespace MediaDevices
{
    /// <summary>
    /// Provides properties for drives.
    /// </summary>
    public sealed class MediaDriveInfo
    {
        private readonly MediaDevice _device;
        private readonly string _objectId;

        internal MediaDriveInfo(MediaDevice device, string objectId)
        {
            this._device = device;
            this._objectId = objectId;

            Initialize();
        }
        private void Initialize()
        {
            MediaStorageInfo? info = _device.GetStorageInfo(_objectId);
            if (info == null) return;

            IsReady = true;

            TotalSize = Convert.ToInt64(info.Capacity);
            TotalFreeSpace = AvailableFreeSpace = Convert.ToInt64(info.FreeSpaceInBytes);

            DriveFormat = info.FileSystemType;
            DriveType = info.GetDriveType();
            
            RootDirectory = new MediaDirectoryInfo(_device, Item.Create(_device, _objectId));
            Name = RootDirectory.FullName;
            VolumeLabel = info.Description;
        }

        /// <summary>
        /// Indicates the available space in bytes.
        /// </summary>
        public long AvailableFreeSpace { get; private set; }

        /// <summary>
        /// Format of the drive.
        /// </summary>
        public string DriveFormat { get; private set; } = string.Empty;

        /// <summary>
        /// Type of the drive
        /// </summary>
        public DriveType DriveType { get; private set; }

        /// <summary>
        /// True is the drive is ready; false if not.
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Name of the drive
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Get the root directory of the drive.
        /// </summary>
        public MediaDirectoryInfo? RootDirectory { get; private set; }

        /// <summary>
        /// Gets the total free space of the device in bytes.
        /// </summary>
        public long TotalFreeSpace { get; private set; }

        /// <summary>
        /// Gets the total size of the device in bytes.
        /// </summary>
        public long TotalSize { get; private set; }

        /// <summary>
        /// Get the volume label of the drive.
        /// </summary>
        public string VolumeLabel { get; private set; } = string.Empty;

        /// <summary>
        /// Eject the drive.
        /// </summary>
        public void Eject() => _device.InternalEject(_objectId);

        /// <summary>
        /// Format the drive.
        /// </summary>
        public void Format() => _device.Format(_objectId);
    }
}
