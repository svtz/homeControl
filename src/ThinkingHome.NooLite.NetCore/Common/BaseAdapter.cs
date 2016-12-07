using System;
using System.Linq;
using System.Text;
using HidLibrary;

namespace ThinkingHome.NooLite.Common
{
	public abstract class BaseAdapter : IDisposable
	{
		public int VendorId
		{
			get { return 0x16C0; }
		}

		public abstract int ProductId { get; }

		protected HidDevice device;

		public bool IsConnected
		{
			get { return device != null && device.IsConnected; }
		}

		protected virtual HidDevice SelectDevice()
		{
			return HidDevices.Enumerate(VendorId, ProductId).FirstOrDefault();
		}

		public virtual bool OpenDevice()
		{
			device = SelectDevice();

			if (device != null)
			{
				device.OpenDevice();
				device.MonitorDeviceEvents = true;
				return true;
			}
			return false;
		}

		public virtual void Dispose()
		{
			if (device != null)
			{
				device.Dispose();
			}
		}

		public static string GetProductString(HidDevice hidDevice)
		{
			byte[] bytes;

			if (hidDevice != null && hidDevice.ReadProduct(out bytes))
			{
				string productString = Encoding.Unicode.GetString(bytes);

				return productString.Trim('\0');
			}

			return string.Empty;
		}
	}
}
