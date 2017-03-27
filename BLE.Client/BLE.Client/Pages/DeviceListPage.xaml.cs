using System.Collections.Generic;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class DeviceListPage
    {
        

        public DeviceListPage()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
            
        }

        
    }
}
