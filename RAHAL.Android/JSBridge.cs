using Android.Webkit;
using Java.Interop;
using System;

namespace RAHAL.Droid
{
    public class JSBridge : Java.Lang.Object
    {
        readonly WeakReference<HybridWebViewRenderer> hybridWebViewRenderer;

        public JSBridge(HybridWebViewRenderer hybridRenderer)
        {
            hybridWebViewRenderer = new WeakReference<HybridWebViewRenderer>(hybridRenderer);
        }

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string syncInfo)
        {
            if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget(out HybridWebViewRenderer hybridRenderer))
            {
                ((HybridWebView)hybridRenderer.Element).InvokeAction(syncInfo);
            }
        }
    }
}

