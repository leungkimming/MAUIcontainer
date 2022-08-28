namespace WebViewHostExample;

public partial class FileViewer : ContentPage
{
	public FileViewer(string url)
	{
		InitializeComponent();
        FileViewWebView.Source = url;
	}
    void onQuitButton(object sender, EventArgs e) {
        Navigation.PopModalAsync();
    }
}