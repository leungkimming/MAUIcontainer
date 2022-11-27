using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer {
    public class MyAppsResponse {
        public MyAppDto[] MyAppsDto { get; set; }
    }
    public class MyAppDto {
        public string Name { get; set; }
        public string Description { get; set; }
        public string BlazorNamespace { get; set; }
        public string BlazorUrl { get; set; }
        public string ImageBase64 { get; set; }
        public MyApp toMyApp {
            get {
                return new MyApp() {
                    Name = Name,
                    Description = Description,
                    BlazorNamespace = BlazorNamespace,
                    BlazorUrl = BlazorUrl,
                    Imagestream = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(ImageBase64)))
                };
            }
        }
    }
}
