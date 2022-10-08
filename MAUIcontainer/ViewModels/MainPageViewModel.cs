using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer.ViewModels
{
    internal class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private WebViewSource source;
        private string _UrlText;
        
        public string UrlText { 
            get => _UrlText;
            set {
                if (!object.Equals(_UrlText, value)) {
                    _UrlText = value;
                    OnPropertyChanged();
                }
            }
        }

        public WebViewSource Source
        {
            get => source;
            set
            {
                if (!object.Equals(source, value))
                {
                    source = value; 
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
