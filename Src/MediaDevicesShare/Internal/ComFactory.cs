using System;
using System.Collections.Generic;
using System.Text;

namespace MediaDevices.Internal
{
	/// <summary>
	/// Centralized factory for all COM objects used with Windows Portable Device API.
	/// This is needed because [ComImport] classes cannot contain static methods.
	/// </summary>
	internal static class ComFactory
	{
		// Singleton for all manager types
		private static readonly Lazy<IPortableDeviceManager> _portableDeviceManager = new(() => (IPortableDeviceManager)new PortableDeviceManager());
		private static readonly Lazy<MediaDevices.WMDM.IWMDeviceManager> _wmDeviceManager = new(() => (MediaDevices.WMDM.IWMDeviceManager)new MediaDevices.WMDM.MediaDevMgr());

		// Singleton instances
		private static IPortableDeviceManager PortableDeviceManager => _portableDeviceManager.Value;
		private static IPortableDeviceServiceManager PortableDeviceServiceManager => (IPortableDeviceServiceManager)PortableDeviceManager;
		private  static MediaDevices.WMDM.IWMDeviceManager WMDeviceManager => _wmDeviceManager.Value;

		/// <summary>
		/// Creates a new IPortableDevice instance.
		/// </summary>
		public static IPortableDevice CreateDevice()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			return (IPortableDevice)new PortableDevice();
		}

		/// <summary>
		/// Creates a new IPortableDeviceManager instance.
		/// Note: The same COM object also implements IPortableDeviceServiceManager.
		/// </summary>
		public static IPortableDeviceManager GetDeviceManagerInstance()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			return PortableDeviceManager;
		}

		/// <summary>
		/// Gets the singleton instance of IPortableDeviceServiceManager.
		/// </summary>
		public static IPortableDeviceServiceManager GetDeviceServiceManagerInstance()
		{
			return PortableDeviceServiceManager;
		}

		/// <summary>
		/// Creates both IPortableDeviceManager and IPortableDeviceServiceManager from the same COM object.
		/// This ensures they share the same underlying COM instance.
		/// </summary>
		public static (IPortableDeviceManager deviceManager, IPortableDeviceServiceManager serviceManager) GetDeviceManagersInstance()
		{
			var manager = new PortableDeviceManager();
			return (
				// ReSharper disable once SuspiciousTypeConversion.Global
				PortableDeviceManager,
				// ReSharper disable once SuspiciousTypeConversion.Global
				PortableDeviceServiceManager
			);
		}

		/// <summary>
		/// Creates a new IPortableDeviceService instance.
		/// </summary>
		public static IPortableDeviceService CreateDeviceService()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			return (IPortableDeviceService)new PortableDeviceService();
		}

		/// <summary>
		/// Creates a new IPortableDevicePropVariantCollection instance.
		/// </summary>
		public static IPortableDevicePropVariantCollection CreateDevicePropVariantCollection()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			return (IPortableDevicePropVariantCollection)new PortableDevicePropVariantCollection();
		}

		/// <summary>
		/// Creates a new IPortableDeviceValues instance.
		/// </summary>
		public static IPortableDeviceValues CreateDeviceValues()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			return (IPortableDeviceValues)new PortableDeviceValues();
		}

		/// <summary>
		/// Creates a new IPortableDeviceKeyCollection instance.
		/// </summary>
		public static IPortableDeviceKeyCollection CreateDeviceKeyCollection()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			return (IPortableDeviceKeyCollection)new PortableDeviceKeyCollection();
		}

		/// <summary>
		/// Creates a new IEnumPortableDeviceConnectors instance.
		/// </summary>
		public static IEnumPortableDeviceConnectors CreateDeviceConnectors()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			return (IEnumPortableDeviceConnectors)new EnumPortableDeviceConnectors();
		}

		// ReSharper disable once CommentTypo
		/// <summary>
		/// Creates a new IWMDeviceManager instance (WMDM).
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public static MediaDevices.WMDM.IWMDeviceManager GetWMDeviceManagerInstance()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			return WMDeviceManager;
		}

		// ReSharper disable once CommentTypo
		/// <summary>
		/// Creates a new MediaDevMgrClassFactory instance (WMDM).
		/// </summary>
		public static object CreateMediaDevMgrClassFactory()
		{
			return new MediaDevices.WMDM.MediaDevMgrClassFactory();
		}
	}
}
