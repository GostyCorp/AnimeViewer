using System.Windows;
using AnimeViewer.Models;

namespace AnimeViewer.Pages;

public partial class FicheView
{
	private readonly MainWindow _main;
	private Serie _serie;

	public FicheView()
	{
		InitializeComponent();
		_main = (MainWindow)Application.Current.MainWindow;
	}

	private async void Button_Click(object sender, RoutedEventArgs e)
	{
		_main.SwitchFrameView(MainWindow.FrameType.EpisodeList);
		await _main.EpisodeListPage.LoadEpisodesAsync(_serie.Id, _serie.Langage);
	}

	private void FicheView_OnLoaded(object sender, RoutedEventArgs e)
	{
		_serie = DataContext as Serie;
	}
}