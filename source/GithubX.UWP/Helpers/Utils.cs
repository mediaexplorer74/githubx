﻿using Akavache;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace GithubX.UWP.Helpers
{
	internal class Utils
	{
		public static bool CheckConnection => NetworkInformation.GetInternetConnectionProfile() != null;

		public async Task DownloadFile(string url, string name)
		{
			try
			{
				CancellationToken token;
				Uri source;
				StorageFolder folder = KnownFolders.VideosLibrary;
				if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out source))
					throw new Exception("Invalid URI");
				StorageFile destinationFile;
				destinationFile = await folder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
				var download = new BackgroundDownloader().CreateDownload(source, destinationFile);
				download.Priority = BackgroundTransferPriority.High;
				await download.StartAsync().AsTask(token);
			}
			catch (Exception e) { throw e; }
		}

		public static SolidColorBrush HexToSolidColor(string hex)
		{
			hex = hex.Replace("#", string.Empty);
			if (hex.Length < 8) hex += "FF";
			return new SolidColorBrush(Windows.UI.Color.FromArgb(
				(byte)Convert.ToUInt32(hex.Substring(0, 2), 16),
				(byte)Convert.ToUInt32(hex.Substring(2, 2), 16),
				(byte)Convert.ToUInt32(hex.Substring(4, 2), 16),
				(byte)Convert.ToUInt32(hex.Substring(6, 2), 16)));
		}

		public static string SolidColorToHex(SolidColorBrush solidColorBrush)
			=> string.Format("#{0:X2}{1:X2}{2:X2}", solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B);

		public static int GetUnixTime()
			=> (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

		public static async Task<string> ReadFileFromAsset(string uri)
			=> await FileIO.ReadTextAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri)));

		public static async Task OpenUri(string uri)
			=> await Windows.System.Launcher.LaunchUriAsync(new Uri(uri));

		public static void ChangeHeaderTheme(string resourceKey, string HexColor)
		{
			var cl = (AcrylicBrush)Windows.UI.Xaml.Application.Current.Resources[resourceKey];
			cl.TintColor = cl.FallbackColor = HexToSolidColor(HexColor).Color;
		}

		public static void ChangeHeaderTheme(string resourceKey, Color color)
		{
			var cl = (AcrylicBrush)Windows.UI.Xaml.Application.Current.Resources[resourceKey];
			cl.TintColor = cl.FallbackColor = color;
		}

		public static async Task<string> GetMarkDownReadyAsync(string url, bool fromCache = true)
		{
			var buffer = await BlobCache.LocalMachine.DownloadUrl(url, fetchAlways: !fromCache);
			string md = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
			try
			{
				// Html2Markdown needs HtmlAgilityPack.NugetPkg
				md = new Html2Markdown.Converter().Convert(md).Trim();
				//md = md.Replace("[`", "[").Replace("`]", "]").Replace("<<", "");
				if (md == null || md.Length < 2) return "> 404 🤔";
				return md;
			}
			catch { return md; }
		}
		public static async Task<string> GetMarkDownReadyAsync(string url, string sha)
		{
			var oldSha = await BlobCache.LocalMachine.GetObject<string>("_" + url) ?? "";
			await BlobCache.LocalMachine.InsertObject("_" + url, sha);
			return await GetMarkDownReadyAsync(url, sha.Equals(oldSha));
		}
	}
}
