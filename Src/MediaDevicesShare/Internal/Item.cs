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
			device.deviceContent.GetObjectIDsFromPersistentUniqueIDs(collection, out IPortableDevicePropVariantCollection results);

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
			catch (Exception ex)
			{
				Trace.TraceError($"{ex.Message} for {Id}");
				return;
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
				if (_parent == null)
				{
					_parent = string.IsNullOrEmpty(ParentId) ? null : new Item(_device, ParentId, Path.GetDirectoryName(Path.GetDirectoryName(FullName)));
				}
				return _parent;
			}
		}

		#endregion

		#region Methods

		public IEnumerable<Item> GetChildren()
		{
			IEnumPortableDeviceObjectIDs enumerator;
			try
			{
				_device.deviceContent.EnumObjects(0, Id, null, out enumerator);
			}
			catch (COMException e)
			{
				Trace.WriteLine("IPortableDeviceContent.EnumObjects failed");
				yield break;
			}

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

		public IEnumerable<Item> GetChildren(string? pattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			IEnumPortableDeviceObjectIDs enumerator;
			try
			{
				_device.deviceContent.EnumObjects(0, Id, null, out enumerator);
			}
			catch (COMException e)
			{
				Trace.WriteLine("IPortableDeviceContent.EnumObjects failed");
				yield break;
			}

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
				enumerator.Next(NUM_OBJECTS_TO_REQUEST, objectIds, ref fetched);
			}
		}

		internal Item? CreateSubdirectory(string path)
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

					string id = string.Empty;
					try
					{
						_device.deviceContent.CreateObjectWithPropertiesOnly(deviceValues, ref id);
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
					throw new Exception($"A path of the path {folder} is a file");
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
			// TODO: get the results back and handle failures correctly

			_device.deviceContent.Delete(recursive ? PORTABLE_DEVICE_DELETE_WITH_RECURSION : PORTABLE_DEVICE_DELETE_NO_RECURSION, objectIdCollection, ref results);

			ComTrace.WriteObject(objectIdCollection);
		}

		public string GetPath()
		{
			if (Id == Item.RootId)
			{
				return @"\";
			}

			Item? item = this;
			StringBuilder sb = new StringBuilder();
			do
			{
				// ++ TODO
				if (string.IsNullOrWhiteSpace(item.ParentId))
				{
					item = TryHandleNonHierarchicalStorage();

					if (item == null)
					{
						throw new Exception($"Problem occurred when trying to get full object path on device {_device.FriendlyName}.");
					}
				}

				// -- TODO

				sb.Insert(0, item.Name);
				sb.Insert(0, DIRECTORY_SEPARATOR_CHAR);

			} while (!(item = new Item(_device, item.ParentId)).IsRoot);
			return sb.ToString();
		}

		// TODO

		/// <summary>
		/// Handles DCF storages specific for Apple iPhones.
		/// </summary>
		/// <returns></returns>
		private Item? TryHandleNonHierarchicalStorage()
		{
			// EXPLANATION
			// Some MTP compatible devices uses different storage formats that Generic
			// Hierarchical storage like WP, Android. Good examples are Apple devices,
			// which are using DCF storage. The specific in that storage is a way how
			// directories handles parent object ID. If in Generic Hierarchical storage
			// we check parent ID of root directory, it contains an ID of functional storage
			// so that means storage ID. In DCF when we check parent ID of root object
			// it will have object ID, not storage ID, e.g. parent id is o10001 (object10001),
			// but storage has ID = s10001 (storage10001). So to find a parent of top most folder
			// we need to fetch an object functional container ID. Which is storage for top most
			// directory.
			var drives = _device.GetDrives();
			var storageRoot = drives.FirstOrDefault(s => s.RootDirectory != null && s.RootDirectory.Id == ParentContainerId);
			return storageRoot?.RootDirectory?.item;
		}

		internal Stream OpenRead()
		{
			_device.deviceContent.Transfer(out IPortableDeviceResources resources);

			IStream wpdStream;
			uint optimalTransferSize = 0;

			resources.GetStream(Id, ref WPD.RESOURCE_DEFAULT, 0, ref optimalTransferSize, out wpdStream);

			return new StreamWrapper(wpdStream, Size);
		}

		internal Stream OpenReadThumbnail()
		{
			_device.deviceContent.Transfer(out IPortableDeviceResources resources);

			IStream wpdStream;
			uint optimalTransferSize = 0;

			resources.GetStream(Id, ref WPD.RESOURCE_THUMBNAIL, 0, ref optimalTransferSize, out wpdStream);

			return new StreamWrapper(wpdStream, Size);
		}

		internal Stream OpenReadIcon()
		{
			_device.deviceContent.Transfer(out IPortableDeviceResources resources);

			IStream wpdStream;
			uint optimalTransferSize = 0;

			resources.GetStream(Id, ref WPD.RESOURCE_ICON, 0, ref optimalTransferSize, out wpdStream);

			return new StreamWrapper(wpdStream, Size);
		}

		internal void UploadFile(string fileName, Stream stream)
		{

			IPortableDeviceValues portableDeviceValues = ComFactory.CreateDeviceValues();

			portableDeviceValues.SetStringValue(ref WPD.OBJECT_PARENT_ID, Id);
			portableDeviceValues.SetUnsignedLargeIntegerValue(ref WPD.OBJECT_SIZE, (ulong)stream.Length);
			portableDeviceValues.SetStringValue(ref WPD.OBJECT_ORIGINAL_FILE_NAME, fileName);
			portableDeviceValues.SetStringValue(ref WPD.OBJECT_NAME, fileName);
			// test
			using (PropVariantFacade now = PropVariantFacade.DateTimeToPropVariant(DateTime.Now))
			{
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_CREATED, ref now.Value);
				portableDeviceValues.SetValue(ref WPD.OBJECT_DATE_MODIFIED, ref now.Value);

				uint num = 0u;
				string? text = null;
				_device.deviceContent.CreateObjectWithPropertiesAndData(portableDeviceValues, out IStream wpdStream, ref num, ref text);

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

