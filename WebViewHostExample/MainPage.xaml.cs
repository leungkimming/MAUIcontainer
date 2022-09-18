using WebViewHostExample.ViewModels;
using WebViewHostExample.Controls;
using System.Security.Policy;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WebViewHostExample.Common;

namespace WebViewHostExample;
public partial class MainPage : ContentPage {
    public IPublicClientApplication IdentityClient { get; set; }
    MainPageViewModel vm;
    public bool IsLogin { get; set; }
    public JavaScriptActionHandler jsActionHandler { get; set; }
    public MainPage() {
		InitializeComponent();

        vm = new MainPageViewModel();
        BindingContext = vm;

        MyWebView.JavaScriptAction += MyWebView_JavaScriptActionHandler;
        //vm.UrlText = "https://192.168.0.30:44355/dotnet6EAA/";
        vm.UrlText = "https://192.168.0.30:7196/";
        //vm.UrlText = "https://dotnet6client.z23.web.core.windows.net/";

        string token = BlazorCallHelper.getAADToken();
        if (token == null) {
            WebViewHostExample.App.Current.ModalPopping += LoadApp;
            Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushModalAsync(new AADLogin());
        } else {
            LoadApp(this, null);
        }

        jsActionHandler = new JavaScriptActionHandler();

        //var pm = WebViewHostExample.App.MessageQueue.Where(x => x.Key == "PushNotification").FirstOrDefault();
        //if (pm.Key != null && pm.Key == "PushNotification") {
        //    vm.Source = vm.UrlText;
        //}
    }
    void LoadApp(object sender, EventArgs e) {
        var pm = WebViewHostExample.App.MessageQueue.Where(x => x.Key == "PushNotification").FirstOrDefault();
        if (pm.Key != null && pm.Key == "PushNotification") {
            vm.Source = vm.UrlText;
        }
    }
    void onSearchButton(object sender, EventArgs e) {
        vm.Source = vm.UrlText;
    }
    private void MyWebView_JavaScriptActionHandler(object sender, Controls.JavaScriptActionEventArgs e) {
        Dispatcher.Dispatch(() => {
            jsActionHandler.ProcessMessage(e.Payload, ((HybridWebView)sender).Handler);
        });
    }
}


