using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace MAUIcontainer;

public partial class QRcode : ContentPage
{
	public QRcode()
	{
		InitializeComponent();

        barcodeView.Options = new BarcodeReaderOptions {
            Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = true
        };
    }
    protected void BarcodesDetected(object sender, BarcodeDetectionEventArgs e) {
        foreach (var barcode in e.Results) {
            Console.WriteLine($"Barcodes: {barcode.Format} -> {barcode.Value}");
            barcodeData.Text += $"Barcodes: {barcode.Format} -> {barcode.Value}\n";
        }
    }
    void TorchButton_Clicked(object sender, EventArgs e) {
        barcodeView.IsTorchOn = !barcodeView.IsTorchOn;
    }
}