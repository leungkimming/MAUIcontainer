using Microsoft.Maui.ApplicationModel;

namespace WebViewHostExample.Controls {
    internal partial class JavaScriptAction {
        public void ProcessMessage(string message) {
            if (message.StartsWith("Download:")) {
                //"Download:abc.txt,mimeType:application\pdf,base64:xxxxxxxxxxxxxxxx
                int minePos = message.IndexOf(",mimeType:");
                int base64Pos = message.IndexOf(",base64:");
                string fileName = message.Substring(9, minePos - 9);
                string minetype = message.Substring(minePos + 10, base64Pos - minePos - 10);
                string base64 = message.Substring(base64Pos + 8);
                string dwldsPath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments), fileName);
                byte[] pdfAsBytes = Convert.FromBase64String(base64);
                File.WriteAllBytes(dwldsPath, pdfAsBytes);
                Application.Current.MainPage.Navigation.PushModalAsync(new FileViewer(dwldsPath));
            }
        }
    }
}
