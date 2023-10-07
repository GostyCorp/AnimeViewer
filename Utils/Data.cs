using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using AnimeViewer.Models;
using HtmlAgilityPack;
using Type = AnimeViewer.Models.Type;

namespace AnimeViewer.Utils
{
	public abstract class Data
	{
		private readonly static SQLiteConnection Sqlite = new SQLiteConnection();

		public static void Init(bool reset = false)
		{
			if(reset)
			{
				if(Sqlite.State == ConnectionState.Open)
				{
					Sqlite.Close();
				}
				if(File.Exists("animeViewer.db"))
				{
					File.Delete("animeViewer.db");
				}
			}

			if(File.Exists("animeViewer.db"))
			{
				OpenSqLite();
				return;
			}

			SQLiteConnection.CreateFile("animeViewer.db");
			OpenSqLite();

			string sql = $"create table Serie (id integer not null constraint Serie_pk primary key autoincrement, title text not null, titleEnglish text, titleRomanji text, titleFrench text, titleOther text, urlVF text, urlVostFr text, urlImage text, genre text);";
			SQLiteCommand liteCommand = new SQLiteCommand(sql, Sqlite);
			liteCommand.ExecuteNonQuery();

			sql = $"create table Episode(id integer not null constraint Episode_pk primary key autoincrement, serie integer not null, number integer not null, type text not null, url text, urlImage text, constraint Episode_pk2 unique (number, serie, type));";
			liteCommand = new SQLiteCommand(sql, Sqlite);
			liteCommand.ExecuteNonQuery();
		}

		private static void OpenSqLite()
		{
			if(Sqlite.State == ConnectionState.Open)
				return;

			if(!File.Exists("animeViewer.db"))
				Init(true);

			Sqlite.ConnectionString = "Data Source=animeViewer.db;Version=3;";
			Sqlite.Open();
		}

		public static void SetData(string sql, Dictionary<string, object> dictionary = null)
		{
			OpenSqLite();
			SQLiteCommand liteCommand = new SQLiteCommand(sql, Sqlite);
			if(dictionary != null)
			{
				foreach(KeyValuePair<string, object> parameter in dictionary)
				{
					liteCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
				}
			}
			liteCommand.ExecuteNonQuery();
		}

		public static SQLiteDataReader GetData(string sql, Dictionary<string, object> dictionary = null)
		{
			OpenSqLite();
			SQLiteCommand liteCommand = new SQLiteCommand(sql, Sqlite);
			if(dictionary == null)
				return liteCommand.ExecuteReader();

			foreach(KeyValuePair<string, object> parameter in dictionary)
			{
				liteCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
			}
			return liteCommand.ExecuteReader();
		}

		public static HtmlNodeCollection GetScrapWebSite(string url, string xPath)
		{
			HtmlWeb webClient = new HtmlWeb();
			Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult);
			if(uriResult == null)
			{
				MessageBox.Show("La série est indisponible", "Erreur de chargement de la série", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}

			HtmlDocument document = webClient.Load(url);
			return document?.DocumentNode.SelectNodes(xPath);
		}

		public static List<Serie> GetSeries(int limit, int lastLimit, Type type, string search = null)
		{
			OpenSqLite();
			List<Serie> series = new List<Serie>();
			string sql = type == Type.Vf ? "SELECT * FROM Serie WHERE urlVF IS NOT NULL" : "SELECT * FROM Serie WHERE urlVostFr IS NOT NULL";
			sql += search != null ? " AND (title LIKE @search OR titleEnglish LIKE @search OR titleRomanji LIKE @search OR titleFrench LIKE @search OR titleOther LIKE @search)" : "";
			sql += " ORDER BY title limit @limit offset @lastLimit;";
			Dictionary<string, object> dictionary = new Dictionary<string, object>()
			{
				{ "@limit", limit },
				{ "@lastLimit", lastLimit }
			};
			if(search != null)
				dictionary.Add("@search", $"%{search}%");
			SQLiteDataReader data = GetData(sql, dictionary);
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
	                    genre: data["genre"].ToString()
					)
				);
			}
			return series;
		}

		public static Serie GetSerie(int id)
		{
			OpenSqLite();
			Serie serie = null;
			string sql = "SELECT * FROM Serie WHERE id = @id;";
			Dictionary<string, object> dictionary = new Dictionary<string, object>()
			{
				{ "@id", id }
			};
			SQLiteDataReader data = GetData(sql, dictionary);
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
					genre: data["genre"].ToString());
			}
			return serie;
		}

		public static List<Episode> GetEpisodes(Serie serie, Type type)
		{
			OpenSqLite();
			List<Episode> episodes = new List<Episode>();
			string sql = $"SELECT * FROM Episode WHERE serie = @serie AND type = @type ORDER BY number;";
			Dictionary<string, object> dictionary = new Dictionary<string, object>()
			{
				{ "@serie", serie.Id },
				{ "@type", type }
			};
			SQLiteDataReader data = GetData(sql, dictionary);
			while(data.Read())
			{
				episodes.Add(new Episode(
						serie: serie,
						number: Convert.ToInt32(data["number"]),
						type: type,
						url: data["url"].ToString(),
						urlImage: data["urlImage"].ToString(),
						id: Convert.ToInt32(data["id"])
					)
				);
			}
			return episodes;
		}

		public static Episode GetEpisode(int id)
		{
			OpenSqLite();
			Episode serie = null;
			string sql = $"SELECT * FROM Episode WHERE id = @id;";
			Dictionary<string, object> dictionary = new Dictionary<string, object>()
			{
				{ "@id", id }
			};
			SQLiteDataReader data = GetData(sql, dictionary);
			while(data.Read())
			{
				serie = new Episode(
					serie: GetSerie(Convert.ToInt32(data["Serie"])),
					number: Convert.ToInt32(data["number"]),
					type: Convert.ToInt32(data["type"]) == 1 ? Type.Vf : Type.Vostfr,
					url: data["url"].ToString(),
					urlImage: data["urlImage"].ToString(),
					id: Convert.ToInt32(data["id"])
				);
			}
			return serie;
		}
	}
}