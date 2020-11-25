using Newtonsoft.Json;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using RAHAL.Global;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RAHAL
{
    public partial class HybridWebViewPage : ContentPage
    {
        private CancellationTokenSource cancellationTokenSource;

        public HybridWebViewPage()
        {
            InitializeComponent();
            hybridWebView.RegisterAction(async syncInfo => await SyncDevice(syncInfo));
            hybridWebView.Uri = Constants.WebSite;
            hybridWebView.Navigated += HybridWebView_Navigated;
            hybridWebView.Navigating += HybridWebView_Navigating;

            Events.OnSMSReceived += GlobalEvents_OnSMSReceived;
        }
        private void GlobalEvents_OnSMSReceived(object sender, SMSEventArgs e)
        {
            //EntryMessage.Text = e.Message;
            //TODO
            if (e.Message.Contains(Constants.SmsOtpText))
            {
                Regex regex = new Regex(Constants.OtpPattern);
                Match match = regex.Match(e.Message);
                if (match.Success)
                {
                    var otpText = match.Value;
                    var syncInfoObj = new SyncInfo() { MDE = "otp", DTK = otpText };
                    var syncInfo = JsonConvert.SerializeObject(syncInfoObj);
                    var script = string.Format("javascript:invokeActionProcessAppRequest('{0}')", syncInfo);
                    hybridWebView.Eval(script);
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var userToken = await SecureStorage.GetAsync("rhltkn");
            if (userToken != null)
            {
                bool isFingerprintAvailable = await CrossFingerprint.Current.IsAvailableAsync(false);
                if (!isFingerprintAvailable)
                {
                    await DisplayAlert("Error", "Biometric Authentication is not available or configured.", "OK");
                    return;
                }
            }
        }

        private void HybridWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            labelProgress.IsVisible = true;
        }

        private void HybridWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            labelProgress.IsVisible = false;

            switch (e.Result)
            {
                case WebNavigationResult.Cancel:
                    // TODO - do stuff here
                    break;
                case WebNavigationResult.Failure:
                case WebNavigationResult.Timeout:
                    var htmlSource = new HtmlWebViewSource
                    {
                        Html = @"<html><body><p>Page could not be loaded</p></body></html>"
                    };
                    hybridWebView.Source = htmlSource;
                    break;
                case WebNavigationResult.Success:
                    // TODO - do stuff here
                    break;
                default:
                    // TODO - do stuff here
                    break;
            }
        }

        private async Task SyncDevice(string jsnObject)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(jsnObject))
                {
                    var syncInfo = JsonConvert.DeserializeObject<SyncInfo>(jsnObject);
                    switch (syncInfo.MDE)
                    {
                        case "stk":
                            SecureStorage.Remove("rhltkn");
                            await SecureStorage.SetAsync("rhltkn", syncInfo.DTK);
                            break;
                        case "pba":
                            var userToken = await SecureStorage.GetAsync("rhltkn");
                            if (userToken != null)
                            {
                                await DoBiometricLogin();
                            }
                            else
                            {
                                await DisplayAlert("Info", "To use Biometric Authentication, you need to login using credentials for the first time.", "OK");
                            }
                            break;
                        case "cba":
                            SecureStorage.Remove("rhltkn");
                            await DisplayAlert("Info", "Token Cleared.", "OK");
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error while processing your request. " + ex.Message, "OK");
            }
        }

        private async Task DoBiometricLogin()
        {
            bool isFingerprintAvailable = await CrossFingerprint.Current.IsAvailableAsync(false);
            if (!isFingerprintAvailable)
            {
                await DisplayAlert("Error", "Biometric Authentication is not available or configured.", "OK");
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();

            AuthenticationRequestConfiguration messageConfig = new AuthenticationRequestConfiguration("RAHAL", "Biometric Authentication, Use your fingerprint to login.");

            var result = await Plugin.Fingerprint.CrossFingerprint.Current.AuthenticateAsync(messageConfig, cancellationTokenSource.Token);

            await SetResultAsync(result);
        }

        private async Task SetResultAsync(FingerprintAuthenticationResult result)
        {
            if (result.Authenticated)
            {
                var userToken = await SecureStorage.GetAsync("rhltkn");
                var syncInfoObj = new SyncInfo() { MDE = "ddl", DTK = userToken };
                var syncInfo = JsonConvert.SerializeObject(syncInfoObj);
                var script = string.Format("javascript:invokeActionProcessAppRequest('{0}')", syncInfo);
                hybridWebView.Eval(script);
            }
            else
            {
                await DisplayAlert("Error", "Biometric Authentication Failed. Please login using Staff Number and Password.", "OK");
            }
        }
    }
}
