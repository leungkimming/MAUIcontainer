using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.NFC;
using System.Text.Json;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace MAUIcontainer {
    public class NFCHelper : INFCHelper {
        NFCNdefTypeFormat _type;
        bool _eventsAlreadySubscribed = false;
        bool _isDeviceiOS = false;
        BlazorCallHelper.Callback _callback;
        string _promiseId = null;
        writeMode _writeMode;
        ITagInfo _writeTagInfo;

        public NFCHelper() { 
        }
        public NFCstatus getStatus() {
            if (!CrossNFC.IsSupported) return NFCstatus.NotSupport;
            if (!CrossNFC.Current.IsAvailable) return NFCstatus.NotAvailable;
            if (!CrossNFC.Current.IsEnabled) return NFCstatus.NotEnabled;
            if (CrossNFC.Current.IsEnabled) {
                if (DeviceInfo.Platform == DevicePlatform.iOS) _isDeviceiOS = true;
                return NFCstatus.Enabled;
            };
            return NFCstatus.UnDefined;
        }
        async void returnError(string message) {
            await StopListening();
            NFCData NFCreturn = new NFCData() {
                tagInfo = null,
                success = false,
                Message = message
            };
            string s_NFCreturn = Convert.ToBase64String(Encoding.Default.GetBytes(JsonSerializer.Serialize(NFCreturn)));
            _callback(_promiseId, s_NFCreturn);
        }
        public async void readNFC(BlazorCallHelper.Callback callback, string promiseId) {
            _callback = callback;
            _promiseId = promiseId;
            if (getStatus() == NFCstatus.Enabled) {
                await BeginListening();
            } else {
                returnError(getStatus().ToString());
            }
        }
        public async void writeNFC(BlazorCallHelper.Callback callback, string promiseId, 
            writeMode mode, ITagInfo tagInfo) {
            _callback = callback;
            _promiseId = promiseId;
            _writeMode = mode;
            _writeTagInfo = tagInfo;
            if (mode != writeMode.Clear && 
                (tagInfo == null) || tagInfo.Records.Length < 1) {
                returnError("Must have data if not clear Tag");
            }
            if (getStatus() == NFCstatus.Enabled) {
                await Publish();
            } else {
                returnError(getStatus().ToString());
            }
        }
        async Task StartListeningIfNotiOS() {
            if (_isDeviceiOS) {
                SubscribeEvents();
                return;
            }
            await BeginListening();
        }
        async Task BeginListening() {
            if (!_isDeviceiOS) {
                App.mainpage.vm.isNFCScanning = true;
            }
            try {
                MainThread.BeginInvokeOnMainThread(() => {
                    SubscribeEvents();
                    CrossNFC.Current.StartListening();
                });
            } catch (Exception ex) {
                returnError(ex.Message);
            }
        }
        public async Task StopListening() {
            App.mainpage.vm.isNFCScanning = false;
            try {
                MainThread.BeginInvokeOnMainThread(() => {
                    CrossNFC.Current.StopListening();
                    UnsubscribeEvents();
                });
            } catch (Exception ex) {
                returnError(ex.Message);
            }
        }
        void SubscribeEvents() {
            if (_eventsAlreadySubscribed)
                UnsubscribeEvents();

            _eventsAlreadySubscribed = true;

            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
            CrossNFC.Current.OnMessagePublished += Current_OnMessagePublished;
            CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
            CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;
            CrossNFC.Current.OnTagListeningStatusChanged += Current_OnTagListeningStatusChanged;

            if (_isDeviceiOS)
                CrossNFC.Current.OniOSReadingSessionCancelled += Current_OniOSReadingSessionCancelled;
        }

        /// <summary>
        /// Unsubscribe from the NFC events
        /// </summary>
        void UnsubscribeEvents() {
            CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
            CrossNFC.Current.OnMessagePublished -= Current_OnMessagePublished;
            CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
            CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;
            CrossNFC.Current.OnTagListeningStatusChanged -= Current_OnTagListeningStatusChanged;

            if (_isDeviceiOS)
                CrossNFC.Current.OniOSReadingSessionCancelled -= Current_OniOSReadingSessionCancelled;

            _eventsAlreadySubscribed = false;
        }
        void Current_OnMessageReceived(ITagInfo tagInfo) {
            NFCData NFCreturn = new NFCData() {
                tagInfo = tagInfo as TagInfo,
                success = false
            };
            if (tagInfo == null) {
                NFCreturn.Message = "No tag found";
            } else {
                var identifier = tagInfo.Identifier;
                NFCreturn.serialNumber = NFCUtils.ByteArrayToHexString(identifier, ":");
                if (!tagInfo.IsSupported) {
                    NFCreturn.Message = "Unsupported tag";
                } else if (tagInfo.IsEmpty) {
                    NFCreturn.Message = "Empty tag";
                    NFCreturn.success = true;
                } else {
                    NFCreturn.success = true;
                }
            }
            Task.Run(() => StopListening());
            string s_NFCreturn = Convert.ToBase64String(Encoding.Default.GetBytes(JsonSerializer.Serialize(NFCreturn)));
            _callback(_promiseId, s_NFCreturn);
        }
        async void Current_OniOSReadingSessionCancelled(object sender, EventArgs e) {
            returnError("Scanning cancelled by user");
        }
        async Task Publish() {
            await StartListeningIfNotiOS();
            try {
                CrossNFC.Current.StartPublishing(_writeMode == writeMode.Clear);
            } catch (Exception ex) {
                returnError(ex.Message + ex.StackTrace);
            }
        }
        async void Current_OnTagDiscovered(ITagInfo tagInfo, bool format) {
            if (!CrossNFC.Current.IsWritingTagSupported) {
                returnError("Writing tag is not supported on this device");
                return;
            }

            try {
                if (format) {
                    NFCNdefRecord record = null;
                    tagInfo.Records = new[] { record };
                    CrossNFC.Current.ClearMessage(tagInfo);
                } else {
                    tagInfo.Records = _writeTagInfo.Records;
                    CrossNFC.Current.PublishMessage(tagInfo, _writeMode == writeMode.WriteProtect);
                }
            } catch (Exception ex) {
                returnError(ex.Message + ex.StackTrace);
            }
        }
        void Current_OnMessagePublished(ITagInfo tagInfo) {
            NFCData NFCreturn = new NFCData() {
                tagInfo = tagInfo as TagInfo,
                success = true
            };
            try {
                CrossNFC.Current.StopPublishing();
                if (tagInfo.IsEmpty)
                    NFCreturn.Message = "Formatting tag operation successful";
                else
                    NFCreturn.Message = "Writing tag operation successful";
                Task.Run(() => StopListening());
                string s_NFCreturn = Convert.ToBase64String(Encoding.Default.GetBytes(JsonSerializer.Serialize(NFCreturn)));
                _callback(_promiseId, s_NFCreturn);
            } catch (Exception ex) {
                returnError(ex.Message + ex.StackTrace);
            }
        }
        void Current_OnTagListeningStatusChanged(bool isListening) { }
        void Current_OnNfcStatusChanged(bool isEnabled) { }
    }
}
