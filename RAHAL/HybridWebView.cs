using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RAHAL
{
    public class HybridWebView : WebView
    {
        Action<string> action;

        public static readonly BindableProperty UriProperty = BindableProperty.Create(
            propertyName: "Uri",
            returnType: typeof(string),
            declaringType: typeof(HybridWebView),
            defaultValue: default(string));

        public string Uri
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        public void RegisterAction(Action<string> callback)
        {
            action = callback;
        }

        public void Cleanup()
        {
            action = null;
        }

        public void InvokeAction(string syncInfo)
        {
            if (action == null || syncInfo == null)
            {
                return;
            }
            if (MainThread.IsMainThread)
                action.Invoke(syncInfo);
            else
                MainThread.BeginInvokeOnMainThread(() => action.Invoke(syncInfo));
        }       
    }
}
