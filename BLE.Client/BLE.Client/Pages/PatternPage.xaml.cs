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
        BaseCarouselPage tabPage;

        public PatternPage()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            tabPage = this.FindByName<BaseCarouselPage>("BaseCarouselPage");

            tabPage.CurrentPageChanged += CurrentPageChanged;
        }

        /*
         * Navigate to the settings page
         */ 
        private void goingToSetting(object sender, System.EventArgs e) {
            tabPage.CurrentPage = tabPage.Children[1];
        }

        /*
         * Alternate between the Modes and Settings pages
         */ 
        private void CurrentPageChanged(object sender, System.EventArgs e)
        {
            Debug.WriteLine(e.GetType().ToString());
            var tabPage = this.FindByName<BaseCarouselPage>("BaseCarouselPage");
            if(tabPage.Title == "Modes")
            {
                tabPage.Title = "Settings";
            } else if(tabPage.Title == "Settings")
            {
                tabPage.Title = "Modes";
            }
        }

        /*
         * Navigate to the page selected from the menu drawer
         */ 
        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            tabPage = this.FindByName<BaseCarouselPage>("BaseCarouselPage");
            Debug.WriteLine(e.SelectedItem.GetType().ToString());
            MasterPageItem mt = (MasterPageItem)e.SelectedItem;
            string t = mt.Title;
            if (t == "Modes")
            {
                tabPage.CurrentPage = tabPage.Children[0];
                this.IsPresented = false;
            }
            if (t == "Settings")
            {
                tabPage.CurrentPage = tabPage.Children[1];
                this.IsPresented = false;
            }
                



        }
    }
}
