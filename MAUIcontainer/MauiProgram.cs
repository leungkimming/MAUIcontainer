
using MAUIcontainer.Controls;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Shared;
using Plugin.Firebase.CloudMessaging;

#if ANDROID
using MAUIcontainer.Platforms.Droid.Renderers;
using Plugin.Firebase.Android;
#endif

#if IOS
using MAUIcontainer.Platforms.iOS.Renderers;
using Plugin.Firebase.iOS;
#endif

namespace MAUIcontainer;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .RegisterFirebaseServices()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureMauiHandlers(handlers => {
                handlers.AddHandler(typeof(HybridWebView), typeof(HybridWebViewHandler));
            });

        return builder.Build();
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
        //builder.Services.AddSingleton(_ => CrossFirebaseCloudMessaging.Current);
        //builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();
        return builder;
    }

    private static CrossFirebaseSettings CreateCrossFirebaseSettings() {
        return new CrossFirebaseSettings(isAuthEnabled: true, isCloudMessagingEnabled: true);
    }
}
