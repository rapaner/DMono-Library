using Android.App;
using Android.Runtime;

namespace Library.Platforms.Android;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        System.Diagnostics.Debug.WriteLine("=== MainApplication constructor called ===");
    }

    protected override MauiApp CreateMauiApp()
    {
        System.Diagnostics.Debug.WriteLine("=== MainApplication.CreateMauiApp called ===");
        return MauiProgram.CreateMauiApp();
    }
}

