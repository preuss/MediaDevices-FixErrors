namespace MediaDevices
{

// vendor opcode data mirrored from bassman V3.x - keep files 1:1 with his source, docs stay sparse.
#pragma warning disable CS1591

public enum OpCodesLeica : ushort
{
    #region Leica (0x9001 - 0x9003)

    /// <summary>
    /// Used by desktop applications such as Leica Image Shuttle to transfer camera settings and profiles directly to the camera body.
    /// </summary>
    Leica_SetCameraSettings = 0x9001,

    /// <summary>
    /// Retrieves the current configuration package and the status of the camera system.
    /// </summary>
    Leica_GetCameraSettings = 0x9002,

    /// <summary>
    /// Reads out specific lens parameters. 
    /// This is used, among other things, in the Adobe Lightroom tethering plugin to retrieve lens metadata from digital rangefinder cameras (such as the M-series).
    /// </summary>
    Leica_GetLensParameter = 0x9003,

    #endregion


}
}