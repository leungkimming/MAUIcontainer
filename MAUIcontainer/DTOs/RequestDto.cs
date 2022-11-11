using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer {
    public class RequestDto {
        public string? Domain => $"{Host}:{Port}";
        public string Host => new Uri(Uri)?.Host;
        public int? Port => new Uri(Uri)?.Port;
        public string Uri { get; set; }
    }
}
