using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace TweetBackgroundNotifier
{
    public sealed class AuthenticationServiceRT
    {
        private const string ConstLocalDataCredentials = "LocalDataCredentials";
        private const string ConstAuthorizer = "Authorizer";
        WinRtAuthorizer _authorizer;
        WinRtCredentials _credentials;

        public IAsyncAction AuthorizeAsync()
        {
            return AuthorizeAsyncResult().AsAsyncAction();
        }

        private async Task AuthorizeAsyncResult()
        {
            Task<WinRtAuthorizer> task = Authenticate();
            await task.ContinueWith(wrt =>
            {
                _authorizer = wrt.Result;
                if (_authorizer.IsAuthorized)
                {
                    _credentials.Save();
                }
            });
        }

        private async Task<WinRtAuthorizer> Authenticate()
        {
            _credentials = await LocalDataCredentials.GetWinRtCredentialsAsync(ApplicationData.Current.LocalFolder);
            if (_credentials.ToString().Equals(",,,,,"))
            {
                _credentials.ConsumerKey = "zPPDxXgf25WedMuVpYpw";
                _credentials.ConsumerSecret = "2Bad9VxQYzfsYDaowidBkoRBIq87wtjjFV8FAvcvV8";
                _credentials.AccessToken = "32533776-uwuq1NLOaJSGdFH3qSrPjHp866I3lssiizrGQowXv";
            }
            _authorizer = new WinRtAuthorizer
            {
                Credentials = _credentials,
                UseCompression = true,
                Callback = new Uri("http://www.twittelytics.com")
            };
            if (!_authorizer.IsAuthorized)
            {
                Task<WinRtAuthorizer> task = _authorizer.AuthorizeAsync();
                await task.ContinueWith(t =>
                {
                    _authorizer.ScreenName = _credentials.ScreenName;
                    _authorizer.UserId = _credentials.UserId;
                });
                if (_authorizer.IsAuthorized)
                {
                    return task.Result;
                }
            }
            return _authorizer;
        }

        public bool IsAuthorized
        {
            get
            {
                if (Authorizer != null && ((WinRtAuthorizer)Authorizer).IsAuthorized)
                {
                    return ((WinRtAuthorizer)Authorizer).IsAuthorized;
                }
                else
                {
                    return false;
                }
            }
        }

        public object Authorizer
        {
            get
            {
                return _authorizer;
            }
        }

        public void Logout()
        {
            _credentials.Clear();
            _authorizer = null;
            _credentials = null;
        }


        public string AuthorizedUserScreenName
        {
            get
            {
                if (_authorizer != null && _authorizer.Credentials != null)
                {
                    return _authorizer.Credentials.ScreenName;
                }
                return string.Empty;
            }
        }
    }
}
