using WebViewHostExample.ViewModels;
using WebViewHostExample.Controls;
using System.Security.Policy;

namespace WebViewHostExample;
public partial class MainPage : ContentPage
{
    MainPageViewModel vm;
    JavaScriptAction jsAction;

    public MainPage() {
		InitializeComponent();

        vm = new MainPageViewModel();
        BindingContext = vm;

        MyWebView.JavaScriptAction += MyWebView_JavaScriptAction;
        vm.UrlText = "https://192.168.1.136:44355/dotnet6EAA/";
        //vm.UrlText = "https://dotnet6client.z23.web.core.windows.net/";

        jsAction = new JavaScriptAction();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
    }
    void onSearchButton(object sender, EventArgs e) {
        vm.Source = vm.UrlText;
    }
    private void MyWebView_JavaScriptAction(object sender, Controls.JavaScriptActionEventArgs e)
    {
		Dispatcher.Dispatch(() =>
		{
            //DisplayAlert("Data from Javascript", e.Payload, "OK");
            jsAction.ProcessMessage(e.Payload);
        });
    }
}


