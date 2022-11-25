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
    public JavaScriptActionHandler jsActionHandler { get; set; }
    public MainPage() {
        InitializeComponent();
        App.mainpage = this;
        vm = new MainPageViewModel();
        BindingContext = vm;

        MyWebView.JavaScriptAction += MyWebView_JavaScriptActionHandler;
        jsActionHandler = new JavaScriptActionHandler();

        //vm.UrlText = "https://192.168.0.30:44355/dotnet6EAA/";
        //vm.UrlText = "https://192.168.0.30:7196/";
        //vm.UrlText = "https://mauiphoto.z23.web.core.windows.net/";

        //if not AAD authen, force authen first. Then, determine what App to load
        string token = BlazorCallHelper.getAADToken();
        if (token == null) {
            MAUIcontainer.App.Current.ModalPopped += LoadApp;
            Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushModalAsync(new AADLogin(true));
        } else {
            App.IsLogin = true;
            LoadApp(this, null);
        }
    }
    public async void LoadApp(object sender, EventArgs e) {
        if (e != null && (e as ModalPoppedEventArgs).Modal.ToString() == "MAUIcontainer.AADLogin") {
            MAUIcontainer.App.Current.ModalPopped -= LoadApp;
        }
        // search PM in queue, launch the PM's App if available.
        int key = 0;
        MyApp app = null;
        var query = App.MessageQueue.Where(x => x.Value != null);
        if (query.Count() > 0) {
            foreach (var item in query) {
                App.errmessage += App.MyAppsArray.Count() + item.Value.Data["App"];
                app = App.MyAppsArray.Where(x => x.Name == item.Value.Data["App"]).FirstOrDefault();
                if (app != null) {
                    key = item.Key;
                    break;
                }
            }
        }
        App.errmessage += $"LoadApp key={key}";
        if (app != null) {
            vm.Source = app.BlazorUrl;
            App.currentApp = app;
            vm.UrlText = app.Name;
            if (Navigation.ModalStack.Count > 0) {
                await Navigation.PopModalAsync();
            }
        } else {
            // Else, let user select which App to launch
            MAUIcontainer.App.Current.ModalPopped += LoadUserApp;
            await Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushModalAsync(new MyApps());
        }
    }
    public void LoadUserApp(object sender, EventArgs e) {
        if (e != null && (e as ModalPoppedEventArgs).Modal.ToString() == "MAUIcontainer.MyApps") {
            MAUIcontainer.App.Current.ModalPopped -= LoadUserApp;
        }
        if (App.currentApp != null) {
            vm.UrlText = App.currentApp.Name;
            vm.Source = App.currentApp.BlazorUrl;
        }
    }
    public async void onQuitButton(object sender, EventArgs e) {
        bool action = await Application.Current.MainPage.DisplayAlert("Confirmation", "Are you sure to CLOSE the App?", "Confirm", "Cancel");
        if (action) {
            vm.Source = "";
            App.currentApp = null;
            // Start all over: check PM to launch App, if no, then let user select App to launch
            LoadApp(this, null);
        }
    }
    private void MyWebView_JavaScriptActionHandler(object sender, Controls.JavaScriptActionEventArgs e) {
        Dispatcher.Dispatch(() => {
            jsActionHandler.ProcessMessage(e.Payload, ((HybridWebView)sender).Handler);
        });
    }
}


