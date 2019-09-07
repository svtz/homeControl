using System;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace ThinkingHome.NooLite.LibUsb.Common
{
	public abstract class BaseAdapter : IDisposable
	{
		private const int WriteIntervalMilliseconds = 200;
		private const int VendorId = 0x16C0;
		private const int ConfigId = 1;
		private const int InterfaceId = 0;
		private const int BufferSize = 8;
		protected abstract int ProductId { get; }
		protected abstract Func<string, bool> ProductNameFilter { get; }

		private IUsbInterface _device;

		public bool IsOpen => _device != null && _device.IsOpen;
		
		private IUsbInterface SelectDevice()
		{
			return (IUsbInterface) UsbDevice.OpenUsbDevice(
				reg => reg.Vid == VendorId
						&& reg.Pid == ProductId
						&& reg.Device is IUsbInterface @interface
						&& ProductNameFilter(GetProductString(@interface)));
		}

		public virtual bool OpenDevice()
		{
			_device = SelectDevice();

			if (_device is IUsbDevice wholeDevice)
			{
				var result = wholeDevice.SetConfiguration(ConfigId);
				result &= wholeDevice.ClaimInterface(InterfaceId);
				
				return result;
			}
			
			return _device != null;
		}

		public virtual void Dispose()
		{
			if (_device != null)
			{
				if (_device is IUsbDevice wholeDevice)
					wholeDevice.ReleaseInterface(InterfaceId);

				_device.Close();
				_device = null;
			}
		}

		private static string GetProductString(IUsbInterface usbDevice)
		{
			if (usbDevice != null)
			{
				return usbDevice.Info.ProductString.Trim('\0');
			}

			return string.Empty;
		}

		protected static byte[] CreateCommand(params byte[] bytes)
		{
			if (bytes.Length > BufferSize)
				throw new ArgumentException(nameof(bytes));

			var buffer = new byte[BufferSize];
			
			Array.Copy(bytes, 0, buffer, 0, bytes.Length);

			return buffer;
		}
		
		public override string ToString()
		{
			var productString = GetProductString(_device);
			
			return !string.IsNullOrWhiteSpace(productString) 
				? productString 
				: base.ToString();
		}
		
		private const byte RequestTypeIn = (byte)UsbRequestType.TypeClass | (byte)UsbRequestRecipient.RecipInterface | (byte)UsbEndpointDirection.EndpointIn;
		private const byte RequestTypeOut = (byte)UsbRequestType.TypeClass | (byte)UsbRequestRecipient.RecipInterface | (byte)UsbEndpointDirection.EndpointOut;
		private const int Request = 0x9;
		private const int Value = 0x300;
		
		protected byte[] ReadBufferData()
		{
			var buffer = new byte[BufferSize];
			var setup = new UsbSetupPacket(RequestTypeIn, Request, Value, 0, BufferSize);
			_device.ControlTransfer(ref setup, buffer, buffer.Length, out _);
			return buffer;
		}
		
		protected void WriteBufferData(byte[] buffer)
		{
			var setup = new UsbSetupPacket(RequestTypeOut, Request, Value, 0, BufferSize);
			_device.ControlTransfer(ref setup, buffer, buffer.Length, out _);
			System.Threading.Thread.Sleep(WriteIntervalMilliseconds);
		}
	}
}
