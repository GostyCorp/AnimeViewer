using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AnimeViewer.Models;

namespace AnimeViewer.Pages
{
	/// <summary>
	/// Logique d'interaction pour ListAnimePage.xaml
	/// </summary>
	public partial class ListPage
	{
		public string Search { get; set; }
		private int _lastLimit;
		private readonly ObservableCollection<Serie> _series = new ObservableCollection<Serie>();
		private readonly MainWindow _main;

		public ListPage()
		{
			InitializeComponent();
			_main = (MainWindow)Application.Current.MainWindow;
			SerieListView.DataContext = _series;
		}

		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if(e.VerticalOffset / e.ExtentHeight <= 0.7)
				return;

			LoadSeries();
		}

		public void LoadSeries(int limit = 50)
		{
			Serie.GetSeries(limit: limit, lastLimit: _lastLimit, langage: _main.Langage, search: Search).ForEach(serie => _series.Add(serie));
			_lastLimit += limit;
		}

		public void ToggleType()
		{
			ClearSeries();
			LoadSeries();
		}

		public void ClearSeries()
		{
			_series.Clear();
			_lastLimit = 0;
		}
	}
}