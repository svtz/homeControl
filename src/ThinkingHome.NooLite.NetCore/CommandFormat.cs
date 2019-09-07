namespace ThinkingHome.NooLite.LibUsb
{
	internal enum CommandFormat
	{
		Undefined = 0x00,
		OneByteData = 0x01,
		FourByteData = 0x03,
		LED = 0x04
	}
}