using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using AnimeViewer.Models;
using AnimeViewer.Pages;
using AnimeViewer.Utils;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace AnimeViewer;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
	public enum FrameType
	{
		Home,
		History,
		SerieList,
		EpisodeList,
		Player
	}

	private readonly HistoryPage _historyPage;
	private readonly ListPage _listPage;
	public readonly EpisodeListPage EpisodeListPage;
	private Player _player;

	public MainWindow()
	{
		InitializeComponent();
		_historyPage = new HistoryPage();
		_listPage = new ListPage();
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

	public Langage Langage { get; private set; } = Langage.Vf;

	public void SwitchFrameView(FrameType frame)
	{
		if(frame != FrameType.Player && _player != null)
		{
			_player.Dispose();
		}
		switch(frame)
		{
			case FrameType.Home:
				PageViewer.Navigate(_historyPage);
				break;

			case FrameType.SerieList:
				PageViewer.Navigate(_listPage);
				_listPage.ViewerScroll.ScrollToTop();
				break;

			case FrameType.EpisodeList:
				PageViewer.Navigate(EpisodeListPage);
				EpisodeListPage.ViewerScroll.ScrollToTop();
				break;

			case FrameType.Player:
				PageViewer.Navigate(_player);
				break;

			case FrameType.History:
				PageViewer.Navigate(_historyPage);
				_historyPage.LoadHistory();
				break;
		}
	}

	public void CreatePlayer(int id)
	{
		Episode episode = Episode.GetEpisode(id);
		Uri.TryCreate(episode.Url, UriKind.Absolute, result: out Uri uriResult);
		if(uriResult == null)
		{
			MessageBox.Show("La vidéo est indisponible", "Erreur de chargement vidéo", MessageBoxButton.OK, MessageBoxImage.Error);
			return;
		}
		_player = new Player(uriResult, episode);
		SwitchFrameView(FrameType.Player);
	}

	private void Search_OnTextChanged(object sender, TextChangedEventArgs e)
	{
		_listPage.Search = (sender as TextBox)?.Text;
		if(_listPage.Search == null || _listPage.Search.Length < 3 && _listPage.Search != "")
			return;

		SwitchFrameView(FrameType.SerieList);
		_listPage.ClearSeries();
		_listPage.LoadSeries();
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
		if(sender is not ComboBox comboBox)
			return;

		switch(comboBox.SelectionBoxItem.ToString())
		{
			case "VostFr":
				Langage = Langage.Vostfr;
				break;
			case "VF":
				Langage = Langage.Vf;
				break;
		}

		SwitchFrameView(FrameType.SerieList);
		_listPage.ToggleType();
	}

	private void HistoryBtn_OnClick(object sender, RoutedEventArgs e)
	{
		SwitchFrameView(FrameType.History);
	}
}