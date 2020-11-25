﻿using Android.Webkit;
using Xamarin.Forms.Platform.Android;

namespace RAHAL.Droid
{
    public class JavascriptWebViewClient : FormsWebViewClient
    {
        readonly string _javascript;

        public JavascriptWebViewClient(HybridWebViewRenderer renderer, string javascript) : base(renderer)
        {
            _javascript = javascript;
        }

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            view.EvaluateJavascript(_javascript, null);
        }
    }
}
