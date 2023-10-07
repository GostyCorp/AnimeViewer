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
		private int _lastLimit = 0;
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
			if(e.VerticalOffset / e.ExtentHeight <= 0.1)
				return;

			LoadSeries();
		}

		public void LoadSeries(int limit = 30)
		{
			Data.GetSeries(limit: limit, lastLimit: _lastLimit, type: _main.Type, search: Search).ForEach(serie => _series.Add(serie));
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