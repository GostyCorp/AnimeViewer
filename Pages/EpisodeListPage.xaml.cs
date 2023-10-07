using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using AnimeViewer.Models;
using AnimeViewer.Utils;

namespace AnimeViewer.Pages
{
	public partial class EpisodeListPage : Page
	{
		private readonly ObservableCollection<Episode> _episodes = new ObservableCollection<Episode>();

		public EpisodeListPage()
		{
			InitializeComponent();
			EpisodeListView.DataContext = _episodes;
		}

		public async Task LoadEpisodesAsync(int id, Type type)
		{
			_episodes.Clear();
			Serie serie = Data.GetSerie(id);
			if(serie == null)
				return;

			await NekoSamaScrap.GetEpisodeScrapingNekoSamaAsync(serie: serie, type: type);
			Data.GetEpisodes(serie, type).ForEach(episode => _episodes.Add(episode));
		}
	}
}