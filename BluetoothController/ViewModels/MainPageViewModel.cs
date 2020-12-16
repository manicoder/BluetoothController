using BluetoothController.Models;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothController.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        IBluetoothLE ble = CrossBluetoothLE.Current;
        public MainPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Main Page";
            Devices = new ObservableCollection<IDevice>();
            //1 . init
            InitBle();


        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            //2. scan devices
            ScanDevices();
        }
        public void InitBle()
        {
            ble.StateChanged += Ble_StateChanged;
            //
            CurrentState = ble.State.ToString();

            //
        }

        private void Ble_StateChanged(object sender, Plugin.BLE.Abstractions.EventArgs.BluetoothStateChangedArgs e)
        {
            CurrentState = $"The bluetooth state changed to {e.NewState}";
        }

        public async void ScanDevices()
        {
            adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            await adapter.StartScanningForDevicesAsync();
        }

        private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            Devices.Add(e.Device);
        }

        public async void StartDeviceSequence(IDevice device)
        {
            try
            {
                await adapter.ConnectToDeviceAsync(device);
                var services = await GetServices(device);
                var characterstics = await GetCharacterstics(services.FirstOrDefault());

                ///read charactestics
                ///
                var characteristic = characterstics.FirstOrDefault();
                var bytes = await characteristic.ReadAsync();
                await characteristic.WriteAsync(bytes);


                characteristic.ValueUpdated += (o, args) =>
                {
                    var bytes1 = args.Characteristic.Value;
                };

                await characteristic.StartUpdatesAsync();



                //read descriptitor 
                var descriptors = await characteristic.GetDescriptorsAsync();
                var descriptor = descriptors.FirstOrDefault();
                var bytes2 = await descriptor.ReadAsync();
                await descriptor.WriteAsync(bytes2);


            }
            catch (DeviceConnectionException e)
            {
                // ... could not connect to device
            }
        }


        public async void ConnectDevice(IDevice device)
        {
            try
            {
                await adapter.ConnectToDeviceAsync(device);
            }
            catch (DeviceConnectionException e)
            {
                // ... could not connect to device
            }
        }

        public async void ConnectDeviceByGuid(Guid guid)
        {
            try
            {
                await adapter.ConnectToKnownDeviceAsync(guid);
            }
            catch (DeviceConnectionException e)
            {
                // ... could not connect to device
            }
        }

        public async Task<IReadOnlyList<IService>> GetServices(IDevice connectedDevice)
        {
            var services = await connectedDevice.GetServicesAsync();
            return services;
        }

        public async Task<IReadOnlyList<ICharacteristic>> GetCharacterstics(IService service)
        {
            var characteristics = await service.GetCharacteristicsAsync();
            return characteristics;
        }

        public async void GetCharacterstic(IService service, Guid id)
        {
            var characteristic = await service.GetCharacteristicAsync(id);

        }


        private ObservableCollection<IDevice> mDevices;
        public ObservableCollection<IDevice> Devices
        {
            get { return mDevices; }
            set
            {
                mDevices = value;
                RaisePropertyChanged();
            }
        }

        private string mCurrentState;
        public string CurrentState
        {
            get { return mCurrentState; }
            set
            {
                mCurrentState = value;
                RaisePropertyChanged();
            }
        }

    }
}
