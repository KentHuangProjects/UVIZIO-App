

using System;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Forms;

namespace BLE.Client.ViewModels
{
    public class DeviceListItemViewModel : MvxNotifyPropertyChanged
    {

        

        public IDevice Device { get; private set; }

        public Guid Id => Device.Id;
        public bool IsConnected => Device.State == DeviceState.Connected;
        public int Rssi => Device.Rssi;
        public string Name => Device.Name;
        //public Image ConnectionImage => GetConnectionImage(Device.Rssi);
        public string ConnectionImageString => GetConnectionImageString(Device.Rssi);

        public DeviceListItemViewModel(IDevice device)
        {
            Device = device;
        }

        public void Update(IDevice newDevice = null)
        {
            if (newDevice != null)
            {
                Device = newDevice;
            }
            RaisePropertyChanged(nameof(IsConnected));
            RaisePropertyChanged(nameof(Rssi));
        }

        private string GetConnectionImageString(int Rssi)
        {
            int imgId = (Rssi+100) / 25;
            if (imgId < 0) imgId = 0;
            if (imgId > 4) imgId = 4;

            return "bar_" + imgId + ".png";
        }




    }
}
