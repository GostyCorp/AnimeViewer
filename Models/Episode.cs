using System;
using System.Collections.Generic;
using System.Data.SQLite;
using AnimeViewer.Utils;

namespace AnimeViewer.Models
{
	public class Episode
	{
		public int Id { get; set; }
		public Serie Serie { get; set; }
		public int Number { get; set; }
		public Langage Langage { get; set; }
		public string Url { get; set; }
		public string UrlImage { get; set; }

		public Episode(Serie serie, int number, Langage langage, string url, string urlImage, int id = -1)
		{
			Id = id;
			Serie = serie;
			Number = number;
			Langage = langage;
			Url = url;
			UrlImage = urlImage;
		}

		public static List<Episode> GetEpisodes(Serie serie, Langage langage)
		{
			List<Episode> episodes = new List<Episode>();
			const string sql = $"SELECT * FROM Episode WHERE serie = @serie AND type = @type ORDER BY number;";
			Dictionary<string, object> dictionary = new Dictionary<string, object>()
			{
				{ "@serie", serie.Id },
				{ "@type", langage }
			};
			SQLiteDataReader data = Data.GetData(sql, dictionary);
			while(data.Read())
			{
				episodes.Add(new Episode(
					serie: serie,
					number: Convert.ToInt32(data["number"]),
					langage: langage,
					url: data["url"].ToString(),
					urlImage: data["urlImage"].ToString(),
					id: Convert.ToInt32(data["id"])
				));
			}
			return episodes;
		}

		public static Episode GetEpisode(int id)
		{
			Episode episode = null;
			const string sql = $"SELECT * FROM Episode WHERE id = @id;";
			Dictionary<string, object> dictionary = new Dictionary<string, object>()
			{
				{ "@id", id }
			};
			SQLiteDataReader data = Data.GetData(sql, dictionary);
			while(data.Read())
			{
				episode = new Episode(
					serie: Serie.GetSerie(Convert.ToInt32(data["Serie"])),
					number: Convert.ToInt32(data["number"]),
					langage: Convert.ToInt32(data["type"]) == 0 ? Langage.Vostfr : Langage.Vf,
					url: data["url"].ToString(),
					urlImage: data["urlImage"].ToString(),
					id: Convert.ToInt32(data["id"])
				);
			}
			return episode;
		}
	}

	public enum Langage
	{
		Vostfr = 0,
		Vf = 1
	}
}