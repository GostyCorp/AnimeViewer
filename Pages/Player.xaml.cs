using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;

namespace AnimeViewer.Pages
{
	public partial class Player : Page
	{
		public Player(Uri url)
		{
			InitializeComponent();
			AsyncInit();
			PlayerWeb.Source = url;
		}

		private async void AsyncInit()
		{
			await PlayerWeb.EnsureCoreWebView2Async();
		}

		private void Player_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
		{
			PlayerWeb.CoreWebView2.AddHostObjectToScript("bridge", new PlayerJs(Application.Current.MainWindow as MainWindow));
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
			await PlayerWeb.CoreWebView2.ExecuteScriptAsync(@"
				AnimeViewerJS = function()
				{
					// Fullscreen
				    const bridge = chrome.webview.hostObjects.bridge;
				    document.getElementsByClassName('jw-icon-fullscreen')[1].addEventListener('click', function()
				    {
				        bridge.FullScreenToggle();
				    });
					document.addEventListener('keydown', function(event)
					{
					    if(event.key === ""Escape"")
						{
					        bridge.FullScreenToggle();
					    }
					});
					
					// Remove ads
					let anchorElements = document.body.querySelectorAll('a');
					anchorElements.forEach(function(anchorElement)
					{
					    anchorElement.remove();
					});
					let scriptElements = document.body.querySelectorAll('script');
					if(scriptElements.length > 0)
					{
					    scriptElements[scriptElements.length - 1].remove();
					}
					
					// Auto play
					jwplayer().play();
					jwplayer().setVolume(50);
				}
				
				for(let i = 0; i < 3; i++)
				{
					let time = i*1000+500;
					setTimeout(function() 
					{
						if(jwplayer().getState() !== 'playing')
							AnimeViewerJS();
					}, time);
				}
		    ");
		}
	}

	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ComVisible(true)]
	public class PlayerJs
	{
		private readonly MainWindow _window;

		public PlayerJs(MainWindow window)
		{
			_window = window;
		}

		public void FullScreenToggle()
		{
			_window.Dispatcher.Invoke(() => { _window.WindowState = _window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; });
			_window.TitleBar.Visibility = _window.WindowState == WindowState.Maximized ? Visibility.Hidden : Visibility.Visible;
			_window.Panel.Visibility = _window.WindowState == WindowState.Maximized ? Visibility.Hidden : Visibility.Visible;
			_window.PageViewer.Margin = new Thickness(_window.WindowState == WindowState.Maximized ? 0 : 180, _window.WindowState == WindowState.Maximized ? 0 : 30, 0, 0);
		}
	}
}

