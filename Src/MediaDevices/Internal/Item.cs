using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaDevices.Internal
{

	[DebuggerDisplay("{this.Type} - {this.Name} - {this.Id}")]
	internal class Item
	{
		private static readonly IPortableDeviceKeyCollection _keyCollection;

		static Item()
		{
			// key collection with all used properties
			_keyCollection = ComFactory.CreateDeviceKeyCollection();
			_keyCollection.Add(ref WPD.OBJECT_CONTENT_TYPE);
			_keyCollection.Add(ref WPD.OBJECT_NAME);
			_keyCollection.Add(ref WPD.OBJECT_ORIGINAL_FILE_NAME);

			_keyCollection.Add(ref WPD.OBJECT_HINT_LOCATION_DISPLAY_NAME);
			_keyCollection.Add(ref WPD.OBJECT_CONTAINER_FUNCTIONAL_OBJECT_ID);
			_keyCollection.Add(ref WPD.OBJECT_SIZE);
			_keyCollection.Add(ref WPD.OBJECT_DATE_CREATED);
			_keyCollection.Add(ref WPD.OBJECT_DATE_MODIFIED);
			_keyCollection.Add(ref WPD.OBJECT_DATE_AUTHORED);
			_keyCollection.Add(ref WPD.OBJECT_CAN_DELETE);
			_keyCollection.Add(ref WPD.OBJECT_ISSYSTEM);
			_keyCollection.Add(ref WPD.OBJECT_ISHIDDEN);
			_keyCollection.Add(ref WPD.OBJECT_IS_DRM_PROTECTED);
			_keyCollection.Add(ref WPD.OBJECT_PARENT_ID);
			_keyCollection.Add(ref WPD.OBJECT_PERSISTENT_UNIQUE_ID);
		}

		private readonly MediaDevice _device;
		private string? _name;
		private readonly string? _path;
		private Item? _parent;

		private const uint PORTABLE_DEVICE_DELETE_NO_RECURSION = 0;
		private const uint PORTABLE_DEVICE_DELETE_WITH_RECURSION = 1;

		private const char DIRECTORY_SEPARATOR_CHAR = '\\';

		private const int NUM_OBJECTS_TO_REQUEST = 32;

		public const string RootId = "DEVICE";

		public static Item GetRoot(MediaDevice device)
		{
			return new Item(device, RootId, @"\");
		}

		public static Item Create(MediaDevice device, string id, string? path = null)
		{
			return new Item(device, id, path);
		}

		public static Item? FindFolder(MediaDevice device, string path)
		{
			var item = FindItem(device, path);
			return item == null || item.Type == ItemType.File ? null : item;
		}

		public static Item? FindFile(MediaDevice device, string path)
		{
			var item = FindItem(device, path);
			return item == null || item.Type != ItemType.File ? null : item;
		}

		public static Item? FindItem(MediaDevice device, string path)
		{
			ArgumentNullException.ThrowIfNull(path);

			var item = Item.GetRoot(device);
			if (path == @"\")
			{
				return item;
			}
			var folders = path.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var folder in folders)
			{
				item = item.GetChildren().FirstOrDefault(i => device.EqualsName(i.Name, folder));
				if (item == null)
				{
					return null;
				}
			}
			return item;
		}

		public static Item? GetFromPersistentUniqueId(MediaDevice device, string persistentUniqueId)
		{
			// fill collection with id to request
			var collection = ComFactory.CreateDevicePropVariantCollection();

			using (var propVariantPUID = PropVariantFacade.StringToPropVariant(persistentUniqueId))
			{
				collection.Add(ref propVariantPUID.Value);
			}
			// request id collection           
			int err = device.deviceContent.GetObjectIDsFromPersistentUniqueIDs(collection, out IPortableDevicePropVariantCollection results);
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.GetObjectIDsFromPersistentUniqueIDs), device.Description);

			//var s = results.ToStrings().ToArray();
			string? mediaObjectId = results.ToStrings().FirstOrDefault();

			// return result item
			return mediaObjectId == null ? null : Item.Create(device, mediaObjectId);
			//return string.IsNullOrEmpty(mediaObjectId) ? null : Item.Create(device, mediaObjectId);
		}

		private Item(MediaDevice device, string id, string? path)
		{
			_device = device;
			Id = id;
			_path = path;

			if (id == Item.RootId)
			{
				Name = @"\";
				FullName = @"\";
				Type = ItemType.Object;
			}
			else
			{
				Refresh();

				// find full name if no path
				if (string.IsNullOrEmpty(path))
				{
					string p = GetPath();
					_path = Path.GetDirectoryName(p);
					FullName = p;
				}
			}
		}

		/// <summary>
		/// Special small constructor for GetPath.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="id"></param>
		private Item(MediaDevice device, string id)
		{
			_device = device;
			Id = id;
			if (id == Item.RootId)
			{
				Name = @"\";
				FullName = @"\";
				Type = ItemType.Object;
			}
			else
			{
				Refresh();
			}
		}

		public void Refresh()
		{
			if (Id != Item.RootId)
			{
				GetProperties();

				Guid contentType = ContentType;
				if (contentType == WPD.CONTENT_TYPE_FUNCTIONAL_OBJECT)
				{
					Name = _name;
					Type = ItemType.Object;

				}
				else if (contentType == WPD.CONTENT_TYPE_FOLDER)
				{
					Name = OriginalFileName;
					Type = ItemType.Folder;
				}
				else
				{
					Name = OriginalFileName;
					Type = ItemType.File;
				}
				if (_path != null) // TODO check if we can remove empty paths
				{
					// TODO: build full name, but should we use OriginalFileName as fallback?
					string? usingName = Name;
					if (string.IsNullOrWhiteSpace(usingName))
					{
						usingName = OriginalFileName;
					}
					if (string.IsNullOrWhiteSpace(usingName))
					{
						// TODO: Should we throw exception here, or append something to show error?
						FullName = _path;
					}
					else
					{
						// don't use Path.Combine
						FullName = _path.TrimEnd(DIRECTORY_SEPARATOR_CHAR) + DIRECTORY_SEPARATOR_CHAR + usingName;
					}
				}
			}
		}

		private void GetProperties()
		{
			IPortableDeviceValues values;
			try
			{
				// get all predefined values
				_device.deviceProperties.GetValues(Id, _keyCollection, out values);
			}
			catch (COMException ex) when (ex.HResult == (int)ErrorCodes.InvalidParameter)
			{
				// Some devices (e.g. Amazon Kindle Paperwhite) do not support GetValues
				// with a keyCollection. Retry with null to get all values.
				try
				{
					_device.deviceProperties.GetValues(Id, null, out values);
				}
				catch (Exception inner)
				{
					throw new IOException(
						$"Could not read properties for device '{_device.FriendlyName}', Id='{Id}'.", inner);
				}
			}
			catch (Exception ex)
			{
				throw new IOException(
					$"Could not read properties for device '{_device.FriendlyName}', Id='{Id}'.", ex);
			}

			// read all properties
			// use a loop to prevent exceptions during calling GetValue for non-existing values 
			uint num = 0;
			values.GetCount(ref num);
			for (uint i = 0; i < num; i++)
			{
				PropertyKey key = new PropertyKey();
				using (PropVariantFacade val = new PropVariantFacade())
				{
					values.GetAt(i, ref key, ref val.Value);

					if (key.fmtid == WPD.OBJECT_PROPERTIES_V1)
					{
						switch ((ObjectProperties)key.pid)
						{
							case ObjectProperties.ContentType:
								ContentType = val;
								break;

							case ObjectProperties.Name:
								_name = val;
								break;

							case ObjectProperties.OriginalFileName:
								OriginalFileName = val;
								break;

							case ObjectProperties.HintLocationDisplayName:
								HintLocationName = val;
								break;

							case ObjectProperties.ContainerFunctionalObjectId:
								ParentContainerId = val;
								break;

							case ObjectProperties.Size:
								Size = val;
								break;

							case ObjectProperties.DateCreated:
								DateCreated = val;
								break;

							case ObjectProperties.DateModified:
								DateModified = val;
								break;

							case ObjectProperties.DateAuthored:
								DateAuthored = val;
								break;

							case ObjectProperties.CanDelete:
								CanDelete = val;
								break;

							case ObjectProperties.IsSystem:
								IsSystem = val.ToBool();
								break;

							case ObjectProperties.IsHidden:
								IsHidden = val;
								break;

							case ObjectProperties.IsDrmProtected:
								IsDRMProtected = val;
								break;

							case ObjectProperties.ParentId:
								ParentId = val;
								break;

							case ObjectProperties.PersistentUniqueId:
								PersistentUniqueId = val;
								break;
						}
					}
				}
			}
		}

		#region Value Properties

		public string Id { get; private set; }
		public string? Name { get; private set; }
		public string FullName { get; set; }
		public ItemType Type { get; private set; }
		public Guid ContentType { get; private set; }
		public string OriginalFileName { get; private set; }
		public string HintLocationName { get; private set; }
		public string ParentContainerId { get; private set; }
		public ulong Size { get; private set; }
		public DateTime? DateCreated { get; private set; }
		public DateTime? DateModified { get; private set; }
		public DateTime? DateAuthored { get; private set; }
		public bool CanDelete { get; private set; }
		public bool IsSystem { get; private set; }
		public bool IsHidden { get; private set; }
		public bool IsDRMProtected { get; private set; }
		public string ParentId { get; private set; }
		public string PersistentUniqueId { get; private set; }

		public bool IsRoot { get { return Id == RootId; } }

		public bool IsFile { get { return Type == ItemType.File; } }

		public Item? Parent
		{
			get
			{
				// Parent resolution may create a new Item and read its properties from the device.
				// Cache it per Item instance so repeated path/parent access does not resolve
				// the same parent chain again for this object.
				//
				// This is intentionally an instance-local cache. The same device object may
				// still be resolved again if represented by another Item instance.
				if (_parent == null)
				{
					_parent = ResolveParentForPath(this);
				}

				return _parent;
			}
		}

		#endregion

		#region Methods

		public IEnumerable<Item> GetChildren()
		{
			int err = _device.deviceContent.EnumObjects(0, Id, null, out IEnumPortableDeviceObjectIDs enumerator);
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.EnumObjects), _device.Description);

			try
			{
				uint fetched = 0;
				var objectIds = new string[NUM_OBJECTS_TO_REQUEST];
				enumerator.Next(NUM_OBJECTS_TO_REQUEST, objectIds, ref fetched);
				while (fetched > 0)
				{
					for (int index = 0; index < fetched; index++)
					{
						Item? item = null;

						try
						{
							item = Item.Create(_device, objectIds[index], FullName);
						}
						catch (FileNotFoundException)
						{
							// handle system files, that cannot be opened or read.
							// Windows sometimes creates a fake files in e.g. System Volume Information.
							// Let's handle such situations.
						}

						if (item != null)
						{
							yield return item;
						}
					}
					enumerator.Next(NUM_OBJECTS_TO_REQUEST, objectIds, ref fetched);
				}
			}
			finally
			{
				if (enumerator != null)
				{
					Marshal.ReleaseComObject(enumerator);
				}
			}
		}

		public IEnumerable<Item> GetChildren(string? pattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			int err = _device.deviceContent.EnumObjects(0, Id, null, out IEnumPortableDeviceObjectIDs enumerator);
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.EnumObjects), _device.Description);

			try
			{
				uint fetched = 0;
				var objectIds = new string[NUM_OBJECTS_TO_REQUEST];
				enumerator.Next(NUM_OBJECTS_TO_REQUEST, objectIds, ref fetched);
				while (fetched > 0)
				{
					for (int index = 0; index < fetched; index++)
					{
						Item? item = null;

						try
						{
							item = Item.Create(_device, objectIds[index], FullName);
						}
						catch (FileNotFoundException)
						{
							// handle system files, that cannot be opened or read.
							// Windows sometimes creates a fake files in e.g.System Volume Information.
							// Let's handle such situations.
						}

						if (item != null)
						{
							if (pattern == null || (item.Name != null && Regex.IsMatch(item.Name, pattern, RegexOptions.IgnoreCase)))
							{
								yield return item;
							}

							if (searchOption == SearchOption.AllDirectories && item.Type != ItemType.File)
							{
								var children = item.GetChildren(pattern, searchOption);
								foreach (var c in children)
								{
									yield return c;
								}
							}
						}
					}
					Array.Clear(objectIds, 0, objectIds.Length);
					enumerator.Next(NUM_OBJECTS_TO_REQUEST, objectIds, ref fetched);
				}
			}
			finally
			{
				if (enumerator != null)
				{
					Marshal.ReleaseComObject(enumerator);
				}
			}
		}

		internal Item? CreateSubdirectory(string path)
		{
			return CreateSubdirectory(path, DateTime.Now, DateTime.Now, DateTime.Now);
		}

		internal Item? CreateSubdirectory(string path, DateTime dateCreated, DateTime dateModified, DateTime dateAuthored)
		{
			Item? child = null;
			Item parent = this;
			var folders = path.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string folder in folders)
			{
				child = parent.GetChildren().FirstOrDefault(i => _device.EqualsName(i.Name, folder));
				if (child == null)
				{
					// create a new directory
					IPortableDeviceValues deviceValues = ComFactory.CreateDeviceValues();
					deviceValues.SetStringValue(ref WPD.OBJECT_PARENT_ID, parent.Id);
					deviceValues.SetStringValue(ref WPD.OBJECT_NAME, folder);
					deviceValues.SetStringValue(ref WPD.OBJECT_ORIGINAL_FILE_NAME, folder);
					deviceValues.SetGuidValue(ref WPD.OBJECT_CONTENT_TYPE, ref WPD.CONTENT_TYPE_FOLDER);

					using (PropVariantFacade created = PropVariantFacade.DateTimeToPropVariant(dateCreated))
					using (PropVariantFacade modified = PropVariantFacade.DateTimeToPropVariant(dateModified))
					using (PropVariantFacade authored = PropVariantFacade.DateTimeToPropVariant(dateAuthored))
					{
						deviceValues.SetValue(ref WPD.OBJECT_DATE_CREATED, ref created.Value);
						deviceValues.SetValue(ref WPD.OBJECT_DATE_MODIFIED, ref modified.Value);
						deviceValues.SetValue(ref WPD.OBJECT_DATE_AUTHORED, ref authored.Value);
					}

					string id = string.Empty;
					try
					{
						int errCreate = _device.deviceContent.CreateObjectWithPropertiesOnly(deviceValues, ref id);
						MediaDeviceException.ThrowIfComError(errCreate, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.CreateObjectWithPropertiesOnly), _device.Description);
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
						return null;
					}
					child = Item.Create(_device, id, parent.FullName);
				}
				else if (child.Type == ItemType.File)
				{
					// folder is already a file
					throw new InvalidOperationException($"A path of the path {folder} is a file");
				}
				else
				{
					// folder exists
					//id = child.Id;
					//new Item()

					// TODO
				}
				parent = child;
			}
			return child;
		}

		public void Delete(bool recursive = false)
		{
			var objectIdCollection = ComFactory.CreateDevicePropVariantCollection();

			using (PropVariantFacade propVariantValue = PropVariantFacade.StringToPropVariant(Id))
			{
				objectIdCollection.Add(ref propVariantValue.Value);
			}

			IPortableDevicePropVariantCollection results = ComFactory.CreateDevicePropVariantCollection();

			int hr = _device.deviceContent.Delete(recursive ? PORTABLE_DEVICE_DELETE_WITH_RECURSION : PORTABLE_DEVICE_DELETE_NO_RECURSION, objectIdCollection, ref results);

			// The device refused the delete. (Ex. Trying to Delete a "protected" file)
			if (hr == (int)ErrorCodes.False)
			{
				throw new IOException($"Failed to delete {Name}!");
			}

			MediaDeviceException.ThrowIfComError(hr, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.Delete), _device.Description);

			ComTrace.WriteObject(objectIdCollection);
		}

		public string GetPath()
		{
			if (IsRoot)
			{
				return @"\";
			}

			var parts = new Stack<string>();
			var visited = new HashSet<string>(StringComparer.Ordinal);

			Item? current = this;

			while (current != null && !current.IsRoot)
			{
				// WPD object graphs are provided by device drivers and are not always reliable.
				// A broken device/driver may expose a cyclic parent chain. Without this guard,
				// path resolution could loop forever.
				if (!visited.Add(current.Id))
				{
					throw new InvalidOperationException(
						$"Cycle detected while resolving path on device '{_device.FriendlyName}', Id='{current.Id}'.");
				}

				// For folders/files we normally use Name. Some devices expose a weak or empty
				// WPD_OBJECT_NAME but still provide WPD_OBJECT_ORIGINAL_FILE_NAME.
				// Use OriginalFileName as a fallback before failing.
				string? name = current.Name;

				if (string.IsNullOrWhiteSpace(name))
				{
					name = current.OriginalFileName;
				}

				if (string.IsNullOrWhiteSpace(name))
				{
					throw new InvalidOperationException(
						$"Object has no usable name while resolving path on device '{_device.FriendlyName}', Id='{current.Id}'.");
				}

				parts.Push(name);

				// Parent resolution is intentionally centralized in Parent/ResolveParentForPath.
				// Normal Android/generic MTP devices use ParentId.
				// Apple/DCF-like devices may require fallback through ParentContainerId.
				current = current.Parent;
			}

			if (current == null)
			{
				throw new InvalidOperationException(
					$"Could not resolve full object path on device '{_device.FriendlyName}', Id='{Id}'.");
			}

			return DIRECTORY_SEPARATOR_CHAR + string.Join(DIRECTORY_SEPARATOR_CHAR.ToString(), parts);
		}

		/// <summary>
		/// Resolves the logical parent used for path construction.
		/// </summary>
		/// <remarks>
		/// Most MTP devices expose a normal hierarchical object graph where each object
		/// has a WPD_OBJECT_PARENT_ID. Android devices usually behave like this.
		///
		/// Some devices, especially Apple devices exposing DCF-style storage, may not
		/// expose a usable ParentId for top-level folders. In those cases the object may
		/// instead expose WPD_OBJECT_CONTAINER_FUNCTIONAL_OBJECT_ID, which points to the
		/// closest functional object, typically the storage root.
		///
		/// This method keeps both behaviours in one place:
		/// 1. Use ParentId for normal hierarchical devices.
		/// 2. Treat functional storage objects as direct children of DEVICE.
		/// 3. Fall back to ParentContainerId for non-hierarchical / DCF-like devices.
		/// </remarks>
		private Item? ResolveParentForPath(Item item)
		{
			// Case 1:
			// Normal hierarchical MTP.
			//
			// Android and most generic MTP devices expose a usable ParentId.
			// In that case we can walk the object tree directly.
			if (!string.IsNullOrWhiteSpace(item.ParentId))
			{
				return new Item(_device, item.ParentId);
			}

			// Case 2:
			// The current object is itself a functional object, usually a storage root.
			//
			// Functional storage objects are conceptually direct children of DEVICE.
			// They may not have a normal parent folder, so terminate the path at root.
			if (item.ContentType == WPD.CONTENT_TYPE_FUNCTIONAL_OBJECT)
			{
				return GetRoot(_device);
			}

			// Case 3:
			// Non-hierarchical / DCF-like storage fallback.
			//
			// Some devices, notably Apple devices, may expose top-level folders where
			// ParentId is empty or not useful. For those objects, ParentContainerId may
			// point to the storage functional object that logically contains them.
			//
			// Example:
			// Generic hierarchical storage:
			//   file/folder.ParentId -> parent folder/storage id
			//
			// DCF-like storage:
			//   topFolder.ParentId may be empty or object-like
			//   topFolder.ParentContainerId -> storage root id
			if (!string.IsNullOrWhiteSpace(item.ParentContainerId))
			{
				var storageRoot = _device
					.GetDrives()
					.Select(d => d.RootDirectory?.Item)
					.FirstOrDefault(root =>
						root != null &&
						string.Equals(root.Id, item.ParentContainerId, StringComparison.Ordinal));

				if (storageRoot != null)
				{
					// Avoid making an object its own parent.
					// If the functional container resolves to the same object,
					// treat it as directly below DEVICE instead.
					if (!string.Equals(storageRoot.Id, item.Id, StringComparison.Ordinal))
					{
						return storageRoot;
					}

					return GetRoot(_device);
				}
			}

			// No reliable parent information could be resolved.
			// The caller decides whether this is acceptable or should fail.
			return null;
		}

		internal Stream OpenRead()
		{
			int errTransfer = _device.deviceContent.Transfer(out IPortableDeviceResources resources);
			MediaDeviceException.ThrowIfComError(errTransfer, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.Transfer), _device.Description);

			uint optimalTransferSize = 0;

			int err = resources.GetStream(Id, ref WPD.RESOURCE_DEFAULT, 0, ref optimalTransferSize, out IStream wpdStream);
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceResources), nameof(IPortableDeviceResources.GetStream), _device.Description);

			return new StreamWrapper(wpdStream, Size);
		}

		internal Stream OpenReadThumbnail()
		{
			int errTransfer = _device.deviceContent.Transfer(out IPortableDeviceResources resources);
			MediaDeviceException.ThrowIfComError(errTransfer, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.Transfer), _device.Description);

			uint optimalTransferSize = 0;

			int err = resources.GetStream(Id, ref WPD.RESOURCE_THUMBNAIL, 0, ref optimalTransferSize, out IStream wpdStream);

			if (err == (int)ErrorCodes.ResourceNotAvailable || err == (int)ErrorCodes.InvalidParameter)
			{
				throw new NotSupportedException($"The device {_device.Description} does not support reading thumbnails.");
			}
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceResources), nameof(IPortableDeviceResources.GetStream), _device.Description);

			return new StreamWrapper(wpdStream, Size);
		}

		internal Stream OpenReadIcon()
		{
			int errTransfer = _device.deviceContent.Transfer(out IPortableDeviceResources resources);
			MediaDeviceException.ThrowIfComError(errTransfer, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.Transfer), _device.Description);

			uint optimalTransferSize = 0;

			int err = resources.GetStream(Id, ref WPD.RESOURCE_ICON, 0, ref optimalTransferSize, out IStream wpdStream);

			if (err == (int)ErrorCodes.ResourceNotAvailable || err == (int)ErrorCodes.InvalidParameter)
			{
				throw new NotSupportedException($"The device {_device.Description} does not support reading icons.");
			}
			MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceResources), nameof(IPortableDeviceResources.GetStream), _device.Description);

			return new StreamWrapper(wpdStream, Size);
		}

		internal void UploadFile(string fileName, Stream stream)
		{
			UploadFile(fileName, stream, DateTime.Now, DateTime.Now, DateTime.Now);
		}

		internal void UploadFile(string fileName, Stream stream, DateTime dateCreated, DateTime dateModified, DateTime dateAuthored)
		{

			IPortableDeviceValues portableDeviceValues = ComFactory.CreateDeviceValues();

			portableDeviceValues.SetStringValue(ref WPD.OBJECT_PARENT_ID, Id);
			portableDeviceValues.SetUnsignedLargeIntegerValue(ref WPD.OBJECT_SIZE, (ulong)stream.Length);
			portableDeviceValues.SetStringValue(ref WPD.OBJECT_ORIGINAL_FILE_NAME, fileName);
			portableDeviceValues.SetStringValue(ref WPD.OBJECT_NAME, fileName);

			using (PropVariantFacade created = PropVariantFacade.DateTimeToPropVariant(dateCreated))
			using (PropVariantFacade modified = PropVariantFacade.DateTimeToPropVariant(dateModified))
			using (PropVariantFacade authored = PropVariantFacade.DateTimeToPropVariant(dateAuthored))
			{
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_CREATED, ref created.Value);
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_MODIFIED, ref modified.Value);
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_AUTHORED, ref authored.Value);

				uint num = 0u;
				string? text = null;
				int err = _device.deviceContent.CreateObjectWithPropertiesAndData(portableDeviceValues, out IStream wpdStream, ref num, ref text);
				MediaDeviceException.ThrowIfComError(err, nameof(IPortableDeviceContent), nameof(IPortableDeviceContent.CreateObjectWithPropertiesAndData), _device.Description);

				using (StreamWrapper destinationStream = new StreamWrapper(wpdStream))
				{
					stream.CopyTo(destinationStream);
					destinationStream.Flush();
				}
			}
		}

		internal bool Rename(string newName)
		{
			IPortableDeviceValues portableDeviceValues = ComFactory.CreateDeviceValues();
			IPortableDeviceValues result;

			// with OBJECT_NAME does not work for Amazon Kindle Paperwhite
			portableDeviceValues.SetStringValue(ref WPD.OBJECT_ORIGINAL_FILE_NAME, newName);
			_device.deviceProperties.SetValues(Id, portableDeviceValues, out result);
			ComTrace.WriteObject(result);

			if (result.TryGetStringValue(WPD.OBJECT_ORIGINAL_FILE_NAME, out string check))
			{
				if (check == "Error: S_OK")
				{
					// id can change on rename (e.g. Amazon Kindle Paperwhite) so find new one
					var newItem = _parent?.GetChildren().FirstOrDefault(i => _device.EqualsName(i.Name, newName));
					if (newItem == null)
					{
						throw new InvalidDataException($"Rename error, newItem new name '{newName}' not accessible");
					}
					if (string.IsNullOrEmpty(newItem.Id))
					{
						throw new InvalidDataException($"Rename error, newItem new name '{newName}' does not have an ID");
					}

					Id = newItem.Id;

					Refresh();
					return true;
				}
			}
			return false;
		}

		internal void SetDateCreated(DateTime value)
		{
			IPortableDeviceValues portableDeviceValues = ComFactory.CreateDeviceValues();
			IPortableDeviceValues result;

			using (PropVariantFacade val = PropVariantFacade.DateTimeToPropVariant(value))
			{
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_CREATED, ref val.Value);
				_device.deviceProperties.SetValues(Id, portableDeviceValues, out result);
				ComTrace.WriteObject(result);
			}

			Refresh();
		}
		internal void SetDateCreated(DateTime? value)
		{
			IPortableDeviceValues portableDeviceValues = ComFactory.CreateDeviceValues();
			IPortableDeviceValues result;

			using (PropVariantFacade val = PropVariantFacade.DateTimeToPropVariant(value))
			{
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_CREATED, ref val.Value);
				_device.deviceProperties.SetValues(Id, portableDeviceValues, out result);
				ComTrace.WriteObject(result);
			}

			Refresh();
		}
		internal void SetDateModified(DateTime value)
		{
			IPortableDeviceValues portableDeviceValues = ComFactory.CreateDeviceValues();
			IPortableDeviceValues result;

			using (PropVariantFacade val = PropVariantFacade.DateTimeToPropVariant(value))
			{
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_MODIFIED, ref val.Value);
				_device.deviceProperties.SetValues(Id, portableDeviceValues, out result);
				ComTrace.WriteObject(result);
			}

			Refresh();
		}
		internal void SetDateModified(DateTime? value)
		{
			IPortableDeviceValues portableDeviceValues = ComFactory.CreateDeviceValues();
			IPortableDeviceValues result;

			using (PropVariantFacade val = PropVariantFacade.DateTimeToPropVariant(value))
			{
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_MODIFIED, ref val.Value);
				_device.deviceProperties.SetValues(Id, portableDeviceValues, out result);
				ComTrace.WriteObject(result);
			}

			Refresh();
		}
		internal void SetDateAuthored(DateTime value)
		{
			IPortableDeviceValues portableDeviceValues = ComFactory.CreateDeviceValues();
			IPortableDeviceValues result;

			using (PropVariantFacade val = PropVariantFacade.DateTimeToPropVariant(value))
			{
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_AUTHORED, ref val.Value);
				_device.deviceProperties.SetValues(Id, portableDeviceValues, out result);
				ComTrace.WriteObject(result);
			}

			Refresh();
		}
		internal void SetDateAuthored(DateTime? value)
		{
			IPortableDeviceValues portableDeviceValues = ComFactory.CreateDeviceValues();
			IPortableDeviceValues result;

			using (PropVariantFacade val = PropVariantFacade.DateTimeToPropVariant(value))
			{
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_AUTHORED, ref val.Value);
				_device.deviceProperties.SetValues(Id, portableDeviceValues, out result);
				ComTrace.WriteObject(result);
			}

			Refresh();
		}
		#endregion
	}
}

