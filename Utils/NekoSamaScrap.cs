using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AnimeViewer.Models;
using AnimeViewer.Pages;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace AnimeViewer.Utils
{
	public abstract class NekoSamaScrap
	{
		private static Loading _loading;
		private static MainWindow _main;
		public static void Init()
		{
			InitAsync();
		}

		private async static void InitAsync()
		{
			_loading = new Loading();
			_loading.Show();
			_main = (MainWindow)Application.Current.MainWindow;

			await Task.Run(() =>
			{
				_main?.Dispatcher.Invoke(() => _main?.Hide());
				SetAnimeDataNekoSama(GetNekoSamaScraps("vf"), Langage.Vf);
				SetAnimeDataNekoSama(GetNekoSamaScraps("vostfr"), Langage.Vostfr);
				_main?.Dispatcher.Invoke(() => _main?.Show());
			});

			_loading.Close();
		}

		private static string WebAccess(Uri url)
		{
			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
			WebClient webClient = new WebClient
			{
				Encoding = Encoding.UTF8
			};
			string web = webClient.DownloadString(url);

			return web;
		}

		private static List<AnimeNekoSama> GetNekoSamaScraps(string mode)
		{
			Uri url = mode switch
			{
				"vf" => new Uri("https://185.146.232.127/animes-search-vf.json"),
				"vostfr" => new Uri("https://185.146.232.127/animes-search-vostfr.json"),
				_ => new Uri("https://185.146.232.127/animes-search-vf.json")
			};

			string web = WebAccess(url);
			return JsonConvert.DeserializeObject<List<AnimeNekoSama>>(web);
		}

		private static void SetAnimeDataNekoSama(IReadOnlyCollection<AnimeNekoSama> list, Langage langage)
		{
			double pas = (double)50 / list.Count;
			double progress = _loading.Dispatcher.Invoke(() => _loading.Progress.Value);
			Parallel.ForEach(list, animeNekoSama =>
			{
				SQLiteDataReader liteDataReader = Data.GetData("select * from Serie where title = @title", new Dictionary<string, object> { { "@title", animeNekoSama.Title } });
				Serie serie = null;
				while(liteDataReader.Read())
				{
					animeNekoSama.Id = Convert.ToInt32(liteDataReader["id"]);
					serie = new Serie(
						id: Convert.ToInt32(liteDataReader["id"]),
						title: liteDataReader["title"].ToString(),
						titleEnglish: liteDataReader["titleEnglish"].ToString(),
						titleRomanji: liteDataReader["titleRomanji"].ToString(),
						titleFrench: liteDataReader["titleFrench"].ToString(),
						titleOther: liteDataReader["titleOther"].ToString(),
						urlVf: liteDataReader["urlVF"].ToString(),
						urlVostFr: liteDataReader["urlVostFr"].ToString(),
						urlImage: liteDataReader["urlImage"].ToString(),
						genre: liteDataReader["genre"].ToString()
					);
				}
				_loading.Dispatcher.Invoke(() => _loading.SetProgress(progress += pas));

				string sql;
				Dictionary<string, object> dictionary;
				switch(serie)
				{
					case not null when langage == Langage.Vostfr:
						sql = "update Serie set urlVostFr = @url where id = @id;";
						dictionary = new Dictionary<string, object>
						{
							{ "@id", animeNekoSama.Id },
							{ "@url", animeNekoSama.Url }
						};
						Data.SetData(sql, dictionary);
						return;

					case not null when langage == Langage.Vf:
						sql = "update Serie set urlVF = @url where id = @id;";
						dictionary = new Dictionary<string, object>
						{
							{ "@id", animeNekoSama.Id },
							{ "@url", animeNekoSama.Url }
						};
						Data.SetData(sql, dictionary);
						return;
				}

				sql = langage == Langage.Vf ? "insert into Serie (title, titleEnglish, titleRomanji, titleFrench, titleOther, urlVF, urlImage, genre) values (@title, @titleEnglish, @titleRomanji, @titleFrench, @titleOther, @url, @urlImage, @genre);" :
									"insert into Serie (title, titleEnglish, titleRomanji, titleFrench, titleOther, urlVostFr, urlImage, genre) values (@title, @titleEnglish, @titleRomanji, @titleFrench, @titleOther, @url, @urlImage, @genre);";
				dictionary = new Dictionary<string, object>
				{
					{ "@title", animeNekoSama.Title },
					{ "@titleEnglish", animeNekoSama.TitleEnglish },
					{ "@titleRomanji", animeNekoSama.TitleRomanji },
					{ "@titleFrench", animeNekoSama.TitleFrench },
					{ "@titleOther", animeNekoSama.Others },
					{ "@url", animeNekoSama.Url },
					{ "@urlImage", animeNekoSama.UrlImage },
					{ "@genre", animeNekoSama.Genres }
				};
				Data.SetData(sql, dictionary);
			});
		}

		public async static Task GetEpisodeScrapingNekoSamaAsync(Serie serie, Langage langage)
		{
			HtmlNodeCollection nodes = Data.GetScrapWebSite(langage == Langage.Vf ? serie.UrlVf : serie.UrlVostFr, "//script[@type='text/javascript']");
			if(nodes == null)
				return;

			Match match = Regex.Match(nodes[1].InnerHtml, @"var episodes = (.*);");
			string listEpisodes = match.Success ? match.Groups[1].Value : null;

			if(listEpisodes == null)
				return;

			List<EpisodeNekoSama> episodeNekoSamas = JsonConvert.DeserializeObject<List<EpisodeNekoSama>>(listEpisodes);

			_loading = new Loading();
			_loading.Show();
			await Task.Run(() => SetEpisodeDataNekoSama(episodeNekoSamas, serie, langage));
			_loading.Close();
		}

		private static void SetEpisodeDataNekoSama(List<EpisodeNekoSama> list, Serie serie, Langage langage)
		{
			double pas = (double)100 / list.Count;
			double progress = _loading.Dispatcher.Invoke(() => _loading.Progress.Value);
			Parallel.ForEach(list, episodeNekoSama =>
			{
				Episode episode = new Episode(serie, episodeNekoSama.Num, langage, episodeNekoSama.Url, episodeNekoSama.UrlImage);
				bool exist = false;

				SQLiteDataReader liteDataReader = Data.GetData("select id, url from Episode where serie = @serie AND number = @number AND type = @type", new Dictionary<string, object> { { "@serie", serie.Id }, { "@number", episodeNekoSama.Num }, { "@type", (int)langage } });

				while(liteDataReader.Read())
				{
					episode.Id = Convert.ToInt32(liteDataReader["id"]);
					exist = true;
				}
				_loading.Dispatcher.Invoke(() => _loading.SetProgress(progress += pas));

				HtmlNodeCollection script = Data.GetScrapWebSite($"https://neko-sama.fr{episodeNekoSama.Url}", "//script[@type='text/javascript']");
				if(script == null)
					return;

				Match match = Regex.Match(script[2].InnerHtml, @"video\[0\]\s*=\s*'([^']*)';");
				episode.Url = match.Success ? match.Groups[1].Value : null;

				string sql;
				Dictionary<string, object> dictionary;
				switch(exist)
				{
					case true:
						sql = "UPDATE Episode SET url = @url, urlImage = @urlImage WHERE id = @id;";
						dictionary = new Dictionary<string, object>
						{
							{ "@url", episode.Url },
							{ "@urlImage", episode.UrlImage },
							{ "@id", episode.Id }
						};
						Data.SetData(sql, dictionary);
						break;

					case false:
						sql = "INSERT INTO Episode (serie, number, type, url, urlImage) VALUES (@serie, @number, @type, @url, @urlImage);";
						dictionary = new Dictionary<string, object>
						{
							{ "@serie", episode.Serie.Id },
							{ "@number", episode.Number },
							{ "@type", episode.Langage },
							{ "@url", episode.Url },
							{ "@urlImage", episode.UrlImage }
						};
						Data.SetData(sql, dictionary);
						break;
				}
			});
		}
	}

	public class AnimeNekoSama
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string TitleEnglish { get; set; }
		public string TitleRomanji { get; set; }
		public string TitleFrench { get; set; }
		public string Others { get; set; }
		public string Type { get; set; }
		public int Status { get; set; }
		public float Popularity { get; set; }
		public Uri Url { get; set; }
		public string Genres { get; set; }
		public Uri UrlImage { get; set; }
		public float Score { get; set; }
		public string StartDateYear { get; set; }
		public string NbEps { get; set; }

		[JsonConstructor]
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public AnimeNekoSama(int id, string title, string title_english, string title_romanji, string title_french, string others, string type, int status, float popularity, Uri url, string[] genres, Uri url_image, float score, string start_date_year, string nb_eps = null)
		{
			Id = id;
			Title = title;
			TitleEnglish = title_english;
			TitleRomanji = title_romanji;
			TitleFrench = title_french;
			Others = others;
			Type = type;
			Status = status;
			Popularity = popularity;
			Url = new Uri($"https://neko-sama.fr{url}");
			string genreFormatted = genres.Aggregate("[", (current, genre) => current + $"{genre}, ");
			Genres = genreFormatted.Remove(genreFormatted.Length - 1) + "]";
			UrlImage = url_image;
			Score = score;
			StartDateYear = start_date_year;
			NbEps = nb_eps;
		}
	}

	public class EpisodeNekoSama
	{
		public string Time { get; set; }
		public string Episode { get; set; }
		public int Num { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public string UrlImage { get; set; }

		[JsonConstructor]
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public EpisodeNekoSama(string time, string episode, int num, string title, string url, string url_image)
		{
			Time = time;
			Episode = episode;
			Num = num;
			Title = title;
			Url = url;
			UrlImage = url_image;
		}
	}
}