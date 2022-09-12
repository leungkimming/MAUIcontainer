using Microsoft.Identity.Client;

namespace WebViewHostExample;

public partial class AADLogin : ContentPage
{
    public AADLogin()
	{
		InitializeComponent();
        var authService = new AuthService();
        getStatus();
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
        } else {
            Status.Text = "Status: Logout";
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
        }
    }
    void onLogout(object sender, EventArgs e) {
        var authService = new AuthService();
        authService.LogoutAsync();
        Status.Text = "Status: Logout";
    }
}