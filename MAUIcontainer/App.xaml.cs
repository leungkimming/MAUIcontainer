using Plugin.Firebase.CloudMessaging;
using System;
using System.Text.Json;
using System.Text;

namespace MAUIcontainer;

public partial class App : Application {
    public static Dictionary<int, FCMNotification> MessageQueue { get; private set; }
    public static string errmessage { get; set; }
    public static string currentURL { get; set; }
    public static MainPage mainpage { get; set; }

    public App() {
        InitializeComponent();
        currentURL = "";
        mainpage = null;
        MessageQueue = new Dictionary<int, FCMNotification>();
        CrossFirebaseCloudMessaging.Current.NotificationReceived += Current_NotificationReceived;
        CrossFirebaseCloudMessaging.Current.NotificationTapped += Current_NotificationTapped;
        MainPage = new AppShell();
    }
    private string Add2Queue(FCMNotification message) {
        string jmessage = JsonSerializer.Serialize(message);
        int hashcode = 0;
        if (message.Data.Any(x => x.Key == "gcm.message_id")) {
            hashcode = message.Data["gcm.message_id"].GetHashCode();
        } 
        else if (message.Data.Any(x => x.Key == "google.message_id")) {
            hashcode = message.Data["google.message_id"].GetHashCode();
        } 
        else {
            hashcode = (jmessage + DateTime.Now.Ticks.ToString()).GetHashCode();
        }
        errmessage += $"hashcode={hashcode};";

        if (!MessageQueue.Any(x => x.Key == hashcode)) {
            MessageQueue.Add(hashcode, message);
            return $"{hashcode}|{Convert.ToBase64String(Encoding.Default.GetBytes(jmessage))}";
        }
        return null;
    }
    private void Current_NotificationTapped(object sender, Plugin.Firebase.CloudMessaging.EventArgs.FCMNotificationTappedEventArgs e) {
        errmessage += "PM Tapped";
        string jmessage = Add2Queue(e.Notification);
        errmessage += $"MessageQ jmessage={(jmessage != null ? jmessage.Substring(0, 15) : null)};";
        if (mainpage != null && currentURL == "") { // actual implementation should check PM message App is loaded?
            errmessage += "LoadApp;";
            mainpage.LoadApp(this, null);
        }
        if (jmessage != null) {
            MessagingCenter.Send<App, string>(this, "PushNotification", jmessage);
            errmessage += $"MessagingCenter jmessage={jmessage.Substring(0, 15)};";
        }
    }
    private void Current_NotificationReceived(object sender, Plugin.Firebase.CloudMessaging.EventArgs.FCMNotificationReceivedEventArgs e) {
        System.Diagnostics.Debug.WriteLine($"PM Received:{e.Notification.Title}, {e.Notification.Body}");
        errmessage += "PM Received";
    }
}
