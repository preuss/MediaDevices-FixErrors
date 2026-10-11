using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaDevices
{
    /// <summary>
    /// Translates vendor opcodes reported by a device into <see cref="VendorOpCode"/>
    /// items with manufacturer, numeric id and name.
    /// </summary>
    public static class VendorOpCodes
    {
        /// <summary>
        /// Returns the vendor opcodes supported by the device, translated through the
        /// opcode enum of the detected manufacturer. Unknown manufacturers fall back
        /// to raw opcode values.
        /// </summary>
        /// <param name="device">The connected device.</param>
        /// <param name="manufacturer">
        /// Manufacturer of the device. Pass the value when it is already known to avoid
        /// an additional device query; when null it is resolved from the device.
        /// </param>
        /// <returns>The supported vendor opcodes.</returns>
        /// <exception cref="MediaDevices.NotConnectedException">device is not connected.</exception>
        public static IEnumerable<VendorOpCode> From(MediaDevice device, ManufacturerId? manufacturer = null)
        {
            manufacturer ??= device.GetManufacturer();

            switch (manufacturer)
            {
                case ManufacturerId.Canon:
                    return From(device.VendorOpcodes<OpCodesCanon>());
                case ManufacturerId.Nikon:
                    return From(device.VendorOpcodes<OpCodesNikon>());
                case ManufacturerId.SonyCorporation:
                case ManufacturerId.SonyMobileCommunications:
                    return From(device.VendorOpcodes<OpCodesSony>());
                case ManufacturerId.Samsung:
                    return From(device.VendorOpcodes<OpCodesSamsung>());
                case ManufacturerId.Panasonic:
                    return From(device.VendorOpcodes<OpCodesPanasonic>());
                case ManufacturerId.Leica:
                    return From(device.VendorOpcodes<OpCodesLeica>());
                case ManufacturerId.Fujifilm:
                    return From(device.VendorOpcodes<OpCodesFujifilm>());
                case ManufacturerId.Microsoft:
                    return From(device.VendorOpcodes<OpCodesMicrosoft>());
                default:
                    return From(device.VendorOpcodes(), manufacturer);
            }
        }

        /// <summary>
        /// Converts vendor opcode enum values to items, with the manufacturer
        /// derived from the enum type.
        /// </summary>
        /// <typeparam name="TEnum">Vendor-specific opcode enum.</typeparam>
        /// <param name="opCodes">The opcodes to convert.</param>
        /// <returns>The converted opcodes.</returns>
        public static IEnumerable<VendorOpCode> From<TEnum>(IEnumerable<TEnum> opCodes) where TEnum : struct
        {
            ManufacturerId? manufacturer = ManufacturerOf(typeof(TEnum));
            // ponytail: ToString on an unconstrained generic is annotated nullable; value types never return null
            return opCodes.Select(o => new VendorOpCode(manufacturer, Convert.ToUInt16(o), o.ToString()!)).ToList();
        }

        /// <summary>
        /// Converts raw vendor opcode values to items with unresolved names.
        /// </summary>
        /// <param name="opCodes">The opcodes to convert.</param>
        /// <param name="manufacturer">Manufacturer of the device, or null when unknown.</param>
        /// <returns>The converted opcodes.</returns>
        public static IEnumerable<VendorOpCode> From(IEnumerable<uint> opCodes, ManufacturerId? manufacturer)
        {
            return opCodes.Select(o => new VendorOpCode(manufacturer, (ushort)o, "Unknown")).ToList();
        }

        private static ManufacturerId? ManufacturerOf(Type enumType) => enumType switch
        {
            _ when enumType == typeof(OpCodesCanon) => ManufacturerId.Canon,
            _ when enumType == typeof(OpCodesNikon) => ManufacturerId.Nikon,
            _ when enumType == typeof(OpCodesSony) => ManufacturerId.SonyCorporation,
            _ when enumType == typeof(OpCodesSamsung) => ManufacturerId.Samsung,
            _ when enumType == typeof(OpCodesPanasonic) => ManufacturerId.Panasonic,
            _ when enumType == typeof(OpCodesLeica) => ManufacturerId.Leica,
            _ when enumType == typeof(OpCodesFujifilm) => ManufacturerId.Fujifilm,
            _ when enumType == typeof(OpCodesMicrosoft) => ManufacturerId.Microsoft,
            _ => null,
        };
    }
}
