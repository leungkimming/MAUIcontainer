
using MAUIcontainer.Controls;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Shared;
using Plugin.Firebase.CloudMessaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

#if ANDROID
using MAUIcontainer.Platforms.Droid.Renderers;
using Plugin.Firebase.Android;
#endif

#if IOS
using MAUIcontainer.Platforms.iOS.Renderers;
using Plugin.Firebase.iOS;
#endif

#if ANDROID
[assembly: Android.App.UsesPermission(Android.Manifest.Permission.Camera)]
#endif

namespace MAUIcontainer;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").Result;
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .RegisterFirebaseServices()
            .RegisterServices()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureMauiHandlers(handlers => {
                handlers.AddHandler(typeof(HybridWebView), typeof(HybridWebViewHandler));
            })
            .UseBarcodeReader()
            .Configuration.AddJsonStream(stream);

        var app = builder.Build();
        using (var scope = app.Services.CreateScope()) {
            BlazorCallHelper.Configure(
                scope.ServiceProvider.GetRequiredService<IPhotoHelper>(),
                scope.ServiceProvider.GetRequiredService<IAuthService>(),
                scope.ServiceProvider.GetRequiredService<INFCHelper>()
            );
        }
        return app;
    }
    private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder) {
        builder.Services
                .AddSingleton<IPhotoHelper, PhotoHelper>()
                .AddSingleton<INFCHelper, NFCHelper>()
                .AddSingleton<IAuthService, AuthService>()
                .AddSingleton<IAPIService, APIService>()
                .AddTransient<AADLogin>()
                .AddTransient<MainPage>()
                .AddTransient<MyApps>();
        return builder;
    }
    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder) {
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.FinishedLaunching((app, launchOptions) => {
                CrossFirebase.Initialize(app, launchOptions, CreateCrossFirebaseSettings());
                return false;
            }));
#else
            events.AddAndroid(android => android.OnCreate((activity, state) =>
                CrossFirebase.Initialize(activity, state, CreateCrossFirebaseSettings())));
#endif
        });
        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        return builder;
    }

    private static CrossFirebaseSettings CreateCrossFirebaseSettings() {
        return new CrossFirebaseSettings(isAuthEnabled: true, isCloudMessagingEnabled: true);
    }
}
