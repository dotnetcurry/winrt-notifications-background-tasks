using LinqToTwitter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetBackgroundNotifier;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace TweetBackgroundNotifier
{

    public sealed class Tweet
    {
        public string id { get; set; }
        public string text { get; set; }
    }

    public sealed class MonitorTweetsTask : IBackgroundTask
    {
        AuthenticationServiceRT _authenticationService = new AuthenticationServiceRT();
        TwitterContext twitterCtx = null;
        BackgroundTaskDeferral _deferral = null;
        IBackgroundTaskInstance _taskInstance = null;
        volatile bool _cancelled = false;
        StreamContent _currentStream;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background " + taskInstance.Task.Name + " Starting...");

            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            _deferral = taskInstance.GetDeferral();
            _taskInstance = taskInstance;
            Login();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancelled = true;
            if (_currentStream != null)
            {
                _currentStream.CloseStream();
            }
        }

        public void Login()
        {
            try
            {
                SendBadgeNotification(98);
                Task t = _authenticationService.AuthorizeAsync().AsTask();
                t.Wait();
                if (_authenticationService.IsAuthorized)
                {
                    ConnectToStream("");
                }
                else
                {
                    SendToastNotification("ALERT: Failed to Authenticate with Twitter, please log in again", "");
                }
            }
            catch (Exception ex)
            {
                SendToastNotification("ERROR: Authentication aborted. Exception - " + ex.Message, "");
                Debug.WriteLine("Logged in blew up " + ex.Message);
            }
        }

        private string ConnectToStream(string message)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;

                twitterCtx = new TwitterContext((WinRtAuthorizer)_authenticationService.Authorizer);
                Debug.WriteLine("\nStreamed Content: \n");
                int count = 0;
                (from strm in twitterCtx.UserStream
                 where strm.Type == UserStreamType.User &&
                     strm.With == "followings"
                 select strm)
                .StreamingCallback(strm =>
                {
                    _currentStream = strm;
                    Debug.WriteLine(strm.Content + "\n" + strm.Error);
                    if (strm.Error == null && !string.IsNullOrEmpty(strm.Content))
                    {
                        Tweet currentTweet = JsonConvert.DeserializeObject<Tweet>(strm.Content, settings);
                        count++;
                        if (strm.Content.Contains(@"@" + _authenticationService.AuthorizedUserScreenName))
                        {
                            SendToastNotification(currentTweet.text, "");
                            SendTileTextNotification(currentTweet.text);
                        }
                        SendBadgeNotification(count);
                    }
                })
                .SingleOrDefault();
            }
            catch (TwitterQueryException ex)
            {
                message = ex.Message;
            }
            return message;
        }

        private void SendBadgeNotification(int count)
        {
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", count.ToString());
            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }

        public void SendToastNotification(string message, string imageName)
        {
            var notificationXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText01);
            var toastElements = notificationXml.GetElementsByTagName("text");
            toastElements[0].AppendChild(notificationXml.CreateTextNode(message));
            if (string.IsNullOrEmpty(imageName))
            {
                imageName = @"Assets/Logo.png";
            }
            var imageElement = notificationXml.GetElementsByTagName("image");
            imageElement[0].Attributes[1].NodeValue = imageName;
            var toastNotification = new ToastNotification(notificationXml);
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        private static void SendTileTextNotification(string tweet)
        {
            var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideText04);
            var tileAttributes = tileXml.GetElementsByTagName("text");
            tileAttributes[0].AppendChild(tileXml.CreateTextNode(tweet));
            var tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

    }
}
