using BLE.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public partial class MasterPage : ContentPage
    {
        public MasterPage()
        {
            InitializeComponent();

            listView.ItemsSource = new List<MasterPageItem> {
                new MasterPageItem
                {
                    Title = "Connection",
                    //IconSource = "todo.png",
                    TargetType = typeof(DeviceListPage)
                },
                    new MasterPageItem
                {
                    Title = "Mode",
                    //IconSource = "todo.png",
                    TargetType = typeof(DeviceListPage)
                },
                    new MasterPageItem
                {
                    Title = "Adjustment",
                    //IconSource = "todo.png",
                    TargetType = typeof(DeviceListPage)
                }
            };
        }
    }
}
