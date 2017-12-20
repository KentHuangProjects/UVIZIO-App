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
using Plugin.Settings.Abstractions;
using System.Threading.Tasks;
using BLE.Client.Helpers;

namespace BLE.Client.ViewModels
{
    /*
     * PatternViewModel manages communication between the Mode/Settings pages and the LED driver
     */ 
    public class PatternViewModel : BaseViewModel
    {
        private ISettings _settings;
        private readonly IUserDialogs _userDialogs;
        private bool _updatesStarted;

        /*
         * Constructor for PatternViewModel
         */ 
        public PatternViewModel(IAdapter adapter, IUserDialogs userDialogs, ISettings settings) : base(adapter)
        {
            _userDialogs = userDialogs;
            _settings = settings;

            if (Settings.MODE == null) Settings.MODE = modes[0];
            currentMode = Settings.MODE;
            selectedMode = Settings.MODE;
        }

        /*
         * Initializes the ViewModel
         */
        protected override async void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            IDevice _device = GetDeviceFromBundle(parameters);

            if (_device != null)
            {
                Settings.DEVICE = _device;
                _settings.AddOrUpdateValue("deviceId", _device.Id.ToString());
            }
        }

        public ICharacteristic Characteristic { get; private set; }

        public string CharacteristicValue => Characteristic?.Value.ToHexString().Replace("-", " ");

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public string UpdateButtonText => _updatesStarted ? "Stop updates" : "Start updates";

        /*
         * Returns the granted characteristic permissions
         */ 
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

        /*
         * Represents a menu item on the menu drawer
         */
        public class MasterPageItem
        {
            public string Title { get; set; }
        }

        /*
         * Sets the newly selected menu item
         */
        public MasterPageItem SelectMasterItem
        {
            get { return null; }
            set
            {
                menuNavigate(value.Title);
            }
        }

        /*
         * The list of menu items on the menu drawer
         */
        public List<MasterPageItem> MenuItems { get; set; } = new List<MasterPageItem>
        {
            new MasterPageItem
            {
                Title = "Devices",
            },
            new MasterPageItem
            {
                Title = "Modes",
            },
            new MasterPageItem
            {
                Title = "Settings",
            },
        };

        /*
         * Determines the action to be performed when a menu item is selected
         */
        public void menuNavigate(String title)
        {
            switch (title)
            {
                case "Devices":
                    Close(this);
                    break;
                case "Modes":
                    break;
                case "Settings":
                    break;
            }
        }

        /*
         * Adds the modes to the modes list
         */
        public ObservableCollection<Mode> modes { get; set; }= new ObservableCollection<Mode>
        {
            new Mode("Rainbow", "bg_1.png", "mode_deselected_icon.png",         Mode.RAINBOW),

            new Mode("Color Strobe", "bg_2.png", "mode_deselected_icon.png",    Mode.COLOR_STROBE),

            new Mode("Color Walk", "bg_3.png", "mode_deselected_icon.png",      Mode.COLOR_WALK),

            new Mode("Fire Pixel", "bg_4.png", "mode_deselected_icon.png",      Mode.FIRE_PIXEL),
        };

        /*
         * The currently selected mode
         */ 
        private Mode currentMode { get; set; }

        /*
         * The item to be binded AS:SelectedItem="{Binding selectedMode, Mode=TwoWay}" 
         * in the listview of PatternPage
         */ 
        public Mode selectedMode
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null && currentMode != value && currentMode != null )
                {
                    // sets the selected icon (green circle) for the newly selected mode
                    Pattern_ItemSelected(value);

                    uvizioWriting(value.BleWritingText);

                    RaisePropertyChanged();
                }
            }
        }

        /*
         * Updates the UI representation of which mode is selected (i.e. has the green cirle icon)
         */ 
        private void  Pattern_ItemSelected(Mode selected)
        {
            if (true || selected != currentMode)
            {
                selected.SelectedImageSrc = "mode_selected_icon.png";
                currentMode.SelectedImageSrc = "mode_deselected_icon.png";

                currentMode = selected;
            }
        }

        /*
         * Sends the new mode to BLE device
         */    
        private async void uvizioWriting(string commandtext, bool showErrors = true)
        {
            try
            {
                if (Settings.DEVICE == null)
                {
                    Close(this);
                }

                string orig = _settings.GetValueOrDefault<string>("lastcommand", null);
                _settings.AddOrUpdateValue("lastcommand", commandtext);

                if (orig == null) return;

                var diag = showErrors ? _userDialogs : null;
                await UVIZIO.writeDevice(Settings.DEVICE, diag, commandtext);

                RaisePropertyChanged(() => CharacteristicValue);
                
            }
            catch (Exception ex)
            {
                if (showErrors)
                {
                    _userDialogs.HideLoading();
                    _userDialogs.ShowError(ex.Message);
                }
            }
        }

        public override void Resume()
        {
            base.Resume();
        }


        /*
         * Reads the BLE characteristics
         */ 
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

        /*
         * Writes the BLE characteristics
         */
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

        // ViewModel
        public string WriteText { get; set; }
        
        /*
         * new MvxCommand
         */
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

        public MvxCommand ReadCommand => new MvxCommand(ReadValueAsync);

        public MvxCommand WriteCommand => new MvxCommand(WriteValueAsync);

        /*
         * Parse the byte string and return it as an array
         */ 
        private static byte[] GetBytes(string text)
        {
            while (text.Split(' ').Length < 3)
                text += " 00";

            return text.Split(' ').Where(token => !string.IsNullOrEmpty(token)).Select(token => Convert.ToByte(token, 16)).ToArray();
        }

        /*
         * The speed of the LED pattern
         */
        public int Speed
        {
            get { return Settings.SPEED; }
            set
            {
                Settings.SPEED = value;
                _settings.AddOrUpdateValue("speed_pct", value);

                var last = Settings.LAST_COMMAND;
                if(last != null) uvizioWriting(last, false);

                RaisePropertyChanged();
            }
        }

        /*
         * The brightess of the LEDs
         */
        public int Brightness
        {
            get { return Settings.BRIGHTNESS; }
            set
            {
                Settings.BRIGHTNESS = value;
                _settings.AddOrUpdateValue("brightness_pct", value);

                var last = Settings.LAST_COMMAND;
                if (last != null) uvizioWriting(last, false);

                RaisePropertyChanged();
            }
        }        
    }
}
