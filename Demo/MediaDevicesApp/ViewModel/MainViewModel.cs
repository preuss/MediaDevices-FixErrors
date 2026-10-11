using MediaDeviceApp.Mvvm;
using MediaDevices;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Threading;
using System.Media;

namespace MediaDeviceApp.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private List<MediaDevice> _devices;
        private MediaDevice? _selectedDevice;
        private bool _usePrivateDevices = false;
        private bool _canReset = true;

        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand ResetCommand { get; private set; }
        public DelegateCommand UsbChangedCommand { get; private set; }


        public InfoViewModel Info { get; private set; }
        public CapabilityViewModel Capability { get; private set; }
        public ContentLocationViewModel ContentLocation { get; private set; }
        public StorageViewModel Storage { get; private set; }
        public DriveViewModel Drive { get; private set; }
        public RootViewModel Root { get; private set; }
        public FilesViewModel Files { get; private set; }
        public StillImageViewModel StillImage { get; private set; }
        public SmsViewModel Sms { get; private set; }
        public ExplorerViewModel Explorer { get; private set; }
        public VendorViewModel Vendor { get; private set; }
        public ServicesViewModel Services { get; private set; }
        public ServiceInfoViewModel ServiceInfo { get; private set; }
        public ServiceStatusViewModel ServiceStatus { get; private set; }
        public ServiceMetadataViewModel ServiceMetadata { get; private set; }

        public MainViewModel()
        {
            RefreshCommand = new DelegateCommand(OnRefresh);
            ResetCommand = new DelegateCommand(OnReset);
            UsbChangedCommand = new DelegateCommand(OnUsbChanged);


            Info = new InfoViewModel();
            Capability = new CapabilityViewModel();
            ContentLocation = new ContentLocationViewModel();
            Storage = new StorageViewModel();
            Drive = new DriveViewModel();
            Root = new RootViewModel();
            Files = new FilesViewModel();
            StillImage = new StillImageViewModel();
            Sms = new SmsViewModel();
            Explorer = new ExplorerViewModel();
            Vendor = new VendorViewModel();
            Services = new ServicesViewModel();
            ServiceInfo = new ServiceInfoViewModel();
            ServiceStatus = new ServiceStatusViewModel();
            ServiceMetadata = new ServiceMetadataViewModel();

            OnRefresh();
        }
        
        public bool UsePrivateDevices
        {
            get
            {
                return _usePrivateDevices;
            }
            set
            {
                if (_usePrivateDevices != value)
                {
                    _usePrivateDevices = value;
                    OnRefresh();
                    NotifyPropertyChanged(nameof(UsePrivateDevices));
                }
            }
        }

        private void OnRefresh()
        {
            if (_usePrivateDevices)
            {
                Devices = MediaDevice.GetPrivateDevices().ToList();
            }
            else
            {
                Devices = MediaDevice.GetDevices().ToList();
            }
            if (_selectedDevice == null)
            {
                SelectedDevice = Devices.FirstOrDefault();
            }
        }

        private void OnUsbChanged()
        {
            SystemSounds.Beep.Play();
            if (_usePrivateDevices)
            {
                Devices = MediaDevice.GetPrivateDevices().ToList();
            }
            else
            {
                Devices = MediaDevice.GetDevices().ToList();
            }
            if (_selectedDevice == null)
            {
                SelectedDevice = Devices.FirstOrDefault();
            }
        }

        public List<MediaDevice> Devices
        {
            get { return _devices; }
            set { _devices = value; NotifyPropertyChanged(nameof(Devices)); }
        }

        public MediaDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (value != _selectedDevice)
                {
                    if (_selectedDevice != null)
                    {
                        try
                        {
                            _selectedDevice.Disconnect();
                        }
                        catch { }
                    }
                    _selectedDevice = value;
                    if (_selectedDevice != null)
                    {
                        _selectedDevice.Connect();

                        _canReset = true;
                    }
                    else
                    {
                        _canReset = false;
                    }
                    NotifyAllPropertiesChanged();
                    
                    TryUpdate(() => Info.Update(_selectedDevice));
                    TryUpdate(() => Capability.Update(_selectedDevice));
                    TryUpdate(() => ContentLocation.Update(_selectedDevice));
                    TryUpdate(() => Storage.Update(_selectedDevice));
                    TryUpdate(() => Drive.Update(_selectedDevice));
                    TryUpdate(() => Root.Update(_selectedDevice));
                    TryUpdate(() => Files.Update(_selectedDevice));
                    TryUpdate(() => StillImage.Update(_selectedDevice));
                    TryUpdate(() => Sms.Update(_selectedDevice));
                    TryUpdate(() => Explorer.Update(_selectedDevice));
                    TryUpdate(() => Vendor.Update(_selectedDevice));
                    TryUpdate(() => Services.Update(_selectedDevice));
                    TryUpdate(() => ServiceInfo.Update(_selectedDevice));
                    TryUpdate(() => ServiceStatus.Update(_selectedDevice));
                    TryUpdate(() => ServiceMetadata.Update(_selectedDevice));
                    //if (selectedDevice.Description != "My Passport 25E2")
                    //{
                    //    var root = selectedDevice.GetRootDirectory();
                    //    var result = root.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).ToList();
                    //    var files = result.OfType<MediaFileInfo>().ToList();
                    //}

                }
            }
        }
        
        private void OnReset()
        {
            if (MsgBox.ShowQuestion("Do your really want to reset your device?"))
            {
                try
                {
                    _selectedDevice.ResetDevice();
                }
                catch (Exception ex)
                {
                    MsgBox.ShowError(ex.Message);
                }
            }
        }

        public bool CanReset
        {
            get { return _canReset; }
            set { _canReset = value; NotifyPropertyChanged(nameof(CanReset)); }

        }

        private static void TryUpdate(Action update)
        {
            try
            {
                update();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"View model update failed: {ex}");
            }
        }
    }
}
