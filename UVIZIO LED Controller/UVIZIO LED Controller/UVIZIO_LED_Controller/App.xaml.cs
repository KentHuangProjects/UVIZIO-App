using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace UVIZIO_LED_Controller
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new UVIZIO_LED_Controller.MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
