using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TweetNotifier;
using TweetNotifier.Common;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TweetNotifier
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        AuthenticationService _authenticationService;

        public MainPage()
        {
            this.InitializeComponent();
            _authenticationService = new AuthenticationService();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void RegisterBackgroundTask_Click(object sender, RoutedEventArgs e)
        {
            BackgroundTaskUtils.UnregisterBackgroundTasks(BackgroundTaskUtils.SampleBackgroundTaskName);
            var task = BackgroundTaskUtils.RegisterBackgroundTask(BackgroundTaskUtils.SampleBackgroundTaskEntryPoint,
                                                                              BackgroundTaskUtils.SampleBackgroundTaskName,
                                                                              new SystemTrigger(SystemTriggerType.UserAway, false),
                                                                              null);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        public void Login()
        {
            Task t = _authenticationService.AuthorizeAsync();
            t.ContinueWith(ts =>
            {
                if (_authenticationService.IsAuthorized)
                {
                    AuthStatusText.Text = "Logged in as " + _authenticationService.AuthorizedUserScreenName;
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
