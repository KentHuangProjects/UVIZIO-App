using Acr.UserDialogs;
using BLE.Client.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE.Client.Helpers
{
    public class UVIZIO
    {
        public static async Task writeDevice(IDevice device, IUserDialogs _userDialogs, String commandtext)
        {
            Settings.LAST_COMMAND = commandtext;

            var service = await Settings.DEVICE.GetServiceAsync(KnownServices.RFDUINO_SERVICE);
            var Characteristic = await service.GetCharacteristicAsync(KnownCharacteristics.RFDUINO_WRITE);
            var data = GetBytes(commandtext);

            // not actually a pct 
            data[1] = (byte)Settings.SPEED;
            data[2] = (byte)Settings.BRIGHTNESS;

            await Characteristic.WriteAsync(data);
        }

        public static async Task disconnectDevice(DeviceListItemViewModel device, IUserDialogs _userDialogs)
        {
            if (!device.IsConnected)
                return;

            _userDialogs.ShowLoading($"Turning Off {device.Name}...");

            var service = await Settings.DEVICE.GetServiceAsync(KnownServices.RFDUINO_SERVICE);
            var CharacteristicD = await service.GetCharacteristicAsync(KnownCharacteristics.RFDUINO_DISCONNECT);
            await CharacteristicD.WriteAsync(new byte[0]);
        }

        private static byte[] GetBytes(string text)
        {
            while (text.Split(' ').Length < 3)
                text += " 00";

            return text.Split(' ').Where(token => !string.IsNullOrEmpty(token)).Select(token => Convert.ToByte(token, 16)).ToArray();
        }
    }
}
