using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Acr.UserDialogs;
using System.Collections.ObjectModel;
using Plugin.Settings.Abstractions;
using Plugin.BLE.Abstractions.Extensions;
using MvvmCross.Core.ViewModels;
using System.Threading;
using Plugin.BLE.Abstractions;
using BLE.Client.Helpers;

namespace BLE.Client.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        
        public MasterPageItem SelectMasterItem
        {
            get { return null; }
            set
            {
                menuNavigate(value.Title);
            }
        }

        public void menuNavigate(String title)
        {
            switch (title)
            {
                case "Devices":
                    ShowViewModel<DeviceListViewModel>(new MvxBundle(new Dictionary<string, string> { { DeviceIdKey, Settings.DEVICE?.Id.ToString() } }));
                    break;
                case "Modes":
                    ShowViewModel<PatternViewModel>(new MvxBundle(new Dictionary<string, string> { { DeviceIdKey, Settings.DEVICE?.Id.ToString() } }));
                    break;
                case "Settings":
                    
                    break;
            }
        }


        //the master  items
        public class MasterPageItem
        {
            public string Title { get; set; }

            //public string IconSource { get; set; }

            //public Type TargetType { get; set; }



        }

        public List<MasterPageItem> MenuItems { get; set; } = new List<MasterPageItem>
            {
                new MasterPageItem
                {
                    Title = "Devices",
                    //IconSource = "todo.png",
                    //TargetType = typeof(MainPage)
                },
                new MasterPageItem
                {
                    Title = "Modes",
                    //IconSource = "todo.png",
                   // TargetType = typeof(TabbedPageModeAndAdjustment)
                },
                new MasterPageItem
                {
                    Title = "Settings",
                },
            };
        //the master  items
       
        private ISettings _settings;
        private byte _speedPct;
        private byte _brightnessPct;


        private readonly IUserDialogs _userDialogs;
        private bool _updatesStarted;
        public ICharacteristic Characteristic { get; private set; }

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



       



        public SettingsViewModel(IAdapter adapter, IUserDialogs userDialogs, ISettings settings) : base(adapter)
        {
            _userDialogs = userDialogs;
            _settings = settings;

            _brightnessPct = Settings.BRIGHTNESS;
            _speedPct = Settings.SPEED;

            //ConnectToPreviousDeviceAsync();



        }

        private async void ConnectToPreviousDeviceAsync()
        {
            //IDevice device;
            var guidString = _settings.GetValueOrDefault<string>("lastguid", null);
            var PreviousGuid = !string.IsNullOrEmpty(guidString) ? Guid.Parse(guidString) : Guid.Empty;
            try
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();


                Settings.DEVICE = await Adapter.ConnectToKnownDeviceAsync(PreviousGuid, tokenSource.Token);
                //await Adapter.ConnectToDeviceAsync(device.Device, tokenSource.Token);
            }
            catch (Exception ex)
            {
                _userDialogs.ShowError(ex.Message, 5000);
                return;
            }
        }

        private bool CanConnectToPrevious()
        {
            var guidString = _settings.GetValueOrDefault<string>("lastguid", null);
            var PreviousGuid = !string.IsNullOrEmpty(guidString) ? Guid.Parse(guidString) : Guid.Empty;
            return PreviousGuid != default(Guid) && PreviousGuid != null;
        }

        /*
        //the item to be binded AS:SelectedItem="{Binding selectedMode, Mode=TwoWay}" in listview of patternpage
        public Mode selectedMode
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                {
                    //call the function to change the icon(selection)
                    Pattern_ItemSelected(value);

                    uvizioWriting(value.BleWritingText);

                    RaisePropertyChanged();
                }
            }
        }
        */

           /*
        private void Pattern_ItemSelected(Mode selected)
        {

            if (selected != currentMode)
            {
                selected.SelectedImageSrc = "mode_selected_icon.png";
                currentMode.SelectedImageSrc = "mode_deselected_icon.png";

                currentMode = selected;
            }

        }
        */
        //function to send different modes to BLE device
        private async void uvizioWriting(string commandtext)
        {
            try
            {
                if (Settings.DEVICE == null)
                {
                    return;
                    //Close(this);
                }

                _settings.AddOrUpdateValue("lastcommand", commandtext);


                var service = await Settings.DEVICE.GetServiceAsync(KnownServices.RFDUINO_SERVICE);
                Characteristic = await service.GetCharacteristicAsync(KnownCharacteristics.RFDUINO_WRITE);

                var data = GetBytes(commandtext);

                int period = (_speedPct) * 1000;
                data[1] = (byte)period;
                data[2] = (byte)(period >> 8);


                _userDialogs.ShowLoading("Setting " + commandtext);
                await Characteristic.WriteAsync(data);
                _userDialogs.HideLoading();

                RaisePropertyChanged(() => CharacteristicValue);

            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                _userDialogs.ShowError(ex.Message);
            }
        }

        protected override async void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
            return;
            var tmp = GetDeviceFromBundle(parameters);
            if(tmp != null)
            {
                _userDialogs.ShowSuccess("Connected");
                Settings.DEVICE = tmp;
            } else
            {
                _userDialogs.ShowError("Unable to connect");
            }
            

            //TODO when sending data, validate
            //if (_device == null)
            //{
            //    Close(this);
            //}
            //var service = await _device.GetServiceAsync(RFduinoService);
            //Characteristic = await service.GetCharacteristicAsync(RFduinoWriteCharacteristic);
        }

        public override void Resume()
        {
            base.Resume();

            //TODO when sending data, validate

            //if (Characteristic != null)
            //{
            //    return;
            //}

            //Close(this);
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

                Messages.Insert(0, $"Error {ex.Message}");

            }
            finally
            {
                _userDialogs.HideLoading();
            }

        }

        // ViewModel
        public string WriteText { get; set; }



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

        public MvxCommand WriteCommand => new MvxCommand(WriteValueAsync);

        private async void WriteValueAsync()
        {
            try
            {
                var result =
                    await
                        _userDialogs.PromptAsync("Input a value (as hex whitespace separated)", "Write value",
                            placeholder: CharacteristicValue);

                if (!result.Ok)
                    return;

                var data = GetBytes(result.Text);

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
            while (text.Split(' ').Length < 3)
                text += " 00";

            return text.Split(' ').Where(token => !string.IsNullOrEmpty(token)).Select(token => Convert.ToByte(token, 16)).ToArray();
        }



        public byte Speed
        {
            get { return _speedPct; }
            set
            {
                //L.Info("PatternViewModel", "Setting speed to "+_speedPct);

                _speedPct = value;
                //_settings.AddOrUpdateValue("speed_pct", _speedPct);
                Settings.SPEED = _speedPct;

                //var last = _settings.GetValueOrDefault<string>("lastcommand", null);
                //if (last != null) uvizioWriting(last);

                RaisePropertyChanged();
            }
        }

        public byte Brightness
        {
            get { return _brightnessPct; }
            set
            {
                _brightnessPct = value;
                //_settings.AddOrUpdateValue("brightness_pct", _brightnessPct);
                Settings.BRIGHTNESS = _brightnessPct;

                //var last = _settings.GetValueOrDefault<string>("lastcommand", null);
                //if (last != null) uvizioWriting(last);

                RaisePropertyChanged();
            }
        }

        //public MvxCommand ToggleUpdatesCommand => new MvxCommand((() =>
        //{
        //    if (_updatesStarted)
        //    {
        //        StopUpdates();
        //    }
        //    else
        //    {
        //        StartUpdates();
        //    }
        //}));

        //private async void StartUpdates()
        //{
        //    try
        //    {
        //        _updatesStarted = true;

        //        Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;
        //        Characteristic.ValueUpdated += CharacteristicOnValueUpdated;
        //        await Characteristic.StartUpdatesAsync();


        //        Messages.Insert(0, $"Start updates");

        //        RaisePropertyChanged(() => UpdateButtonText);

        //    }
        //    catch (Exception ex)
        //    {
        //        _userDialogs.ShowError(ex.Message);
        //    }
        //}

        //private async void StopUpdates()
        //{
        //    try
        //    {
        //        _updatesStarted = false;

        //        await Characteristic.StopUpdatesAsync();
        //        Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;

        //        Messages.Insert(0, $"Stop updates");

        //        RaisePropertyChanged(() => UpdateButtonText);

        //    }
        //    catch (Exception ex)
        //    {
        //        _userDialogs.ShowError(ex.Message);
        //    }
        //}

        //private void CharacteristicOnValueUpdated(object sender, CharacteristicUpdatedEventArgs characteristicUpdatedEventArgs)
        //{
        //    Messages.Insert(0, $"Updated value: {CharacteristicValue}");
        //    RaisePropertyChanged(() => CharacteristicValue);
        //}
    }
}
