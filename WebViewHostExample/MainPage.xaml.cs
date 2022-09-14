using WebViewHostExample.ViewModels;
using WebViewHostExample.Controls;
using System.Security.Policy;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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
        //vm.UrlText = "https://192.168.1.136:44355/dotnet6EAA/";
        vm.UrlText = "https://192.168.0.30:7196/";
        //vm.UrlText = "https://dotnet6client.z23.web.core.windows.net/";

        jsActionHandler = new JavaScriptActionHandler();
    }

    protected override void OnParentSet() {
        base.OnParentSet();
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


