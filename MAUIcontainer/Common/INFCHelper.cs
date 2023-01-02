using Plugin.NFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer {
    public interface INFCHelper {
        public NFCstatus getStatus();
        public void readNFC(BlazorCallHelper.Callback callback, string promiseId);
        public void writeNFC(BlazorCallHelper.Callback callback, string promiseId,
            writeMode mode, ITagInfo tagInfo);
    }
}
