using System.Windows;
using System.Windows.Controls;

namespace AnimeViewer.Pages
{
	public partial class EpisodeFicheView : UserControl
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
	}
}