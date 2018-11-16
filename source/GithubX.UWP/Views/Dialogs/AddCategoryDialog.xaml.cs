﻿using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using GithubX.Shared.Models;
using static GithubX.Shared.Helpers.CollectionExtension;
using static GithubX.UWP.Handlers.FontHandler;

namespace GithubX.UWP.Views.Dialogs
{
	public sealed partial class AddCategoryDialog : ContentDialog
	{
		#region Variables
		private readonly Button[] categoryButtons = new Button[6];
		private readonly ObservableCollection<GradientColor> colorsList = new ObservableCollection<GradientColor>() { new GradientColor() { Color1 = "#5B247A", Color2 = "#00ff00" } };
		private readonly ObservableCollection<string> emojiKeboardList = new ObservableCollection<string>();
		public Category category = new Category();
		#endregion
		#region ctor
		public AddCategoryDialog() => InitializeComponent();

		public AddCategoryDialog(Category cat)
		{
			this.InitializeComponent();
			iconTextBlock.Text = category.Icon;
		}
		#endregion

		private void ContentDialog_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			//set emoji buttons event
			categoryButtons[0] = (Button)FindName("DevButton");
			categoryButtons[1] = (Button)FindName("SmilesButton");
			categoryButtons[2] = (Button)FindName("BalloonButton");
			categoryButtons[3] = (Button)FindName("PizzaButton");
			categoryButtons[4] = (Button)FindName("CarButton");
			categoryButtons[5] = (Button)FindName("HeartButton");
			foreach (var button in categoryButtons)
				button.Click += ChangeCategoryClick;
			ChangeCategoryClick(categoryButtons[0], null);
		}

		private void ChangeCategoryClick(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			var tag = int.Parse(button.Tag.ToString());
			Grid.SetColumn(highlightBorder, tag + 1);
			emojiKeboardList.RemoveAll();
			emojiKeboardList.AddRange(GetEmojiGroup(tag));
		}

		private void EmojiPresenter_ItemClick(object sender, ItemClickEventArgs e) => iconTextBlock.Text = e?.ClickedItem?.ToString();


		private void Color_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (e.ClickedItem is GradientColor color)
			{
				category.Background = color;
				Bindings.Update();
			}
		}

		#region return

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			if (category.Id == 0) category.Id = Services.Utils.GetUnixTime();
		}

		private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => category = null;
		#endregion
	}
}
