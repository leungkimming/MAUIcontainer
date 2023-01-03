using Plugin.NFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer {
    public enum NFCstatus {
        NotSupport = 0,
        NotAvailable = 1,
        NotEnabled = 2,
        Enabled = 3,
        UnDefined = 4
    }
    public enum writeMode { 
        Clear = 0,
        WriteProtect = 1,
        Update = 2,
    }
    public class NFCData {
        public string serialNumber { get; set; }
        public TagInfo tagInfo { get; set; }
        public string Message { get; set; }
        public bool success { get; set; }
    }
    public class NFCWriteData {
        public writeMode mode { get; set; }
        public TagInfo writeTagInfo { get; set; }
    }
}
