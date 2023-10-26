using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using HtmlAgilityPack;

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
			SQLiteCommand liteCommand = new SQLiteCommand(File.ReadAllText("Script/db.sql"), Sqlite);
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
	}
}