﻿using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using GithubXamarin.Core.ViewModels;
using GithubXamarin.UWP.Services;
using MvvmCross.WindowsUWP.Views;
using Octokit;
using Plugin.SecureStorage;

namespace GithubXamarin.UWP.Views
{
    [MvxRegion("MainFrame")]
    public sealed partial class LoginView : MvxWindowsPage
    {
        private readonly GitHubClient _client;
        private readonly string _clientId;
        private readonly string _clientSecret;

        private new LoginViewModel ViewModel
        {
            get { return (LoginViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public LoginView()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
            //Values can be found at https://github.com/settings/applications
            
            _clientId = ApiKeysManager.GithubClientId;
            _clientSecret = ApiKeysManager.GithubClientSecret;
            _client = new GitHubClient(new ProductHeaderValue("GitX"));

            while(ApiKeysManager.GithubClientId == null) { }

            var loginRequest = new OauthLoginRequest(_clientId)
            {
                //Scopes = { "user", "notifications", "repo", "delete_repo", "gist", "admin:org" }
                Scopes = { "user", "notifications"}
            };

            var oAuthLoginUrl = _client.Oauth.GetGitHubLoginUrl(loginRequest);
            
            LoginWebView.Navigate(oAuthLoginUrl);
            
            LoginWebView.NavigationCompleted += LoginWebViewOnNavigationCompleted;
        }

        /// <summary>
        /// Method attached to navigation of the web-browser. It must return void
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void LoginWebViewOnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            string argsUri = args.Uri + "&code=12345";
            LoginProgressBar.Value = 25;
            
            //if (1==1) //
            if(args.Uri != null && args.Uri.ToString().Contains("code="))
            {
                LoginProgressBar.Value = 50;

                await CodeRetrieverandTokenSaver(args.Uri.ToString());
                //await CodeRetrieverandTokenSaver(argsUri.ToString());
            }
        }

        /// <summary>
        /// Retrieves code value from a given string and uses it to create access token and then saves it.
        /// </summary>
        /// <param name="retrievedUrl">Url from browser</param>
        /// <returns></returns>
        private async Task CodeRetrieverandTokenSaver(string retrievedUrl)
        {
            //Retrieves code from URL
            var code = retrievedUrl.Split(new[] { "code=" }, StringSplitOptions.None)[1];
            
            code = code.Replace("&state=", string.Empty);
            var tokenRequest = new OauthTokenRequest(_clientId, _clientSecret, code);
            
            var accessToken = await _client.Oauth.CreateAccessToken(tokenRequest);

            if (CrossSecureStorage.Current != null)
            {
                try
                {
                    CrossSecureStorage.Current.SetValue("OAuthToken", accessToken.AccessToken);
                }
                catch 
                {
                    var msgDialog2 = new MessageDialog("Caution", "Unsolvable Token problems detected!");

                    await msgDialog2.ShowAsync();
                    LoginProgressBar.Value = 100;

                    return;
                }
            }
            
            //var msgDialog = new MessageDialog("Choose any page you want to go from the menu on the left or you can just stare at this page. Your choice!", "Login Successful!");
            var msgDialog = new MessageDialog("Information", "Login Successful!");

            await msgDialog.ShowAsync();

            LoginProgressBar.Value = 100;
        }
    }
}
