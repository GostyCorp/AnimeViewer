using System;
using System.Windows;
using System.Windows.Controls;
using Type = AnimeViewer.Models.Type;

namespace AnimeViewer.Pages
{
	public partial class FicheView : UserControl
	{
		public FicheView()
		{
            InitializeComponent();
		}

        private void Button_Click(object sender, RoutedEventArgs e)
		{
			MainWindow main = (MainWindow)Application.Current.MainWindow;
			ListPage listPage = main?.ListPage;
			switch(listPage?.ListType)
			{
				case ListType.Serie:
					listPage.ToggleListType(ListType.Episode);
					listPage.LoadEpisodes(id: (int)IdNumber.Value);
					break;

				case ListType.Episode:
					main?.CreatePlayer(id: (int)IdNumber.Value);
					break;

				default:
					listPage?.LoadSeries();
					break;
			}
		}
    }
}