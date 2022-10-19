using Android.App;
using Android.OS;
using Android.Runtime;
using Firebase;

namespace MAUIcontainer;

[Application]
public class MainApplication : MauiApplication {
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership) {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void OnCreate() {
        base.OnCreate();
    }
}
