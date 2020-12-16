
using BluetoothController.ViewModels;
using Plugin.BLE.Abstractions.Contracts;

namespace BluetoothController.Views
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            var context = this.BindingContext as MainPageViewModel;
            if (context != null)
            {
                var item = e.Item as IDevice;
                context.StartDeviceSequence(item);
            }
        }
    }
}
