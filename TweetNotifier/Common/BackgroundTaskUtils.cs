using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace TweetNotifier.Common
{
    public class BackgroundTaskUtils
    {
        public const string SampleBackgroundTaskEntryPoint = "TweetBackgroundNotifier.MonitorTweetsTask";
        public const string SampleBackgroundTaskName = "MonitorTweetsTask";
        public static string SampleBackgroundTaskProgress = "";
        public static bool SampleBackgroundTaskRegistered = false;


        /// <summary>
        /// Register a background task with the specified taskEntryPoint, name, trigger,
        /// and condition (optional).
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point for the background task.</param>
        /// <param name="name">A name for the background task.</param>
        /// <param name="trigger">The trigger for the background task.</param>
        /// <param name="condition">An optional conditional event that must be true for the task to fire.</param>
        public static BackgroundTaskRegistration RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            var builder = new BackgroundTaskBuilder();

            builder.Name = name;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);


            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = builder.Register();

            UpdateBackgroundTaskStatus(name, true);
            UpdateBadge();
            //
            // Remove previous completion status from local settings.
            //
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values.Remove(name);

            return task;
        }

        private static void UpdateBadge()
        {
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", "1");
            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }


        /// <summary>
        /// Unregister background tasks with specified name.
        /// </summary>
        /// <param name="name">Name of the background task to unregister.</param>
        public static void UnregisterBackgroundTasks(string name)
        {
            //
            // Loop through all background tasks and unregister any with SampleBackgroundTaskName or
            // SampleBackgroundTaskWithConditionName.
            //
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == name)
                {
                    cur.Value.Unregister(true);
                }
            }

            UpdateBackgroundTaskStatus(name, false);
        }

        public static void UpdateBackgroundTaskStatus(String name, bool registered)
        {
            switch (name)
            {
                case SampleBackgroundTaskName:
                    SampleBackgroundTaskRegistered = registered;
                    break;
            }
        }

        public static String GetBackgroundTaskStatus(String name)
        {
            var registered = false;
            switch (name)
            {
                case SampleBackgroundTaskName:
                    registered = SampleBackgroundTaskRegistered;
                    break;
            }

            var status = registered ? "Registered" : "Unregistered";

            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(name))
            {
                status += " - " + settings.Values[name].ToString();
            }

            return status;
        }
    }
}
