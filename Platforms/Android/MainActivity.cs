using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;

namespace Library.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private BackPressHandler? _backHandler;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _backHandler = new BackPressHandler(this);
        OnBackPressedDispatcher.AddCallback(this, _backHandler);
    }

    protected override void OnDestroy()
    {
        _backHandler?.Remove();
        _backHandler = null;

        base.OnDestroy();
    }

    internal bool TryHandleBackNavigation()
    {
        var navigation = Shell.Current?.Navigation;
        if (navigation is null)
        {
            return false;
        }

        if (navigation.ModalStack.Count > 0)
        {
            MainThread.BeginInvokeOnMainThread(async () => await navigation.PopModalAsync());
            return true;
        }

        if (navigation.NavigationStack.Count > 1)
        {
            MainThread.BeginInvokeOnMainThread(async () => await navigation.PopAsync());
            return true;
        }

        return false;
    }

    public override void OnBackPressed()
    {
        if (!TryHandleBackNavigation())
        {
            base.OnBackPressed();
        }
    }

    private sealed class BackPressHandler : OnBackPressedCallback
    {
        private readonly MainActivity _activity;

        public BackPressHandler(MainActivity activity)
            : base(true)
        {
            _activity = activity;
        }

        public override void HandleOnBackPressed()
        {
            if (!_activity.TryHandleBackNavigation())
            {
                Enabled = false;
                try
                {
                    _activity.OnBackPressedDispatcher.OnBackPressed();
                }
                finally
                {
                    Enabled = true;
                }
            }
        }
    }
}