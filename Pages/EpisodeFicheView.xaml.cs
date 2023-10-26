using System.Windows;
using AnimeViewer.Models;

namespace AnimeViewer.Pages
{
	public partial class EpisodeFicheView
	{
        private readonly MainWindow _main;
		public EpisodeFicheView()
		{
			InitializeComponent();
            _main = (MainWindow)Application.Current.MainWindow;
		}

		private void Button_OnClick(object sender, RoutedEventArgs e)
		{
			_main?.CreatePlayer(id: (int)IdNumber.Value);
		}

		private void GetProgress()
		{
			History history = History.GetHistory(Episode.GetEpisode((int)IdNumber.Value));
			Progress.Value = history?.Time ?? 0;
			Progress.Maximum = history?.MaxTime ?? 100;
			if(Progress.Value != 0)
			{
				Progress.Visibility = Visibility.Visible;
			}
		}

		private void EpisodeFicheView_OnLoaded(object sender, RoutedEventArgs e)
		{
			GetProgress();
		}
	}
}