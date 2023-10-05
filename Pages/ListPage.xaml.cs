using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AnimeViewer.Models;
using AnimeViewer.Utils;
using Type = AnimeViewer.Models.Type;

namespace AnimeViewer.Pages
{
	/// <summary>
	/// Logique d'interaction pour ListAnimePage.xaml
	/// </summary>
	public partial class ListPage : Page
	{
		public string Search { get; set; } = null;
		public ListType ListType { get; set; } = ListType.Serie;
		private Type Type { get; set; } = Type.Vf;

		private int _lastLimit = 0;
		private ObservableCollection<Serie> _series = new ObservableCollection<Serie>();
		private ObservableCollection<Episode> _episodes = new ObservableCollection<Episode>();

		public ListPage()
		{
			InitializeComponent();
			SerieListView.DataContext = _series;
			EpisodeListView.DataContext = _episodes;
		}

		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if(e.VerticalOffset / e.ExtentHeight <= 0.5 || ListType == ListType.Episode)
				return;

			LoadSeries();
		}

		public void LoadSeries(int limit = 50)
		{
			Data.GetSeries(limit: limit, lastLimit: _lastLimit, type: Type, search: Search).ForEach(serie => _series.Add(serie));
			_lastLimit += limit;
		}

		public void LoadEpisodes(int id)
		{
			_episodes.Clear();
			Serie serie = Data.GetSerie(id);
			if(serie != null)
			{
				NekoSamaScrap.GetEpisodeScrapingNekoSama(serie: serie, type: Type);
				Data.GetEpisodes(serie, Type).ForEach(episode => _episodes.Add(episode));
			}
		}

		public void ToggleType(Type type)
		{
			Type = type;
			ClearSeries();
			LoadSeries();
		}

		public void ToggleListType(ListType listType)
		{
			ListType = listType;
			ViewerScroll.ScrollToTop();
			if(listType == ListType.Serie)
			{
				SerieListView.Visibility = Visibility.Visible;
				EpisodeListView.Visibility = Visibility.Hidden;
			}
			else
			{
				EpisodeListView.Visibility = Visibility.Visible;
				SerieListView.Visibility = Visibility.Collapsed;
			}
		}

		public void ClearSeries()
		{
			_series.Clear();
			_lastLimit = 0;
		}

		public void ClearEpisodes()
		{
			_episodes = new ObservableCollection<Episode>();
		}
	}

	public enum ListType
	{
		Serie,
		Episode
	}
}