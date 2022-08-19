//using Android.App.AppSearch;
//using Java.Net;
using WebViewHostExample.ViewModels;

namespace WebViewHostExample;

public partial class MainPage : ContentPage
{
	int count = 0;
    MainPageViewModel vm;

    public MainPage()
	{
		InitializeComponent();

        vm = new MainPageViewModel();
        //MyWebView.BindingContext = vm;
        BindingContext = vm;

        MyWebView.JavaScriptAction += MyWebView_JavaScriptAction;
        vm.UrlText = "https://192.168.1.136:44355/dotnet6EAA/";
        //vm.UrlText = "https://dotnet6client.z23.web.core.windows.net/";
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
            //ChangeLabel.Text = "The Web Button Was Clicked! Count: " + e.Payload;
        });
    }
}





//        string htmlSource = @"
//<html>
//<head></head>
//<body>

//<script>
//    var counter = 1;
//    function buttonClicked(e) {		
//		invokeCSharpAction(counter++);
//    }
//</script>

//<div style='display: flex; flex-direction: column; justify-content: center; align-items: center; width: 100%'>
//<h2 style='font-family: script'><i>Fancy Web Title</i></h2>
//<button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='javascript:buttonClicked(event)'>Click Me!</button>
//</div>
//</html>
//";


