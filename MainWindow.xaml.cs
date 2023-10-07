using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using AnimeViewer.Models;
using AnimeViewer.Pages;
using AnimeViewer.Utils;
using MessageBox = System.Windows.MessageBox;
using TextBox = Wpf.Ui.Controls.TextBox;
using Type = AnimeViewer.Models.Type;

namespace AnimeViewer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly UserPage _userPage;
		public readonly ListPage ListPage;
		public readonly EpisodeListPage EpisodeListPage;
		private Player _player;

		public Type Type { get; private set; } = Type.Vf;

		public MainWindow()
		{
			InitializeComponent();
			_userPage = new UserPage();
			ListPage = new ListPage();
			EpisodeListPage = new EpisodeListPage();

			bool isFirstRun = Convert.ToBoolean(ConfigurationManager.AppSettings["IsFirstRun"]);
			if(isFirstRun)
			{
				Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				config.AppSettings.Settings["IsFirstRun"].Value = "false";
				config.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection("appSettings");

				Data.Init(true);
			}
			else
			{
				Data.Init();
			}
			if(isFirstRun)
			{
				ReloadBtn_OnClick(null, null);
			}
			else
			{
				HomeBtn_OnClick(null, null);
			}
		}

		public void SwitchFrameView(FrameType frame)
		{
			if(frame != FrameType.Player && _player != null)
			{
				_player.Dispose();
			}
			switch(frame)
			{
				case FrameType.Home:
					PageViewer.Navigate(_userPage);
					break;

				case FrameType.SerieList:
					PageViewer.Navigate(ListPage);
					ListPage.ViewerScroll.ScrollToTop();
					break;

				case FrameType.EpisodeList:
					PageViewer.Navigate(EpisodeListPage);
					EpisodeListPage.ViewerScroll.ScrollToTop();
					break;

				case FrameType.Player:
					PageViewer.Navigate(_player);
					break;
			}
		}

		public void CreatePlayer(int id)
		{
			Episode episode = Data.GetEpisode(id);
			Uri.TryCreate(episode.Url, UriKind.Absolute, out Uri uriResult);
			if(uriResult == null)
			{
				MessageBox.Show("La vidéo est indisponible", "Erreur de chargement vidéo", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			_player = new Player(uriResult);
			SwitchFrameView(FrameType.Player);
		}

		private void Search_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			ListPage.Search = (sender as TextBox)?.Text;
			if(ListPage.Search == null || ListPage.Search.Length < 3 && ListPage.Search != "")
				return;

			SwitchFrameView(FrameType.SerieList);
			ListPage.ClearSeries();
			ListPage.LoadSeries();
		}

		private void ReloadBtn_OnClick(object sender, RoutedEventArgs e)
		{
			NekoSamaScrap.Init();
		}

		private void HomeBtn_OnClick(object sender, RoutedEventArgs e)
		{
			SwitchFrameView(FrameType.SerieList);
		}

		private void ReturnBtn_OnClick(object sender, RoutedEventArgs e)
		{
			if(PageViewer.NavigationService.CanGoBack)
				PageViewer.NavigationService.GoBack();
			_player?.Dispose();
		}

		private void TypeBox_OnDropDownClosed(object sender, EventArgs e)
		{
			if(!(sender is ComboBox comboBox))
				return;

			switch(comboBox.SelectionBoxItem.ToString())
			{
				case "VostFr":
					Type = Type.Vostfr;
					break;
				case "VF":
					Type = Type.Vf;
					break;
			}

			SwitchFrameView(FrameType.SerieList);
			ListPage.ToggleType();
		}

		public enum FrameType
		{
			Home,
			User,
			SerieList,
			EpisodeList,
			Player
		}
	}
}