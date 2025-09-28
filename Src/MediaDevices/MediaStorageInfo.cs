using System.IO;
using System.Runtime.CompilerServices;
using MediaDevices;

namespace MediaDevices
{
	/// <summary>
	/// Storage informations
	/// </summary>
	public class MediaStorageInfo
	{
		internal MediaStorageInfo(
			StorageType type,
			string fileSystemType,
			ulong capacity,
			ulong freeSpaceInBytes,
			ulong freeSpaceInObjects,
			string description,
			string serialNumber,
			ulong maxObjectSize,
			ulong capacityInObjects,
			StorageAccessCapability accessCapability)
		{
			Type = type;
			FileSystemType = fileSystemType;
			Capacity = capacity;
			FreeSpaceInBytes = freeSpaceInBytes;
			FreeSpaceInObjects = freeSpaceInObjects;
			Description = description;
			SerialNumber = serialNumber;
			MaxObjectSize = maxObjectSize;
			CapacityInObjects = capacityInObjects;
			AccessCapability = accessCapability;
		}

		/// <summary>
		/// Indicates the type of storage e.g. fixed, removable etc.
		/// </summary>
		public StorageType Type { get; }

		/// <summary>
		/// Indicates the file system type e.g. "FAT32" or "NTFS" or "My Special File System"
		/// </summary>
		public string FileSystemType { get; }

		/// <summary>
		/// Indicates the total storage capacity in bytes.
		/// </summary>
		public ulong Capacity { get; }

		/// <summary>
		/// Indicates the available space in bytes.
		/// </summary>
		public ulong FreeSpaceInBytes { get; }

		/// <summary>
		/// Indicates the available space in objects e.g. available slots on a SIM card.
		/// </summary>
		public ulong FreeSpaceInObjects { get; }

		/// <summary>
		/// Contains a description of the storage.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Contains the serial number of the storage.
		/// </summary>
		public string SerialNumber { get; }

		/// <summary>
		/// Specifies the maximum size of a single object (in bytes) that can be placed on this storage.
		/// </summary>
		public ulong MaxObjectSize { get; }

		/// <summary>
		/// Indicates the total storage capacity in objects e.g. available slots on a SIM card.
		/// </summary>
		public ulong CapacityInObjects { get; }

		/// <summary>
		/// This property identifies any write-protection that globally affects this storage. This takes precedence over access specified on individual objects.
		/// </summary>
		public StorageAccessCapability AccessCapability { get; }

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

		/// <summary>
		/// Convenience proxy to <see cref="GetDriveType"/>.
		/// </summary>
		public DriveType DriveType => GetDriveType();
	}
}