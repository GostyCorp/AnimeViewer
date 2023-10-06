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
		private readonly UserPage _userPage = new UserPage();
		public readonly ListPage ListPage = new ListPage();
		private Player _player;

		public FrameType Frame = FrameType.Home;

		public MainWindow()
		{
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
			InitializeComponent();
			if(isFirstRun)
			{
				ReloadBtn_OnClick(null, null);
			}
			else
			{
				HomeBtn_OnClick(null, null);
			}
		}

		private void SwitchFrameView(FrameType frame)
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

				case FrameType.List:
					PageViewer.Navigate(ListPage);
					ListPage.ToggleListType(ListType.Serie);
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
			if(ListPage.Search == null || ListPage.Search.Length < 3)
				return;

			SwitchFrameView(FrameType.List);
			ListPage.ClearSeries();
			ListPage.LoadSeries();
		}

		private void ReloadBtn_OnClick(object sender, RoutedEventArgs e)
		{
			NekoSamaScrap.Init();
		}

		private void HomeBtn_OnClick(object sender, RoutedEventArgs e)
		{
			SwitchFrameView(FrameType.List);
		}

		private void ReturnBtn_OnClick(object sender, RoutedEventArgs e)
		{
			if(PageViewer.NavigationService.CanGoBack)
				PageViewer.NavigationService.GoBack();
		}

		private void TypeBox_OnDropDownClosed(object sender, EventArgs e)
		{
			if(!(sender is ComboBox comboBox))
				return;

			switch(comboBox.SelectionBoxItem.ToString())
			{
				case "VostFr":
					ListPage.ToggleType(Type.Vostfr);
					break;
				case "VF":
					ListPage.ToggleType(Type.Vf);
					break;
			}

			SwitchFrameView(FrameType.List);
			ListPage.ClearSeries();
			ListPage.LoadSeries();
		}

		public enum FrameType
		{
			Home,
			User,
			List,
			Player
		}
	}
}