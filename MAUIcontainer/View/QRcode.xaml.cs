using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace MAUIcontainer;

public partial class QRcode : ContentPage {
    public ObservableCollection<ScannedCode> ScannedList { get; set; }
    public QRcode()
	{
		InitializeComponent();
        ScannedList = new ObservableCollection<ScannedCode>();
        BindingContext = this;

        barcodeView.Options = new BarcodeReaderOptions {
            Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = true
        };
    }
    protected override void OnAppearing() {
        barcodeView.IsDetecting = true;
        base.OnAppearing();
    }
    protected override void OnDisappearing() {
        barcodeView.IsDetecting = false;
        base.OnDisappearing();
    }
    protected void BarcodesDetected(object sender, BarcodeDetectionEventArgs e) {
        foreach (var barcode in e.Results) {
            Dispatcher.Dispatch(() => {
                if (!ScannedList.Any(x => x.Value == barcode.Value)) {
                    ScannedList.Add(new ScannedCode() {
                        Value = barcode.Value,
                        Type = barcode.Format.ToString(),
                    });
                }
            });
        }
    }
    void Cancel_Clicked(object sender, EventArgs e) {
        ScannedList.Clear();
        MessagingCenter.Send<QRcode, ObservableCollection<ScannedCode>>(this, "ScanCode", ScannedList);
        Navigation.PopModalAsync();
    }
    void Clear_Clicked(object sender, EventArgs e) {
        ScannedList.Clear();
        Navigation.PopModalAsync();
    }
    void Save_Clicked(object sender, EventArgs e) {
        MessagingCenter.Send<QRcode, ObservableCollection<ScannedCode>>(this, "ScanCode", ScannedList);
        Navigation.PopModalAsync();
    }
    void TorchButton_Clicked(object sender, EventArgs e) {
        barcodeView.IsTorchOn = !barcodeView.IsTorchOn;
    }
}