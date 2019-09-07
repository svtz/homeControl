namespace ThinkingHome.NooLite.LibUsb
{
	public enum PC11XXLedCommand : byte
	{
		SetLevel = 0x06,
		ChangeColor = 0x11,		// 17
		SetColorMode = 0x12,	// 18
		SetColorSpeed = 0x13	// 19
	}
}