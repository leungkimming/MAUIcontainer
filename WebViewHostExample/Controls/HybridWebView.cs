using Microsoft.Maui.Handlers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;


namespace WebViewHostExample.Controls {
    public class SourceChangedEventArgs : EventArgs {
        public WebViewSource Source {
            get;
            private set;
        }

        public SourceChangedEventArgs(WebViewSource source) {
            Source = source;
        }
    }

    public class JavaScriptActionEventArgs : EventArgs {
        public string Payload { get; private set; }
        public JavaScriptActionEventArgs(string payload) {
            Payload = payload;
        }
    }

    public interface IHybridWebView : IView {
        event EventHandler<SourceChangedEventArgs> SourceChanged;
        event EventHandler<JavaScriptActionEventArgs> JavaScriptAction;
        void Refresh();

        WebViewSource Source { get; set; }

        void Cleanup();

        void InvokeAction(string data);

    }


    public class HybridWebView : WebView, IHybridWebView {
        public event EventHandler<SourceChangedEventArgs> SourceChanged;
        public event EventHandler<JavaScriptActionEventArgs> JavaScriptAction;

        public HybridWebView() {
        }

        public void Refresh() {
            if (Source == null) return;
            var s = Source;
            Source = null;
            Source = s;
        }

        public WebViewSource Source {
            get { return (WebViewSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
          propertyName: "Source",
          returnType: typeof(WebViewSource),
          declaringType: typeof(HybridWebView),
          defaultValue: new UrlWebViewSource() { Url = "about:blank" },
          propertyChanged: OnSourceChanged);

        private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue) {
            var view = bindable as HybridWebView;

            bindable.Dispatcher.Dispatch(() => {
                view.SourceChanged?.Invoke(view, new SourceChangedEventArgs(newValue as WebViewSource));

            });
        }

        public void Cleanup() {
            JavaScriptAction = null;
        }

        public void InvokeAction(string data) {
            JavaScriptAction?.Invoke(this, new JavaScriptActionEventArgs(data));
        }
    }
    public static class BlazorCallHelper {
        public class CallMAUIjson {
            public string method { get; set; }
            public string promiseId { get; set; }
            public string args { get; set; }
        }
        public static string callMAUIhandler(string args, out string promiseId) {
            CallMAUIjson callMAUIjson = JsonSerializer.Deserialize<CallMAUIjson>(args);
            promiseId = callMAUIjson.promiseId;
            return (string)typeof(BlazorCallHelper).GetMethod(callMAUIjson.method).Invoke(null, new object[] { callMAUIjson.args });
        }
        public static string getToken(string args) {
            return $"I recieved \"{args}\" from you";
        }
    }
}
