namespace MAUIcontainer;

public partial class ImageViewer : ContentPage
{
	public ImageViewer(string filePath)
	{
		InitializeComponent();
        FilePathLabel.Text = $"File Name : {filePath.Split("/").Last()}";
        FileViewWebView.Source = filePath;
    }
    void onQuitButton(object sender, EventArgs e) {
        Navigation.PopModalAsync();
    }
}