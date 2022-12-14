namespace MAUIcontainer;

public partial class AppShell : Shell
{
    public Command ExitBlazorCommand { get; set; }
    public AppShell()
	{
		InitializeComponent();
        ExitBlazorCommand = new Command(OnBlazorExitClicked);
        BindingContext = this;      
        Shell.SetNavBarIsVisible(ShellBar,false);
    }
    public void OnBlazorExitClicked() {
        App.mainpage.onQuitButton(this, null);
        Shell.Current.FlyoutIsPresented = false;
    }
}
