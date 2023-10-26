using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AnimeViewer.Models;
using AnimeViewer.Utils;

namespace AnimeViewer.Pages;

public partial class EpisodeListPage
{
	private readonly ObservableCollection<Episode> _episodes = new ObservableCollection<Episode>();

	public EpisodeListPage()
	{
		InitializeComponent();
		EpisodeListView.DataContext = _episodes;
	}

	public async Task LoadEpisodesAsync(int id, Langage langage)
	{
		_episodes.Clear();
		Serie serie = Serie.GetSerie(id, langage);
		if(serie == null)
			return;

		await NekoSamaScrap.GetEpisodeScrapingNekoSamaAsync(serie, langage);
		Episode.GetEpisodes(serie, langage).ForEach(episode => _episodes.Add(episode));
	}
}