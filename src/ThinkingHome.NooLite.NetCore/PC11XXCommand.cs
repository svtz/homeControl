namespace ThinkingHome.NooLite.LibUsb
{
    /// <summary>
    /// Available commands for device
    /// </summary>
    public enum PC11XXCommand : byte
    {
		On = 0x02,
		Off = 0x00,
        Switch = 0x04,
        SetLevel = 0x06,

		LoadState = 0x07,
		SaveState = 0x08,

        Bind = 0x0f,
        UnBind = 0x09,
    }
}