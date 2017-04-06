using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static BLE.Client.ViewModels.PatternViewModel;

namespace BLE.Client.Pages
{

    public partial class PatternPage 
    {
        

        public PatternPage()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

           
            //ModeList.SelectedItem = modes.ElementAt(0);

           
        }

        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var tabPage = this.FindByName<BaseCarouselPage>("BaseCarouselPage");
            Debug.WriteLine(e.SelectedItem.GetType().ToString());
            MasterPageItem mt = (MasterPageItem)e.SelectedItem;
            string t = mt.Title;
            if (t == "Modes")
                tabPage.Title = "Modes";
                tabPage.CurrentPage = tabPage.Children[0];
                this.IsPresented = false;
            if (t == "Settings")
            {
                tabPage.Title = "Settings";
                tabPage.CurrentPage = tabPage.Children[0];
                this.IsPresented = false;
            }
                



        }
    }
}
