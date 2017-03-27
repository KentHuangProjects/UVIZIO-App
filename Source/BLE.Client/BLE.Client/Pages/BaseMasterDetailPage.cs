using BLE.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BLE.Client.Pages
{
    public class BaseMasterDetailPage: MasterDetailPage
    {
        protected override void OnAppearing()
        {
            base.OnAppearing();

            var viewModel = BindingContext as BaseViewModel;
            viewModel?.Resume();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var viewModel = BindingContext as BaseViewModel;
            viewModel?.Suspend();
        }
    }
}
