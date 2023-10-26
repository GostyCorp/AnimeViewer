using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using AnimeViewer.Models;
using Microsoft.Web.WebView2.Core;
using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace AnimeViewer.Pages
{
	public partial class Player
	{
		private readonly Episode _episode;
		public Player(Uri url, Episode episode)
		{
			InitializeComponent();
			AsyncInit();
			PlayerWeb.Source = url;
			_episode = episode;
		}

		private async void AsyncInit()
		{
			await PlayerWeb.EnsureCoreWebView2Async();
		}

		private void Player_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
		{
			PlayerWeb.CoreWebView2.AddHostObjectToScript("bridge", new PlayerJs(Application.Current.MainWindow as MainWindow, PlayerWeb, _episode));
			PlayerWeb.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
		}

		private static void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
		{
			e.Handled = true;
		}

		public void Dispose()
		{
			PlayerWeb.Dispose();
		}

		private async void PlayerWeb_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			/* @lang JavaScript */
			await PlayerWeb.CoreWebView2.ExecuteScriptAsync(File.ReadAllText("Script/player.js"));
		}
	}

	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ComVisible(true)]
	public class PlayerJs
	{
		private readonly MainWindow _window;
		private readonly WebView2 _webView2;
		private readonly Episode _episode;

		public PlayerJs(MainWindow window, WebView2 webView2, Episode episode)
		{
			_window = window;
			_webView2 = webView2;
			_episode = episode;
		}

		public void FullScreenToggle()
		{
			_window.Dispatcher.Invoke(() => { _window.WindowState = _window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; });
			_window.TitleBar.Visibility = _window.WindowState == WindowState.Maximized ? Visibility.Hidden : Visibility.Visible;
			_window.Panel.Visibility = _window.WindowState == WindowState.Maximized ? Visibility.Hidden : Visibility.Visible;
			_window.PageViewer.Margin = new Thickness(_window.WindowState == WindowState.Maximized ? 0 : 180, _window.WindowState == WindowState.Maximized ? 0 : 30, 0, 0);
		}

		public void SetVideoTime(string time, string maxTime)
		{
			History.SetHistory(episode: _episode, time: Convert.ToSingle(time), maxTime: Convert.ToSingle(maxTime));
		}

		public void GetVideoTime()
		{
			History history = History.GetHistory(episode: _episode);
			if(history != null)
				_webView2.CoreWebView2.PostWebMessageAsString(history.Time.ToString(CultureInfo.InvariantCulture));
        }
	}
}

