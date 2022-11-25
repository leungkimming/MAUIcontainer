using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAUIcontainer;

public partial class MyApps : ContentPage {
    public List<MyApp> MyAppsSource {
        get {
            if (!App.IsLogin) return new List<MyApp>();
            return App.MyAppsArray;
        }
    }

    public MyApps() {
		InitializeComponent();
        BindingContext = this;
    }
    public async void OnItemSelected(object sender, SelectedItemChangedEventArgs args) {
        App.currentApp = args.SelectedItem as MyApp;
        await Navigation.PopModalAsync();
    }
    public void onLoginButton(object sender, EventArgs e) {
        MAUIcontainer.App.Current.ModalPopping += RefreshApps;
        Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushModalAsync(new AADLogin(true));
    }
    public void RefreshApps(object sender, EventArgs e) {
        if (e != null && (e as ModalPoppingEventArgs).Modal.ToString() == "MAUIcontainer.AADLogin") {
            MAUIcontainer.App.Current.ModalPopping -= RefreshApps;
        }
        MyAppsList.ItemsSource= MyAppsSource;
    }
}