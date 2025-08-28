using System.IO;
using System.Runtime.CompilerServices;

namespace MediaDevices
{
	/// <summary>
	/// Storage informations
	/// </summary>
	public class MediaStorageInfo
	{
		internal MediaStorageInfo()
		{
		}

		/// <summary>
		/// Indicates the type of storage e.g. fixed, removable etc.
		/// </summary>
		public StorageType Type { get; internal set; }

		/// <summary>
		/// Indicates the file system type e.g. "FAT32" or "NTFS" or "My Special File System"
		/// </summary>
		public string FileSystemType { get; internal set; }

		/// <summary>
		/// Indicates the total storage capacity in bytes.
		/// </summary>
		public ulong Capacity { get; internal set; }

		/// <summary>
		/// Indicates the available space in bytes.
		/// </summary>
		public ulong FreeSpaceInBytes { get; internal set; }

		/// <summary>
		/// Indicates the available space in objects e.g. available slots on a SIM card.
		/// </summary>
		public ulong FreeSpaceInObjects { get; internal set; }

		/// <summary>
		/// Contains a description of the storage.
		/// </summary>
		public string Description { get; internal set; }

		/// <summary>
		/// Contains the serial number of the storage.
		/// </summary>
		public string SerialNumber { get; internal set; }

		/// <summary>
		/// Specifies the maximum size of a single object (in bytes) that can be placed on this storage.
		/// </summary>
		public ulong MaxObjectSize { get; internal set; }

		/// <summary>
		/// Indicates the total storage capacity in objects e.g. available slots on a SIM card.
		/// </summary>
		public ulong CapacityInObjects { get; internal set; }

		/// <summary>
		/// This property identifies any write-protection that globally affects this storage. This takes precedence over access specified on individual objects.
		/// </summary>
		public StorageAccessCapability AccessCapability { get; internal set; }

		/// <summary>
		/// Maps <see cref="StorageType"/> to <see cref="DriveType"/>.
		/// </summary>
		public DriveType GetDriveType()
		{
			switch (Type)
			{
				case StorageType.FixedRam:
				case StorageType.FixedRom:
					return DriveType.Fixed;

				case StorageType.RemovableRam:
				case StorageType.RemovableRom:
					return DriveType.Removable;

				case StorageType.Undefined:
				default:
					return DriveType.Unknown;
			}
		}
	}
}