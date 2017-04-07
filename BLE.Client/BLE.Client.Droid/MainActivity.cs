using Acr.UserDialogs;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Forms.Presenter.Droid;
using MvvmCross.Platform;
using SVG.Forms.Plugin.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace BLE.Client.Droid
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity
        : FormsAppCompatActivity
    {
        private readonly string[] Permissions =
       {
            Manifest.Permission.Bluetooth,
            Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.BluetoothPrivileged,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        protected override void OnCreate(Bundle bundle)
        {

            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            SvgImageRenderer.Init();

            UserDialogs.Init(this);
            Forms.Init(this, bundle);

            if(((int)Android.OS.Build.VERSION.SdkInt) >= 23)
            {
                CheckPermissions();
            }
            
            var formsApp = new BleMvxFormsApp();
            LoadApplication(formsApp);

            var presenter = (MvxFormsDroidPagePresenter) Mvx.Resolve<IMvxViewPresenter>();
            presenter.MvxFormsApp = formsApp;

            Mvx.Resolve<IMvxAppStart>().Start();
        }

        private void CheckPermissions()
        {
            bool minimumPermissionsGranted = true;

            foreach (string permission in Permissions)
            {
                if (CheckSelfPermission(permission) != Permission.Granted) minimumPermissionsGranted = false;
            }

            // If one of the minimum permissions aren't granted, we request them from the user
            if (!minimumPermissionsGranted) RequestPermissions(Permissions, 0);
        }

        /*
        public override void OnBackPressed()
        {
            return;
            Finish();
            base.OnBackPressed();
            //base.OnBackPressed() p
        }
        */

    }
}