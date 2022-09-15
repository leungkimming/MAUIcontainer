﻿using Plugin.FirebasePushNotification;

namespace WebViewHostExample;

public partial class App : Application {
    public App() {
        InitializeComponent();

        CrossFirebasePushNotification.Current.OnTokenRefresh += (s, p) => {
            System.Diagnostics.Debug.WriteLine($"TOKEN : {p.Token}");
        };
        CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) => {
            System.Diagnostics.Debug.WriteLine("Received");
            MessagingCenter.Send<App, string>(this, "PushNotification", p.Data["body"].ToString());
        };
        CrossFirebasePushNotification.Current.OnNotificationOpened += (s, p) => {
            System.Diagnostics.Debug.WriteLine("Opened");
            foreach (var data in p.Data) {
                System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
            }
            MainThread.BeginInvokeOnMainThread(() => Microsoft.Maui.Controls.Application.Current.MainPage
                .DisplayAlert("Push Notification Open", $"{p.Data["body"].ToString()}", "Ok"));
        };

        MainPage = new AppShell();
    }
}
