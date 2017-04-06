using Acr.UserDialogs;
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
        protected override void OnCreate(Bundle bundle)
        {

            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            SvgImageRenderer.Init();

            UserDialogs.Init(this);
            Forms.Init(this, bundle);
            var formsApp = new BleMvxFormsApp();
            LoadApplication(formsApp);

            var presenter = (MvxFormsDroidPagePresenter) Mvx.Resolve<IMvxViewPresenter>();
            presenter.MvxFormsApp = formsApp;

            Mvx.Resolve<IMvxAppStart>().Start();
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