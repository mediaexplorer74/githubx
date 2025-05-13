﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GithubXamarin.UWP.Services;
using MvvmCross.Platform;
using Plugin.SecureStorage;
using UniversalRateReminder;

namespace GithubXamarin.UWP
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            SetTheme();

            //HockeyApp Integration
            //HockeyClient.Current.Configure("0fc1c4757f2f41a8b88fcf2dc1603f5b");

            Current.UnhandledException += Current_UnhandledException;
        }

        private async void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
#if DEBUG
            e.Handled = false;
#else
            e.Handled = true;
#endif
            var contentDialog = new ContentDialog()
            {
                Title = "Uh-oh. An error has occured :(",
                Content = e.Message,
                PrimaryButtonText = "No Problem!",
                SecondaryButtonText = ""
            };
            await contentDialog.ShowAsync();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;
            //WinSecureStorageBase.StoragePassword = "12345";

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    await SetStatusBarVisibility();
                    await RegisterGithubNotificationsBackgroundTask();
                    await RegisterMarkNotificationAsReadBackgroundTask();
                    var setup = new Setup(rootFrame);
                    setup.Initialize();

                    var start = Mvx.Resolve<MvvmCross.Core.ViewModels.IMvxAppStart>();
                    start.Start();
                }
                // Ensure the current window is active
                Window.Current.Activate();
                AuthenticationService.Authenticate();

                //Ask user to rate app
                /*
                RatePopup.Title = "Rate GithubX!";
                var result = await RatePopup.CheckRateReminderAsync();
                if (result == RateReminderResult.Dismissed)
                {
                    FeedbackPopup.ContactEmail = "me@nm.ru";
                    FeedbackPopup.EmailSubject = "Feedback for GithubX";
                    FeedbackPopup.EmailBody = "";
                    FeedbackPopup.Title = "Would you like to send feedback?";
                    FeedbackPopup.Content = "Maybe you want to send me an email with feedback regarding your experience with GitHubX?";
                    FeedbackPopup.SendFeedbackButtonText = "yes, send feedback";
                    FeedbackPopup.CancelButtonText = "no, thanks";
                    await FeedbackPopup.ShowFeedbackDialogAsync();
                }
                */
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }



        private async Task RegisterGithubNotificationsBackgroundTask()
        {
            // Register GithubNotificationsBackgroundTask
            const string taskName = "GithubNotificationsBackgroundTask";
            var localSettingsValues = ApplicationData.Current.LocalSettings.Values;

            var taskRegistered = BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == taskName);

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();

                var access = await BackgroundExecutionManager.RequestAccessAsync();
                switch (access)
                {
                    case BackgroundAccessStatus.DeniedByUser:
                    case BackgroundAccessStatus.DeniedBySystemPolicy:
                        break;
                    default:
                        builder.Name = taskName;
                        builder.SetTrigger(new TimeTrigger(15, false));
                        builder.IsNetworkRequested = true;
                        builder.TaskEntryPoint =
                            typeof(Background.GithubNotificationsBackgroundTask).FullName;
                        var task = builder.Register();
                        localSettingsValues["BackgroundTaskTime"] = 15;
                        break;
                }
            }
        }

        private async Task RegisterMarkNotificationAsReadBackgroundTask()
        {
            // Register GithubNotificationsBackgroundTask
            const string taskName = "MarkNotificationAsReadBackgroundTask";

            var taskRegistered = BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == taskName);

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();

                var access = await BackgroundExecutionManager.RequestAccessAsync();
                switch (access)
                {
                    case BackgroundAccessStatus.DeniedByUser:
                    case BackgroundAccessStatus.DeniedBySystemPolicy:
                        break;
                    default:
                        builder.Name = taskName;
                        builder.SetTrigger(new ToastNotificationActionTrigger());
                        builder.IsNetworkRequested = true;
                        builder.TaskEntryPoint =
                            typeof(Background.MarkNotificationAsReadBackgroundTask).FullName;
                        var task = builder.Register();
                        break;
                }
            }
        }

        private void SetTheme()
        {
            var localSettingsValues = ApplicationData.Current.LocalSettings.Values;
            if (localSettingsValues == null) return;

            if (localSettingsValues.ContainsKey("RequestedTheme"))
            {
                switch (localSettingsValues["RequestedTheme"].ToString())
                {
                    case "Dark":
                        Application.Current.RequestedTheme = ApplicationTheme.Dark;
                        break;
                    case "Light":
                        Application.Current.RequestedTheme = ApplicationTheme.Light;
                        break;
                    case "System":
                        break;
                }
            }
            else
            {
                localSettingsValues.Add("RequestedTheme", "Dark");
                Application.Current.RequestedTheme = ApplicationTheme.Dark;
            }
        }

        private async Task SetStatusBarVisibility()
        {
            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                return;

            var localSettingsValues = ApplicationData.Current.LocalSettings.Values;
            if (localSettingsValues == null) return;
            var statusBar = StatusBar.GetForCurrentView();
            if (statusBar == null) return;

            if (localSettingsValues.ContainsKey("StatusBarVisibility"))
            {
                switch (localSettingsValues["StatusBarVisibility"].ToString())
                {
                    case "Hidden":
                        await statusBar.HideAsync();
                        break;
                    case "Visible":
                        await statusBar.ShowAsync();
                        break;
                }
            }
            else
            {
                localSettingsValues.Add("StatusBarVisibility", "Hidden");
                await statusBar.HideAsync();
            }
        }

    }
}
