using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MAUIcontainer;

public partial class MyApps : ContentPage {

    public List<MyApp> MyAppsSource {
        get => App.MyAppsArray;
    }

    public MyApps() {
		InitializeComponent();
        BindingContext = this;
        //MyAppsList.ItemsSource = App.MyAppsArray;
    }
    async void OnItemSelected(object sender, SelectedItemChangedEventArgs args) {
        App.currentApp = args.SelectedItem as MyApp;
        await Navigation.PopModalAsync();
    }
}