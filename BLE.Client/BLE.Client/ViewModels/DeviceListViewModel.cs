using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using BLE.Client.Extensions;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.Settings.Abstractions;
using BLE.Client.Helpers;
using Xamarin.Forms;

namespace BLE.Client.ViewModels
{
    /*
     * DeviceListViewModel manages the BLE connection with the LED driver
     */
    public class DeviceListViewModel : BaseViewModel
    {
        private readonly IBluetoothLE _bluetoothLe;
        private readonly IUserDialogs _userDialogs;
        private readonly ISettings _settings;
        private Guid _previousGuid;
        private string _previousName;
        private CancellationTokenSource _cancellationTokenSource;

        /*
         * Constructor for DeviceListViewModel
         */
        public DeviceListViewModel(IBluetoothLE bluetoothLe, IAdapter adapter, IUserDialogs userDialogs, ISettings settings) : base(adapter)
        {
            _bluetoothLe = bluetoothLe;
            _userDialogs = userDialogs;
            _settings = settings;
            // quick and dirty :>

            _bluetoothLe.StateChanged -= OnStateChanged;
            _bluetoothLe.StateChanged += OnStateChanged;
            Adapter.DeviceDiscovered -= OnDeviceDiscovered;
            Adapter.DeviceDiscovered += OnDeviceDiscovered;
            Adapter.ScanTimeoutElapsed -= Adapter_ScanTimeoutElapsed;
            Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            Adapter.DeviceDisconnected -= OnDeviceDisconnected;
            Adapter.DeviceDisconnected += OnDeviceDisconnected;
            Adapter.DeviceConnectionLost -= OnDeviceConnectionLost;
            Adapter.DeviceConnectionLost += OnDeviceConnectionLost;

            Task.Run(async () =>
            {
                await Task.Delay(2000);

                if (_bluetoothLe.State == BluetoothState.On) TryStartScanning(true);

                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _userDialogs.Toast("Pull down to scan", TimeSpan.FromMilliseconds(6000));
                    });
                }
            });
        }

        /*
         * Initializes the ViewModel
         */
        protected override async void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            Settings.DEVICE = GetDeviceFromBundle(parameters);
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
            switch(title)
            {
                case "Devices":
                    break;
                case "Modes":
                    ShowViewModel<PatternViewModel>(new MvxBundle(new Dictionary<string, string> { { DeviceIdKey, Settings.DEVICE?.Id.ToString() } }));
                    break;
                case "Settings":
                    break;
            }
        }        

        /*
         * Gets and sets the previous device
         */ 
        public Guid PreviousGuid
        {
            get { return _previousGuid; }
            set
            {
                _previousGuid = value;
                _settings.AddOrUpdateValue("lastguid", _previousGuid.ToString());
                RaisePropertyChanged();
                RaisePropertyChanged(() => ConnectToPreviousCommand);
            }
        }

        /*
         * Gets and sets the name of the previous device
         */ 
        public string PreviousName
        {
            get { return _previousName; }
            set
            {
                _previousName = value;
                _settings.AddOrUpdateValue("lastname", _previousName);
                RaisePropertyChanged();
                RaisePropertyChanged(() => ConnectToPreviousCommand);
                RaisePropertyChanged(() => ReconnectPreviousName);
            }
        }

        /*
         * Returns whether a connection can be made with the previous device
         */ 
        public string ReconnectPreviousName
        {
            get {
                if (CanConnectToPrevious())
                    return "Reconnect to " + _previousName;
                else
                    return "No Previous Device";
            }
        }

        public MvxCommand RefreshCommand => new MvxCommand(() => TryStartScanning(true));

        public MvxCommand<DeviceListItemViewModel> DisconnectCommand => 
            new MvxCommand<DeviceListItemViewModel>(DisconnectDevice);

        public MvxCommand<DeviceListItemViewModel> ConnectDisposeCommand => 
            new MvxCommand<DeviceListItemViewModel>(ConnectAndDisposeDevice);

        public ObservableCollection<DeviceListItemViewModel> Devices { get; set; } = 
            new ObservableCollection<DeviceListItemViewModel>();

        public bool IsRefreshing => Adapter.IsScanning;
        public bool IsStateOn => _bluetoothLe.IsOn;
        public string StateText => GetStateText();

        public DeviceListItemViewModel SelectedDevice
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    HandleSelectedDevice(value);
                }

                RaisePropertyChanged();
            }
        }

        public MvxCommand StopScanCommand => new MvxCommand(() =>
        {
            _cancellationTokenSource.Cancel();
            CleanupCancellationToken();
            RaisePropertyChanged(() => IsRefreshing);
        }, () => _cancellationTokenSource != null); 

        private Task GetPreviousGuidAsync()
        {
            return Task.Run(() =>
            {
                var guidString = _settings.GetValueOrDefault<string>("lastguid", null);
                PreviousGuid = !string.IsNullOrEmpty(guidString) ? Guid.Parse(guidString) : Guid.Empty;

                var nameString = _settings.GetValueOrDefault<string>("lastname", null);
                PreviousName = !string.IsNullOrEmpty(nameString) ? nameString : null;
            });
        }

        private void OnDeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            Settings.DEVICE = null;
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();

            _userDialogs.HideLoading();
            _userDialogs.ErrorToast("Error", $"Lost connection to {e.Device.Name}", TimeSpan.FromMilliseconds(6000));
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            RaisePropertyChanged(nameof(IsStateOn));
            RaisePropertyChanged(nameof(StateText));
        }

        /*
         * Returns the current BLE state as a string
         */ 
        private string GetStateText()
        {
            switch (_bluetoothLe.State)
            {
                case BluetoothState.Unknown:
                    return "Unknown BLE state.";
                case BluetoothState.Unavailable:
                    return "BLE is not available on this device.";
                case BluetoothState.Unauthorized:
                    return "You are not allowed to use BLE.";
                case BluetoothState.TurningOn:
                    return "BLE is warming up, please wait.";
                case BluetoothState.On:
                    return "BLE is on.";
                case BluetoothState.TurningOff:
                    return "BLE is turning off. That's sad!";
                case BluetoothState.Off:
                    return "BLE is off. Turn it on!";
                default:
                    return "Unknown BLE state.";
            }
        }

        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => IsRefreshing);

            CleanupCancellationToken();
        }

        private void OnDeviceDiscovered(object sender, DeviceEventArgs args)
        {
            AddOrUpdateDevice(args.Device);
        }

        private void AddOrUpdateDevice(IDevice device)
        {
            InvokeOnMainThread(() =>
            {
                var vm = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
                if (vm != null)
                {
                    vm.Update();
                }
                else
                {
                    Devices.Add(new DeviceListItemViewModel(device));
                }
            });
        }

        public override async void Resume()
        {
            base.Resume();

            await GetPreviousGuidAsync();
            GetSystemConnectedOrPairedDevices();
        }  

        private void GetSystemConnectedOrPairedDevices()
        {
            try
            {
                //heart rate
                var guid = Guid.Parse("0000180d-0000-1000-8000-00805f9b34fb");

                SystemDevices = Adapter.GetSystemConnectedOrPairedDevices(new[] { guid }).Select(d => new DeviceListItemViewModel(d)).ToList();
                RaisePropertyChanged(() => SystemDevices);
            }
            catch (Exception ex)
            {
                Trace.Message("Failed to retreive system connected devices. {0}", ex.Message);
            }
        }

        public List<DeviceListItemViewModel> SystemDevices { get; private set; }

        public override void Suspend()
        {
            base.Suspend();

            Adapter.StopScanningForDevicesAsync();
            RaisePropertyChanged(() => IsRefreshing);
        }

        private void TryStartScanning(bool refresh = false)
        {
            if (Settings.DEVICE != null) return;

            if (IsStateOn && (refresh || !Devices.Any()) && !IsRefreshing)
            {
                ScanForDevices();
            }
        }

        private async void ScanForDevices()
        {
            Devices.Clear();

            foreach (var connectedDevice in Adapter.ConnectedDevices)
            {
                //update rssi for already connected evices (so tha 0 is not shown in the list)
                try
                {
                    await connectedDevice.UpdateRssiAsync();
                }
                catch (Exception ex)
                {
                    Mvx.Trace(ex.Message);
                    _userDialogs.ShowError($"Failed to update RSSI for {connectedDevice.Name}");
                }

                AddOrUpdateDevice(connectedDevice);
            }

            _cancellationTokenSource = new CancellationTokenSource();
            RaisePropertyChanged(() => StopScanCommand);

            await Adapter.StartScanningForDevicesAsync(_cancellationTokenSource.Token);
            RaisePropertyChanged(() => IsRefreshing);
        }

        private void CleanupCancellationToken()
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            RaisePropertyChanged(() => StopScanCommand);
        }

        private async void DisconnectDevice(DeviceListItemViewModel device)
        {
            try
            {
                if (!device.IsConnected)
                    return;

                await UVIZIO.disconnectDevice(device, _userDialogs);
            }
            catch (Exception ex)
            {
                _userDialogs.Alert("Unable to Disconnect", "Disconnect error");
            }
            finally
            {
                device.Update();
                _userDialogs.HideLoading();
            }
        }

        private async void HandleSelectedDevice(DeviceListItemViewModel device)
        {
            if (await ConnectDeviceAsync(device))
            {
                openDevice(device.Device.Id);
            }
        }

        private void openDevice(Guid deviceId) {
            ShowViewModel<PatternViewModel>(new MvxBundle(new Dictionary<string, string> { { DeviceIdKey, deviceId.ToString() } }));
        }

        private async Task<bool> ConnectDeviceAsync(DeviceListItemViewModel device, bool showPrompt = true)
        {
            try
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                var config = new ProgressDialogConfig()
                {
                    Title = $"Connecting to '{device.Name}'",
                    CancelText = "Cancel",
                    IsDeterministic = false,
                    OnCancel = tokenSource.Cancel
                };

                using (var progress = _userDialogs.Progress(config))
                {
                    progress.Show();

                    await Adapter.ConnectToDeviceAsync(device.Device, tokenSource.Token);
                }

                _userDialogs.ShowSuccess($"Connected to {device.Device.Name}.");

                PreviousGuid = device.Device.Id;
                PreviousName = device.Name;
                return true;

            }
            catch (Exception ex)
            {
                _userDialogs.Alert("Could not connect to "+device.Name, "Connection error");
                Mvx.Trace(ex.Message);
                return false;
            }
            finally
            {
                _userDialogs.HideLoading();
                device.Update();
            }
        }

        public MvxCommand ConnectToPreviousCommand => new MvxCommand(ConnectToPreviousDeviceAsync, CanConnectToPrevious);

        private async void ConnectToPreviousDeviceAsync()
        {
            IDevice device;
            try
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                var config = new ProgressDialogConfig()
                {
                    Title = $"Searching for '{PreviousName}'",
                    CancelText = "Cancel",
                    IsDeterministic = false,
                    OnCancel = tokenSource.Cancel
                };

                using (var progress = _userDialogs.Progress(config))
                {
                    progress.Show();

                    device = await Adapter.ConnectToKnownDeviceAsync(PreviousGuid, tokenSource.Token);

                }

                _userDialogs.ShowSuccess($"Connected to {device.Name}.");
                openDevice(device.Id);

                var deviceItem = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
                if (deviceItem == null)
                {
                    deviceItem = new DeviceListItemViewModel(device);
                    Devices.Add(deviceItem);
                }
                else
                {
                    deviceItem.Update(device);
                }
                
            }
            catch (Exception ex)
            {
                _userDialogs.ShowError($"Couldn't connect to {PreviousName}.", 5000);
                return;
            }
        }

        /*
         * Returns whether a connection can be made with the previous device
         */ 
        private bool CanConnectToPrevious()
        {
            return PreviousGuid != default(Guid) && PreviousGuid != null && PreviousName != null && PreviousName != "";
        }

        private async void ConnectAndDisposeDevice(DeviceListItemViewModel item)
        {
            try
            {
                using (item.Device)
                {
                    _userDialogs.ShowLoading($"Connecting to {item.Name} ...");
                    await Adapter.ConnectToDeviceAsync(item.Device);
                    item.Update();
                    _userDialogs.ShowSuccess($"Connected {item.Device.Name}");

                    _userDialogs.HideLoading();
                    for (var i = 5; i >= 1; i--)
                    {
                        _userDialogs.ShowLoading($"Disconnect in {i}s...");

                        await Task.Delay(1000);

                        _userDialogs.HideLoading();
                    }
                }
            }
            catch (Exception ex)
            {
                _userDialogs.Alert("Could not connect", "Failed to connect and dispose.");
            }
            finally
            {
                _userDialogs.HideLoading();
            }
        }

        private void OnDeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();
            _userDialogs.HideLoading();
            _userDialogs.Toast($"Disconnected {e.Device.Name}");
        }

        public MvxCommand<DeviceListItemViewModel> CopyGuidCommand => new MvxCommand<DeviceListItemViewModel>(device =>
        {
            PreviousGuid = device.Id;
        });
    }
}
