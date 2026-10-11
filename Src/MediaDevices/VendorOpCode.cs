namespace MediaDevices
{

// The positional record properties (Manufacturer, Id, Name) are compiler-generated and cannot
// carry individual XML documentation. The project generates a documentation file, so CS1591 is
// suppressed file-wide the same way as in the mirrored vendor opcode enum files.
#pragma warning disable CS1591

    /// <summary>
    /// A vendor extended operation code together with the manufacturer it belongs to.
    /// The numeric id alone is ambiguous across vendors (0x9201 is both
    /// <see cref="OpCodesNikon.StartLiveView"/> and <see cref="OpCodesSony.Sony_SDIO_Connect"/>),
    /// so the manufacturer is part of this item's identity.
    /// </summary>
    public sealed record VendorOpCode(ManufacturerId? Manufacturer, ushort Id, string Name)
    {
        /// <summary>
        /// Returns the opcode as a hex id followed by its name, e.g. "0x9201 StartLiveView".
        /// </summary>
        public override string ToString() => $"0x{Id:X4} {Name}";
    }
}
