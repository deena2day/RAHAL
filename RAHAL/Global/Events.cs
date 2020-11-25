using System;

namespace RAHAL.Global
{
    public class Events
    {
        public static event EventHandler<SMSEventArgs> OnSMSReceived;

        public static void OnSMSReceived_Event(object sender, SMSEventArgs sms)
        {
            OnSMSReceived?.Invoke(sender, sms);
        }
    }

    public class SMSEventArgs : EventArgs
    {
        public string PhoneNumber { get; set; }

        public string Message { get; set; }
    }
}