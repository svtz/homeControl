using System;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace ThinkingHome.NooLite.Common
{
	public abstract class BaseAdapter : IDisposable
	{
		private const int WriteIntervalMilliseconds = 200;
		private const int VendorId = 0x16C0;
		private const int ConfigId = 1;
		private const int InterfaceId = 0;
		protected const int BufferSize = 9;
		protected abstract int ProductId { get; }
		protected abstract Func<string, bool> ProductNameFilter { get; }
		
		protected IUsbInterface Device { get; private set; }

		public bool IsOpen => Device != null && Device.IsOpen;
		
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
			Device = SelectDevice();

			if (Device is IUsbDevice wholeDevice)
			{
				var result = wholeDevice.SetConfiguration(ConfigId);
				result &= wholeDevice.ClaimInterface(InterfaceId);
				
				return result;
			}
			
			return Device != null;
		}

		public virtual void Dispose()
		{
			if (Device != null)
			{
				if (Device is IUsbDevice wholeDevice)
					wholeDevice.ReleaseInterface(InterfaceId);

				Device.Close();
				Device = null;
			}
		}

		protected static string GetProductString(IUsbInterface usbDevice)
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
			var productString = GetProductString(Device);
			
			return !string.IsNullOrWhiteSpace(productString) 
				? productString 
				: base.ToString();
		}
		
		private const int RequestTypeIn = (int)UsbRequestType.TypeClass | (int)UsbRequestRecipient.RecipInterface | (int)UsbEndpointDirection.EndpointIn;
		private const int RequestTypeOut = (int)UsbRequestType.TypeClass | (int)UsbRequestRecipient.RecipInterface | (int)UsbEndpointDirection.EndpointOut;
		private const int Request = 0x9;
		private const int Value = 0x300;
		
		protected byte[] ReadBufferData()
		{
			var buffer = new byte[BufferSize];
			var setup = new UsbSetupPacket(RequestTypeIn, Request, Value, 0, 0);
			
			Device.ControlTransfer(ref setup, buffer, buffer.Length, out _);

			return buffer;
		}
		
		protected void WriteBufferData(byte[] buffer)
		{
			var setup = new UsbSetupPacket(RequestTypeOut, Request, Value, 0, 0);
			Device.ControlTransfer(ref setup, buffer, buffer.Length, out _);
			System.Threading.Thread.Sleep(WriteIntervalMilliseconds);
		}
	}
}
