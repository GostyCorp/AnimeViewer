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
using Type = AnimeViewer.Models.Type;

namespace AnimeViewer.Utils
{
	public abstract class NekoSamaScrap
	{
		public static void Init()
		{
			InitAsync();
		}

		private async static void InitAsync()
		{
			Loading loading = new Loading();
			loading.Show();

			await Task.Run(() =>
			{
				loading.SetProgress(25);
				SetAnimeDataNekoSama(GetNekoSamaScraps("vf"), Type.Vf);
				loading.Dispatcher.Invoke(() => loading.SetProgress(75));
				SetAnimeDataNekoSama(GetNekoSamaScraps("vostfr"), Type.Vostfr);
				loading.Dispatcher.Invoke(() => loading.SetProgress(100));
			});

			loading.Close();
			MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
			mainWindow?.Show();
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
			Uri url;
			switch(mode)
			{
				case "vf":
					url = new Uri("https://185.146.232.127/animes-search-vf.json");
					break;

				case "vostfr":
					url = new Uri("https://185.146.232.127/animes-search-vostfr.json");
					break;

				default:
					url = new Uri("https://185.146.232.127/animes-search-vf.json");
					break;
			}

			string web = WebAccess(url);
			return JsonConvert.DeserializeObject<List<AnimeNekoSama>>(web);
		}

		private static void SetAnimeDataNekoSama(List<AnimeNekoSama> list, Type type)
		{
			foreach(AnimeNekoSama animeNekoSama in list)
			{
				bool exist = false;
				SQLiteDataReader liteDataReader = Data.GetData("select id, urlVF, urlVostFr from Serie where title = @title", new Dictionary<string, object>
				{ { "@title", animeNekoSama.Title } });
				bool urlExist = false;
				while(liteDataReader.Read())
				{
					animeNekoSama.Id = Convert.ToInt32(liteDataReader["id"]);
					exist = true;
					if(liteDataReader["urlVF"] != DBNull.Value && type == Type.Vf || liteDataReader["urlVostFr"] != DBNull.Value && type == Type.Vostfr)
						urlExist = true;
				}
				if(urlExist)
					continue;

				string sql;
				Dictionary<string, object> dictionary;
				switch(exist)
				{
					case true when type == Type.Vostfr:
						sql = "update Serie set urlVostFr = @url where id = @id;";
						dictionary = new Dictionary<string, object>
						{
							{ "@id", animeNekoSama.Id },
							{ "@url", animeNekoSama.Url }
						};
						Data.SetData(sql, dictionary);
						continue;
					case true:
						continue;
				}

				sql = type == Type.Vf ? "insert into Serie (title, titleEnglish, titleRomanji, titleFrench, titleOther, urlVF, urlImage, genre) values (@title, @titleEnglish, @titleRomanji, @titleFrench, @titleOther, @url, @urlImage, @genre);" :
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
			}
		}

		public static void GetEpisodeScrapingNekoSama(Serie serie, Type type)
		{
			HtmlNodeCollection nodes = Data.GetScrapWebSite(type == Type.Vf ? serie.UrlVf : serie.UrlVostFr, "//script[@type='text/javascript']");
			Match match = Regex.Match(nodes[1].InnerHtml, @"var episodes = (.*);");
			string listEpisodes = match.Success ? match.Groups[1].Value : null;

			if(listEpisodes == null)
				return;

			List<EpisodeNekoSama> episodeNekoSamas = JsonConvert.DeserializeObject<List<EpisodeNekoSama>>(listEpisodes);
			SetEpisodeDataNekoSama(episodeNekoSamas, serie, type);
		}

		private static void SetEpisodeDataNekoSama(List<EpisodeNekoSama> list, Serie serie, Type type)
		{
			foreach(EpisodeNekoSama episodeNekoSama in list)
			{
				Episode episode = new Episode(serie, episodeNekoSama.Num, type, episodeNekoSama.Url, episodeNekoSama.UrlImage);
				bool exist = false;

				SQLiteDataReader liteDataReader = Data.GetData("select id, url from Episode where serie = @serie AND number = @number AND type = @type", new Dictionary<string, object>
				{ { "@serie", serie.Id }, { "@number", episodeNekoSama.Num }, { "@type", type } });

				bool episodeExist = false;
				while(liteDataReader.Read())
				{
					episode.Id = Convert.ToInt32(liteDataReader["id"]);
					exist = true;
					if(liteDataReader["url"] != DBNull.Value)
						episodeExist = true;
				}
				if(episodeExist)
					continue;

				HtmlNodeCollection script = Data.GetScrapWebSite($"https://neko-sama.fr{episodeNekoSama.Url}", "//script[@type='text/javascript']");
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
							{ "@type", episode.Type },
							{ "@url", episode.Url },
							{ "@urlImage", episode.UrlImage }
						};
						Data.SetData(sql, dictionary);
						break;
				}
			}
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