namespace WebViewHostExample;

public partial class Login : ContentPage
{
	//public string user { get; set; }
	//public string pw { get; set; }
	public Login()
	{
		InitializeComponent();
	}
    void onSubmit(object sender, EventArgs e) {
		string[] authen = new string[] {user.Text, pw.Text };
        MessagingCenter.Send<Login, string[]>(this, "WindowsAuthen", authen);
        Navigation.PopModalAsync();
    }
}