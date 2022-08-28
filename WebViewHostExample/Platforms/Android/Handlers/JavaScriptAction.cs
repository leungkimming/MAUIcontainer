using Android.Content;
using Android.Provider;
using Android.Webkit;

namespace WebViewHostExample.Controls {
    internal partial class JavaScriptAction {
        public void ProcessMessage(string message) {
            if (message.StartsWith("Download:")) {
                //"Download:abc.txt,mimeType:application\pdf,base64:xxxxxxxxxxxxxxxx
                int minePos = message.IndexOf(",mimeType:");
                int base64Pos = message.IndexOf(",base64:");
                string fileName = message.Substring(9, minePos - 9);
                string base64 = message.Substring(base64Pos + 8);
                MimeTypeMap mime = MimeTypeMap.Singleton;
                string mimetype = mime.GetMimeTypeFromExtension(fileName.Split('.')[1]);
                var resolver = Android.App.Application.Context.ContentResolver;
                var contentValues = new ContentValues();
                contentValues.Put(MediaStore.IMediaColumns.Title, fileName);
                contentValues.Put(MediaStore.IMediaColumns.MimeType, mimetype);
                contentValues.Put(Android.Provider.MediaStore.Downloads.InterfaceConsts.DisplayName, fileName.Split('.')[0]);
                var url = resolver.Insert(MediaStore.Downloads.ExternalContentUri, contentValues);
                var stream = resolver.OpenOutputStream(url);
                byte[] pdfAsBytes1 = Convert.FromBase64String(base64);
                stream.Write(pdfAsBytes1);
                stream.Flush();
                stream.Close();

                Intent intent = new Intent(Intent.ActionPick);
                intent.SetDataAndType(url, "*/*");
                intent.SetAction(Intent.ActionGetContent);
                Platform.CurrentActivity.StartActivityForResult(Intent.CreateChooser(intent, "Open Downloaded File"), 1);
            }
        }
    }
}
