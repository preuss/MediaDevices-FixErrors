using MediaDeviceApp.Mvvm;
using MediaDevices;
using System.Collections.Generic;

namespace MediaDeviceApp.ViewModel
{
    public class VendorViewModel : BaseViewModel
    {
        private ManufacturerId? manufacturer;
        private List<VendorOpCode> opCodes;
        private string vendorExtensionDescription;

        public VendorViewModel()
        { }

        public void Update(MediaDevice device)
        {
            this.Manufacturer = device.GetManufacturer();
            this.OpCodes = VendorOpCodes.From(device, this.Manufacturer).ToList();
            this.VendorExtensionDescription = device.VendorExtentionDescription();
        }

        public ManufacturerId? Manufacturer
        {
            get
            {
                return this.manufacturer;
            }
            set
            {
                if (this.manufacturer != value)
                {
                    this.manufacturer = value;
                    NotifyPropertyChanged(nameof(Manufacturer));
                }
            }
        }

        public List<VendorOpCode> OpCodes
        {
            get
            {
                return this.opCodes;
            }
            set
            {
                if (this.opCodes != value)
                {
                    this.opCodes = value;
                    NotifyPropertyChanged(nameof(OpCodes));
                }
            }
        }

        public string VendorExtensionDescription
        {
            get
            {
                return this.vendorExtensionDescription;
            }
            set
            {
                if (this.vendorExtensionDescription != value)
                {
                    this.vendorExtensionDescription = value;
                    NotifyPropertyChanged(nameof(VendorExtensionDescription));
                }
            }
        }
    }
}
