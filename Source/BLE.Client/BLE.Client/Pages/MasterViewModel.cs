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
using Xamarin.Forms;

namespace BLE.Client.ViewModels
{
    public class MasterViewModel : BaseViewModel
    {

        private readonly IUserDialogs _userDialogs;
        public ListView listView { get { return listView; } }

        public MasterViewModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }





    }
}