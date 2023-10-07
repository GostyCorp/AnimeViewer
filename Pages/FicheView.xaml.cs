using System.Windows;
using System.Windows.Controls;

namespace AnimeViewer.Pages
{
	public partial class FicheView : UserControl
	{
		private readonly MainWindow _main;
		public FicheView()
		{
            InitializeComponent();
			_main = (MainWindow)Application.Current.MainWindow;
		}

        private async void Button_Click(object sender, RoutedEventArgs e)
		{
			_main.SwitchFrameView(MainWindow.FrameType.EpisodeList);
			await _main.EpisodeListPage.LoadEpisodesAsync(id: (int)IdNumber.Value, _main.Type);
		}
    }
}