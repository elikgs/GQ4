using Android.App;
using Android.Content.PM;
using Android.OS;

namespace GQ4
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static MainActivity? Instance { get; private set; }
        public static System.Threading.Tasks.TaskCompletionSource<string?>? PhotoTcs;

        protected override void OnCreate(Android.OS.Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Instance = this;
        }

        protected override void OnActivityResult(int requestCode, [Android.Runtime.GeneratedEnum] Android.App.Result resultCode, Android.Content.Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1001)
            {
                var path = data?.GetStringExtra("photoPath");
                PhotoTcs?.TrySetResult(path);
                PhotoTcs = null;
            }
        }
    }
}
