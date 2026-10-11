using System;
using MediaDevices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MediaDevicesUnitTest
{
    /// <summary>
    /// Device-independent self-check for the vendor opcode enums and the generic
    /// vendor-API conversions. Verifies that the mirrored bassman V3.x data keeps
    /// its expected values.
    /// </summary>
    [TestClass]
    public class VendorOpCodesUnitTest
    {
        [TestMethod]
        public void BaseOpCodesValuesTest()
        {
            Assert.AreEqual((ushort)0x0000, (ushort)OpCodes.Unknown);
            Assert.AreEqual((ushort)0x1001, (ushort)OpCodes.GetDeviceInfo);
            Assert.AreEqual((ushort)0x1002, (ushort)OpCodes.OpenSession);
            Assert.AreEqual((ushort)0x1004, (ushort)OpCodes.GetStorageIDs);
            Assert.AreEqual((ushort)0x9201, (ushort)OpCodes.ReportAddedDeletedItems);
        }

        [TestMethod]
        public void VendorOpCodesValuesTest()
        {
            Assert.AreEqual((ushort)0x910F, (ushort)OpCodesCanon.EOS_RemoteRelease);
            Assert.AreEqual((ushort)0x9201, (ushort)OpCodesNikon.StartLiveView);
            Assert.AreEqual((ushort)0x90CB, (ushort)OpCodesNikon.AfCaptureSDRAM);
            Assert.AreEqual((ushort)0x9201, (ushort)OpCodesSony.Sony_SDIO_Connect);
            Assert.AreEqual((ushort)0x9501, (ushort)OpCodesSamsung.Unknown1);
            Assert.AreEqual((ushort)0x902B, (ushort)OpCodesFujifilm.Fujifilm_GetCheckEvent);
        }

        [TestMethod]
        public void ManufacturerIdValuesTest()
        {
            Assert.AreEqual((ushort)0x03F0, (ushort)ManufacturerId.HewlettPackard);
            Assert.AreEqual((ushort)0x04A9, (ushort)ManufacturerId.Canon);
            Assert.AreEqual((ushort)0x04B0, (ushort)ManufacturerId.Nikon);
            Assert.AreEqual((ushort)0x054C, (ushort)ManufacturerId.SonyCorporation);
            Assert.AreEqual((ushort)0x05AC, (ushort)ManufacturerId.Apple);
            Assert.AreEqual((ushort)0x2717, (ushort)ManufacturerId.Xiaomi);
        }

        [TestMethod]
        public void GenericVendorConversionTest()
        {
            // the generic vendor API converts the typed opcode to uint through Convert.ToUInt32
            Assert.AreEqual(0x9201u, Convert.ToUInt32(OpCodesNikon.StartLiveView));
            Assert.AreEqual(0x910Fu, Convert.ToUInt32(OpCodesCanon.EOS_RemoteRelease));

            // and back for results via Enum.ToObject
            Assert.AreEqual(OpCodesNikon.StartLiveView, (OpCodesNikon)Enum.ToObject(typeof(OpCodesNikon), (ushort)0x9201));
        }

        [TestMethod]
        public void VendorOpCodeConversionTest()
        {
            IReadOnlyList<VendorOpCode> items = VendorOpCodes.From(new[] { OpCodesNikon.StartLiveView, OpCodesNikon.AfCaptureSDRAM }).ToList();
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(ManufacturerId.Nikon, items[0].Manufacturer);
            Assert.AreEqual((ushort)0x9201, items[0].Id);
            Assert.AreEqual("StartLiveView", items[0].Name);
            Assert.AreEqual("0x9201 StartLiveView", items[0].ToString());
        }

        [TestMethod]
        public void ManufacturerFromEnumTypeTest()
        {
            Assert.AreEqual(ManufacturerId.Canon, VendorOpCodes.From(new[] { OpCodesCanon.EOS_RemoteRelease }).Single().Manufacturer);
            Assert.AreEqual(ManufacturerId.Nikon, VendorOpCodes.From(new[] { OpCodesNikon.StartLiveView }).Single().Manufacturer);
            Assert.AreEqual(ManufacturerId.SonyCorporation, VendorOpCodes.From(new[] { OpCodesSony.Sony_SDIO_Connect }).Single().Manufacturer);
            Assert.AreEqual(ManufacturerId.Samsung, VendorOpCodes.From(new[] { OpCodesSamsung.Unknown1 }).Single().Manufacturer);
            Assert.AreEqual(ManufacturerId.Panasonic, VendorOpCodes.From(new[] { OpCodesPanasonic.Panasonic_InternalInit }).Single().Manufacturer);
            Assert.AreEqual(ManufacturerId.Leica, VendorOpCodes.From(new[] { OpCodesLeica.Leica_SetCameraSettings }).Single().Manufacturer);
            Assert.AreEqual(ManufacturerId.Fujifilm, VendorOpCodes.From(new[] { OpCodesFujifilm.Fujifilm_GetCheckEvent }).Single().Manufacturer);
            Assert.AreEqual(ManufacturerId.Microsoft, VendorOpCodes.From(new[] { OpCodesMicrosoft.Zune_GetUndefined001 }).Single().Manufacturer);

            // the canonical opcode enum is not vendor-specific and has no manufacturer
            Assert.IsNull(VendorOpCodes.From(new[] { OpCodes.GetDeviceInfo }).Single().Manufacturer);
        }

        [TestMethod]
        public void RawOpCodeFallbackTest()
        {
            IReadOnlyList<VendorOpCode> items = VendorOpCodes.From(new uint[] { 0x9201 }, null).ToList();
            Assert.IsNull(items[0].Manufacturer);
            Assert.AreEqual((ushort)0x9201, items[0].Id);
            Assert.AreEqual("Unknown", items[0].Name);
            Assert.AreEqual("0x9201 Unknown", items[0].ToString());
        }
    }
}
