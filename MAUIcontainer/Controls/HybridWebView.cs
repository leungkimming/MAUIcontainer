using Microsoft.Maui.Handlers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer.Controls {
    public class JavaScriptActionEventArgs : EventArgs {
        public string Payload { get; private set; }
        public JavaScriptActionEventArgs(string payload) {
            Payload = payload;
        }
    }

    public interface IHybridWebView : IView {
        event EventHandler<JavaScriptActionEventArgs> JavaScriptAction;

        WebViewSource Source { get; set; }

        void Cleanup();

        void InvokeAction(string data);

    }


    public class HybridWebView : WebView, IHybridWebView {
        public event EventHandler<JavaScriptActionEventArgs> JavaScriptAction;
        public HybridWebView() {
        }
        public void Cleanup() {
            JavaScriptAction = null;
        }
        public void InvokeAction(string data) {
            JavaScriptAction?.Invoke(this, new JavaScriptActionEventArgs(data));
        }
    }
}
