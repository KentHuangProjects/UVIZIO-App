using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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

        

       
    }
}
