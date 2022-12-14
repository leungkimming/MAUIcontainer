using Microsoft.Identity.Client;
using Plugin.Firebase.CloudMessaging;
using System.ComponentModel;

namespace MAUIcontainer;

public partial class AADLogin : ContentPage, INotifyPropertyChanged {
    public bool isPush { get; set; }
    public static IAuthService authService { get; set; }
    public static string errmessage { get; set; }
    public AADLogin(IAuthService _authService) {
        authService = _authService;
        InitializeComponent();
        useNTLMButton.IsVisible = true;
        isPush = false;
        getStatus();
    }
    public AADLogin setIsPush() {
        this.isPush = true;
        useNTLMButton.IsVisible = false;
        return this;
    }
    async void getStatus() {
        var result = await authService.LoginStatus();
        if (result != null) {
            Status.Text = "Status: Authenticated" +
                "\nUser: " + result.Account.Username +
                "\nTenant: " + result.TenantId +
                "\nScope: " + result.Scopes.FirstOrDefault() +
                "\nToken Expiry: " + result.ExtendedExpiresOn.LocalDateTime.ToString("dd/MM/yyyy HH:MM:ss");
            Login.IsEnabled = false;
        } else {
            Status.Text = "Status: Logout";
            Login.IsEnabled = true;
        }
    }
    async Task<string> GetCFMToken() {
        int count = 0;
        string token = "";
        while (count < 2) {
            try {
                await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
                token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                break;
            } catch (Exception e) {
                token = e.Message;
            }
            count++;
        }
        System.Diagnostics.Debug.WriteLine(token);
        return token;
    }
    async void onDebug(object sender, EventArgs e) {
        App.errmessage += "FCM Token:" + await GetCFMToken();
        bool choice = await DisplayAlert("Debug", $"{App.errmessage} ", "Share", "End");
        if (choice) {
            await Share.Default.RequestAsync(new ShareTextRequest {
                Text = App.errmessage,
                Title = "Share Text"
            });
        }
        App.errmessage = "";
    }

    async void onLogin(object sender, EventArgs e) {
        var result = await authService.LoginAsync(CancellationToken.None);
        if (result != null) {
            Status.Text = "Status: Authenticated" +
                "\nUser: " + result.Account.Username +
                "\nTenant: " + result.TenantId +
                "\nScope: " + result.Scopes.FirstOrDefault() +
                "\nToken Expiry: " + result.ExtendedExpiresOn.LocalDateTime.ToString("dd/MM/yyyy HH:MM:ss");
            Login.IsEnabled = false;
            App.IsLogin = true;
            if (isPush) {
                await Navigation.PopModalAsync();
            }
        }
    }
    async void onLogout(object sender, EventArgs e) {
        authService.LogoutAsync();
        Status.Text = "Status: Logout";
        Login.IsEnabled = true;
        App.IsLogin = false;
#if ANDROID
        bool action  = await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert("Confirmation", "Exit the Application?", "Confirm", "Cancel");
        if (action) {
            Microsoft.Maui.Controls.Application.Current?.Quit();
        }
#endif
    }
    async void useNTLM(object sender, EventArgs e) {
        if (isPush) {
            await Navigation.PopModalAsync();
        }
    }
    public async void onReturnButton(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }
}