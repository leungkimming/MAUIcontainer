using MAUIcontainer.Controls;
using System.Security.Policy;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using MAUIcontainer.Common;
using MAUIcontainer.ViewModels;
using Plugin.Firebase.CloudMessaging;
using System.Text.Json;

namespace MAUIcontainer;
public partial class MainPage : ContentPage {
    public IPublicClientApplication IdentityClient { get; set; }
    MainPageViewModel vm;
    public bool IsLogin { get; set; }
    public JavaScriptActionHandler jsActionHandler { get; set; }
    public MainPage() {
		InitializeComponent();
        App.mainpage = this;
        vm = new MainPageViewModel();
        BindingContext = vm;

        MyWebView.JavaScriptAction += MyWebView_JavaScriptActionHandler;
        //vm.UrlText = "https://192.168.0.30:44355/dotnet6EAA/";
        vm.UrlText = "https://192.168.0.30:7196/";
        //vm.UrlText = "https://mauiphoto.z23.web.core.windows.net/";

        string token = BlazorCallHelper.getAADToken();
        if (token == null) {
            MAUIcontainer.App.Current.ModalPopping += LoadApp;
            Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushModalAsync(new AADLogin(true));
        } else {
            LoadApp(this, null);
        }

        jsActionHandler = new JavaScriptActionHandler();
    }
    public void LoadApp(object sender, EventArgs e) {
        int key = App.MessageQueue.Where(x => x.Value != null).FirstOrDefault().Key;
        App.errmessage += $"LoadApp key={key};";
        if (key != 0) {
            vm.Source = vm.UrlText;
            App.currentURL = vm.UrlText;
        }
    }
    void onSearchButton(object sender, EventArgs e) {
        vm.Source = vm.UrlText;
        App.currentURL = vm.UrlText;
    }
    private void MyWebView_JavaScriptActionHandler(object sender, Controls.JavaScriptActionEventArgs e) {
        Dispatcher.Dispatch(() => {
            jsActionHandler.ProcessMessage(e.Payload, ((HybridWebView)sender).Handler);
        });
    }
}


