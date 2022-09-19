using Microsoft.Identity.Client;
using System.ComponentModel;

namespace WebViewHostExample;

public partial class AADLogin : ContentPage, INotifyPropertyChanged {
    public bool isPush { get; set; }
    public AADLogin()
	{
		InitializeComponent();
        useNTLMButton.IsVisible = false;
        var authService = new AuthService();
        getStatus();
    }
    public AADLogin(bool _isPush) : this() {
        this.isPush = _isPush;
        useNTLMButton.IsVisible = true;
    }
    async void getStatus() {
        var authService = new AuthService();
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
    async void onLogin(object sender, EventArgs e) {
        var authService = new AuthService();
        var result = await authService.LoginAsync(CancellationToken.None);
        if (result != null) {
            Status.Text = "Status: Authenticated" +
                "\nUser: " + result.Account.Username +
                "\nTenant: " + result.TenantId +
                "\nScope: " + result.Scopes.FirstOrDefault() +
                "\nToken Expiry: " + result.ExtendedExpiresOn.LocalDateTime.ToString("dd/MM/yyyy HH:MM:ss");
            Login.IsEnabled = false;
            if (isPush) {
                await Navigation.PopModalAsync();
            }
        }
    }
    async void onLogout(object sender, EventArgs e) {
        var authService = new AuthService();
        authService.LogoutAsync();
        Status.Text = "Status: Logout";
        Login.IsEnabled = true;
#if ANDROID
        bool action  = await Application.Current.MainPage.DisplayAlert("Confirmation", "Exit the Application?", "Confirm", "Cancel");
        if (action) {
            Application.Current?.Quit();
        }
#endif
    }
    async void useNTLM(object sender, EventArgs e) {
        if (isPush) {
            await Navigation.PopModalAsync();
        }
    }
}