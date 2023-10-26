using System;
using System.Collections.Generic;
using System.Data.SQLite;
using AnimeViewer.Utils;

namespace AnimeViewer.Models
{
	public class Serie
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string TitleEnglish { get; set; }
		public string TitleRomanji { get; set; }
		public string TitleFrench { get; set; }
		public string TitleOther { get; set; }
		public string UrlVf { get; set; }
		public string UrlVostFr { get; set; }
		public string UrlImage { get; set; }
		public string Genre { get; set; }
		public Langage Langage { get; set; }

		public Serie(int id, string title, string titleEnglish, string titleRomanji, string titleFrench, string titleOther, string urlVf, string urlVostFr, string urlImage, string genre, Langage langage)
		{
			Id = id;
			Title = title;
			TitleEnglish = titleEnglish;
			TitleRomanji = titleRomanji;
			TitleFrench = titleFrench;
			TitleOther = titleOther;
			UrlVf = urlVf;
			UrlVostFr = urlVostFr;
			UrlImage = urlImage;
			Genre = genre;
			Langage = langage;
		}

		public static List<Serie> GetSeries(int limit, int lastLimit, Langage langage, string search = null)
		{
			List<Serie> series = new List<Serie>();
			string sql = langage == Langage.Vf ? "SELECT * FROM Serie WHERE urlVF IS NOT NULL" : "SELECT * FROM Serie WHERE urlVostFr IS NOT NULL";
			sql += search != null ? " AND (title LIKE @search OR titleEnglish LIKE @search OR titleRomanji LIKE @search OR titleFrench LIKE @search OR titleOther LIKE @search)" : "";
			sql += " ORDER BY title limit @limit offset @lastLimit;";
			Dictionary<string, object> dictionary = new Dictionary<string, object>()
			{
				{ "@limit", limit },
				{ "@lastLimit", lastLimit }
			};
			if(search != null)
				dictionary.Add("@search", $"%{search}%");
			SQLiteDataReader data = Data.GetData(sql, dictionary);
			while(data.Read())
			{
				series.Add(new Serie(
						id: Convert.ToInt32(data["id"]),
						title: data["title"].ToString(),
						titleEnglish: data["titleEnglish"].ToString(),
	                    titleRomanji: data["titleRomanji"].ToString(),
	                    titleFrench: data["titleFrench"].ToString(),
						titleOther: data["titleOther"].ToString(),
	                    urlVf: data["urlVF"].ToString(),
	                    urlVostFr: data["urlVostFr"].ToString(),
						urlImage:  data["urlImage"].ToString(),
	                    genre: data["genre"].ToString(),
						langage: langage
					)
				);
			}
			return series;
		}

		public static Serie GetSerie(int id, Langage langage)
		{
			Serie serie = null;
			string sql = "SELECT * FROM Serie WHERE id = @id;";
			Dictionary<string, object> dictionary = new Dictionary<string, object>()
			{
				{ "@id", id }
			};
			SQLiteDataReader data = Data.GetData(sql, dictionary);
			while(data.Read())
			{
				serie = new Serie(
					id: Convert.ToInt32(data["id"]),
					title: data["title"].ToString(),
					titleEnglish: data["titleEnglish"].ToString(),
					titleRomanji: data["titleRomanji"].ToString(),
					titleFrench: data["titleFrench"].ToString(),
					titleOther: data["titleOther"].ToString(),
					urlVf: data["urlVF"].ToString(),
					urlVostFr: data["urlVostFr"].ToString(),
					urlImage: data["urlImage"].ToString(),
					genre: data["genre"].ToString(),
					langage: langage
				);
			}
			return serie;
		}
	}
}