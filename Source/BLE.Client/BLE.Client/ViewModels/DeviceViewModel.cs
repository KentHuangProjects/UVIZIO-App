using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions.Contracts;

using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;

namespace BLE.Client.ViewModels
{
    public class DeviceViewModel : BaseViewModel
    {
        private static Guid RFduinoService = Guid.ParseExact("aba8a706-f28c-11e6-bc64-92361f002671", "d");
        private static Guid RFduinoWriteCharacteristic = Guid.ParseExact("aba8a708-f28c-11e6-bc64-92361f002671", "d");

        private readonly IUserDialogs _userDialogs;
        private bool _updatesStarted;
        private IDevice _device;

        public IList<IService> Services { get; private set; }
        public DeviceViewModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

        /*
        private async void LoadServices()
        {
            try
            {
                _userDialogs.ShowLoading("Discovering services...");

                Services = await _device.GetServicesAsync();
                RaisePropertyChanged(() => Services);
            }
            catch (Exception ex)
            {
                _userDialogs.Alert(ex.Message, "Error while discovering services");
                Mvx.Trace(ex.Message);
            }
            finally
            {
                _userDialogs.HideLoading();
            }
        }
        */


        /*
        public IService SelectedService
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    var bundle = new MvxBundle(new Dictionary<string, string>(Bundle.Data) { { ServiceIdKey, value.Id.ToString() } });
                    ShowViewModel<CharacteristicListViewModel>(bundle);
                }

                RaisePropertyChanged();

            }
        }
        */

        public ICharacteristic Characteristic { get; private set; }
        public IDevice Device { get; private set; }


        public string CharacteristicValue => Characteristic?.Value.ToHexString().Replace("-", " ");

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public string UpdateButtonText => _updatesStarted ? "Stop updates" : "Start updates";

        public string Permissions
        {
            get
            {
                if (Characteristic == null)
                    return string.Empty;

                return (Characteristic.CanRead ? "Read " : "") +
                       (Characteristic.CanWrite ? "Write " : "") +
                       (Characteristic.CanUpdate ? "Update" : "");
            }
        }



        protected override async void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            //Characteristic = await GetCharacteristicFromBundleAsync(parameters);
            Device = GetDeviceFromBundle(parameters);
            var service = await Device.GetServiceAsync(RFduinoService);
            Characteristic = await service.GetCharacteristicAsync(RFduinoWriteCharacteristic);
        }

        public override void Resume()
        {
            base.Resume();

            if (Characteristic != null)
            {
                return;
            }

            Close(this);
        }

        public MvxCommand ReadCommand => new MvxCommand(ReadValueAsync);


        private async void ReadValueAsync()
        {
            if (Characteristic == null)
                return;

            try
            {
                _userDialogs.ShowLoading("Reading characteristic value...");

                await Characteristic.ReadAsync();

                RaisePropertyChanged(() => CharacteristicValue);

                Messages.Insert(0, $"Read value {CharacteristicValue}");
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                _userDialogs.ShowError(ex.Message);

                Messages.Insert(0, $"Error: {ex.Message}");

            }
            finally
            {
                _userDialogs.HideLoading();
            }

        }

        // ViewModel
        public string WriteText { get; set; }

        public string StaticRed   = "00 00 00 00 FF 00 00";
        public string StaticGreen = "00 00 00 00 00 FF 00";
        public string StaticBlue  = "00 00 00 00 00 00 FF";

        public string BlinkPurple = "01 00 FF 01 FF 00 FF";


        //new MvxCommand
        public MvxCommand<string> InputCommand => new MvxCommand<string>(async param =>
        {
            try
            {
                WriteText = param;

                await Characteristic.WriteAsync(GetBytes(param));
                RaisePropertyChanged(() => CharacteristicValue);
                Messages.Insert(0, $"Wrote value {CharacteristicValue}");
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                _userDialogs.ShowError(ex.Message);
            }
        });




        //new MvxCommand
        public MvxCommand<string> WriteCommandDirect => new MvxCommand<string>(async param =>
        {
            try
            {
                String param2 = WriteText;

                param2 = param2.Replace("Wrote value ", "");
                param2 = param2.Replace("Read value ", "");
                param2 = param2.Trim();
                if (String.IsNullOrEmpty(param2) || Char.IsLetter(param2[0]))
                {
                    throw new Exception("'" + param + "' is invalid for WRITEing!");
                }
                var data = GetBytes(param2);

                //var data = GetBytes(this.FindByName)

                _userDialogs.ShowLoading("Write characteristic value");
                await Characteristic.WriteAsync(data);
                _userDialogs.HideLoading();

                RaisePropertyChanged(() => CharacteristicValue);
                Messages.Insert(0, $"Wrote value {CharacteristicValue}");
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                _userDialogs.ShowError(ex.Message);
            }
        });

        public MvxCommand WriteCommand => new MvxCommand(WriteValueAsync);

    


        private async void WriteValueAsync()
        {
            try
            {



                var result =
                    await
                        _userDialogs.PromptAsync("Input a value (as hex whitespace separated)", "Write value",

                           placeholder: CharacteristicValue);
                // _userDialogs.PromptAsync()



                if (!result.Ok)
                    return;

                var data = GetBytes(result.Text);

                //var data = GetBytes(this.FindByName)

                _userDialogs.ShowLoading("Write characteristic value");
                await Characteristic.WriteAsync(data);
                _userDialogs.HideLoading();

                RaisePropertyChanged(() => CharacteristicValue);
                Messages.Insert(0, $"Wrote value {CharacteristicValue}");
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                _userDialogs.ShowError(ex.Message);
            }

        }





        private static byte[] GetBytes(string text)
        {
            return text.Split(' ').Where(token => !string.IsNullOrEmpty(token)).Select(token => Convert.ToByte(token, 16)).ToArray();
        }

        public MvxCommand ToggleUpdatesCommand => new MvxCommand((() =>
        {
            if (_updatesStarted)
            {
                StopUpdates();
            }
            else
            {
                StartUpdates();
            }
        }));

        private async void StartUpdates()
        {
            try
            {
                _updatesStarted = true;

                Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;
                Characteristic.ValueUpdated += CharacteristicOnValueUpdated;
                await Characteristic.StartUpdatesAsync();


                Messages.Insert(0, $"Start updates");

                RaisePropertyChanged(() => UpdateButtonText);

            }
            catch (Exception ex)
            {
                _userDialogs.ShowError(ex.Message);
            }
        }

        private async void StopUpdates()
        {
            try
            {
                _updatesStarted = false;

                await Characteristic.StopUpdatesAsync();
                Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;

                Messages.Insert(0, $"Stop updates");

                RaisePropertyChanged(() => UpdateButtonText);

            }
            catch (Exception ex)
            {
                _userDialogs.ShowError(ex.Message);
            }
        }

        private void CharacteristicOnValueUpdated(object sender, CharacteristicUpdatedEventArgs characteristicUpdatedEventArgs)
        {
            Messages.Insert(0, $"Updated value: {CharacteristicValue}");
            RaisePropertyChanged(() => CharacteristicValue);
        }

    }
}