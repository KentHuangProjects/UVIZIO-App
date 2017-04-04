﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using System.Collections.Generic;
using Xamarin.Forms;
using Plugin.Settings.Abstractions;
using System.Threading.Tasks;

namespace BLE.Client.ViewModels
{
    public class PatternViewModel : BaseViewModel
    {
        public static string MODE_OFF = "00";
        public static string MODE_STATIC = "01";
        public static string MODE_BLINK = "02";
        public static string MODE_FADE = "03";
        public static string MODE_FRAMES = "04";
        public static string MODE_RAINBOW = "05";
        public static string MODE_COLOR_STROBE = "06";
        public static string MODE_COLOR_WALK = "07";
        public static string MODE_FIRE_PIXEL = "08";

        public static string STATIC_RED =   MODE_STATIC + " 00 00 00 FF 00 00";
        public static string STATIC_GREEN = MODE_STATIC + " 00 00 00 00 FF 00";
        public static string STATIC_BLUE =  MODE_STATIC + " 00 00 00 00 00 FF";

        public static string BLINK_PURPLE = MODE_BLINK  + " 00 FF 01 FF 00 FF";

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
                    ShowViewModel<DeviceListViewModel>(new MvxBundle(new Dictionary<string, string> { { DeviceIdKey, _device?.Id.ToString() } }));
                    break;
                case "Modes":
                    break;
                case "Settings":
                    ShowViewModel<SettingsViewModel>(new MvxBundle(new Dictionary<string, string> { { DeviceIdKey, _device?.Id.ToString() } }));
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


        private static Guid RFduinoService = Guid.ParseExact("aba8a706-f28c-11e6-bc64-92361f002671", "d");
        private static Guid RFduinoWriteCharacteristic = Guid.ParseExact("aba8a708-f28c-11e6-bc64-92361f002671", "d");

        private IDevice _device;

        private ISettings _settings;
        private int _speedPct = 200;            // out of 255 not 100 
        private int _brightnessPct = 250;


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

        

        public ObservableCollection<Mode> modes { get; set; }= new ObservableCollection<Mode>
            {
                new Mode("Rainbow", "bg_1.png", "mode_selected_icon.png",           MODE_RAINBOW ),

                new Mode("Colou Strobe", "bg_2.png", "mode_deselected_icon.png",    MODE_COLOR_STROBE ),

                new Mode("Colou Walk", "bg_3.png", "mode_deselected_icon.png",      MODE_COLOR_WALK),

                new Mode("Fire Pixel", "bg_4.png", "mode_deselected_icon.png",      MODE_FIRE_PIXEL),
            };
        private Mode currentMode { get; set; }


        
            public PatternViewModel(IAdapter adapter, IUserDialogs userDialogs, ISettings settings) : base(adapter)
        {
            _userDialogs = userDialogs;
            _settings = settings;

            //_brightnessPct = _settings.GetValueOrDefault("brightness_pct", 100);
            //int speed = _settings.GetValueOrDefault("speed_pct", 50);
            //_speedPct = speed;


            currentMode = modes.ElementAt(0);
            
        }

        //the item to be binded AS:SelectedItem="{Binding selectedMode, Mode=TwoWay}" in listview of patternpage
        public Mode selectedMode
        {
            get
            {
                return null;
            }
            set
            {
                if(value!=null && currentMode != value)
                {
                    //call the function to change the icon(selection)
                    Pattern_ItemSelected(value);

                    uvizioWriting(value.BleWritingText);

                    RaisePropertyChanged();
                }
            }
        }



        private void  Pattern_ItemSelected(Mode selected)
        {

            if (selected != currentMode)
            {
                selected.SelectedImageSrc = "mode_selected_icon.png";
                currentMode.SelectedImageSrc = "mode_deselected_icon.png";

                currentMode = selected;
            }

        }
        //function to send different modes to BLE device
        private async void uvizioWriting(string commandtext)
        {
            try
            {
                if (_device == null)
                {
                    Close(this);
                }

                string orig = _settings.GetValueOrDefault<string>("lastcommand", null);

                _settings.AddOrUpdateValue("lastcommand", commandtext);

                if (orig == null) return;

                var service = await _device.GetServiceAsync(RFduinoService);
                Characteristic = await service.GetCharacteristicAsync(RFduinoWriteCharacteristic);

                var data = GetBytes(commandtext);

                /*
                int period = (_speedPct) * 1000;
                data[1] = (byte)period;
                data[2] = (byte)(period >> 8);
                */

                // not actually a pct 
                data[1] = (byte)_speedPct;
                data[2] = (byte)_brightnessPct;


                _userDialogs.ShowLoading("Setting "+data.ToHexString());
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

            _device = GetDeviceFromBundle(parameters);

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


        
        public int Speed
        {
            get { return _speedPct; }
            set
            {
                //L.Info("PatternViewModel", "Setting speed to "+_speedPct);

                _speedPct = value;
                _settings.AddOrUpdateValue("speed_pct", _speedPct);

                var last = _settings.GetValueOrDefault<string>("lastcommand", null);
                if(last != null) uvizioWriting(last);

                RaisePropertyChanged();
            }
        }

        public int Brightness
        {
            get { return _brightnessPct; }
            set
            {
                _brightnessPct = value;
                _settings.AddOrUpdateValue("brightness_pct", _brightnessPct);

                var last = _settings.GetValueOrDefault<string>("lastcommand", null);
                if (last != null) uvizioWriting(last);

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