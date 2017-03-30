using System;
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

namespace BLE.Client.ViewModels
{
    public class PatternViewModel : BaseViewModel
    {

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
            };
        //the master  items


        private static Guid RFduinoService = Guid.ParseExact("aba8a706-f28c-11e6-bc64-92361f002671", "d");
        private static Guid RFduinoWriteCharacteristic = Guid.ParseExact("aba8a708-f28c-11e6-bc64-92361f002671", "d");

        private IDevice _device;

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

        public string StaticRed = "00 00 00 00 FF 00 00";
        public string StaticGreen = "00 00 00 00 00 FF 00";
        public string StaticBlue = "00 00 00 00 00 00 FF";

        public string BlinkPurple = "01 00 FF 01 FF 00 FF";

        public ObservableCollection<Mode> modes { get; set; }= new ObservableCollection<Mode>
            {
                new Mode("Rainbow", "bg_1.png", "mode_selected_icon.png","00 00 00 00 FF 00 00" ),

                new Mode("Colou Strobe", "bg_2.png", "mode_deselected_icon.png","00 00 00 00 00 FF 00"),

                new Mode("Colou Walk", "bg_3.png", "mode_deselected_icon.png","00 00 00 00 00 00 FF"),

                new Mode("Fire Pixel", "bg_4.png", "mode_deselected_icon.png","01 00 FF 01 FF 00 FF"),
            };
        private Mode currentMode { get; set; }

        public PatternViewModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
            


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
                if(value!=null)
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
                //if (_device == null)
                //{
                //    Close(this);
                //}
                var service = await _device.GetServiceAsync(RFduinoService);
                Characteristic = await service.GetCharacteristicAsync(RFduinoWriteCharacteristic);
                var data = GetBytes(commandtext);

                _userDialogs.ShowLoading("Write characteristic value");
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
            return text.Split(' ').Where(token => !string.IsNullOrEmpty(token)).Select(token => Convert.ToByte(token, 16)).ToArray();
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