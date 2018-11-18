﻿using Windows.UI.Xaml.Controls;

namespace GithubX.UWP.Views
{
	public sealed partial class MainPage : Page
	{
		public static Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification NotifyElement { get; set; }

		public MainPage()
		{
			this.InitializeComponent();
			NotifyElement = Notif;
			var acc = Helpers.Api.ApiHandler.LoginFromCache();
			if (acc == null)
				iframe.Navigate(typeof(LoginPage));
			else
				iframe.Navigate(typeof(ListPage), acc);
		}
	}
}
