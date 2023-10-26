using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AnimeViewer.Models;

namespace AnimeViewer.Pages
{
	/// <summary>
	/// Logique d'interaction pour userPage.xaml
	/// </summary>
	public partial class HistoryPage
	{
		private readonly ObservableCollection<Serie> _series = new ObservableCollection<Serie>();

		public HistoryPage()
		{
			InitializeComponent();
			SerieListView.DataContext = _series;
		}

		public void LoadHistory()
		{
			List<History> histories = History.GetListHistory();
			_series.Clear();
			foreach(History history in histories)
			{
				if(_series.All(serie => serie.Id != history.Episode.Serie.Id))
				{
					_series.Add(history.Episode.Serie);
				}
			}
		}
	}
}
