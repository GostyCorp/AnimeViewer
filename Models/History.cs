using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using AnimeViewer.Utils;

namespace AnimeViewer.Models
{
	public class History
	{
		private int Id { get; set; }
		private Episode Episode { get; set; }
		public float Time { get; set; }
		public float MaxTime { get; set; }
		public DateTime Date { get; set; }

		private History(int id, Episode episode, float time, float maxTime, DateTime date)
		{
			Id = id;
			Episode = episode;
			Time = time;
			MaxTime = maxTime;
			Date = date;
		}

		public static void SetHistory(Episode episode, float time, float maxTime)
		{
			History history = GetHistory(episode);
			if(history == null)
			{
				const string sql = $"INSERT INTO History (episode, time, maxTime, date) VALUES (@episode, @time, @maxTime, @date);";
				Dictionary<string, object> dictionary = new Dictionary<string, object>()
				{
					{ "@episode", episode.Id },
					{ "@time", time },
					{ "@maxTime", maxTime },
					{ "@date", ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() }
				};
				Data.SetData(sql, dictionary);
			}
			else
			{
				const string sql = $"UPDATE History SET time = @time, maxTime = @maxTime, episode = @episode, date = @date WHERE id = @id;";
				Dictionary<string, object> dictionary = new Dictionary<string, object>()
				{
					{ "@id", history.Id },
					{ "@episode", episode.Id },
					{ "@time", time },
					{ "@maxTime", maxTime },
					{ "@date", ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() }
				};
				Data.SetData(sql, dictionary);
			}
		}

		public static List<History> GetListHistory()
		{
			List<History> histories = new List<History>();
			const string sql = $"SELECT * FROM History;";

			SQLiteDataReader data = Data.GetData(sql);
			while(data.Read())
			{
				histories.Add(new History(
					id : Convert.ToInt32(data["id"]),
					episode: Episode.GetEpisode(Convert.ToInt32(data["episode"])),
					time: Convert.ToSingle(data["time"]),
					maxTime: Convert.ToSingle(data["maxTime"]),
					date: DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(data["date"])).LocalDateTime
				));
			}
			return histories;
		}

		public static History GetHistory(Episode episode)
		{
			const string sql = $"SELECT * FROM History WHERE episode = @episode;";
			Dictionary<string, object> dictionary = new Dictionary<string, object>()
			{
				{ "@episode", episode.Id }
			};
			SQLiteDataReader data = Data.GetData(sql, dictionary);
			while(data.Read())
			{
				return new History(
					id : Convert.ToInt32(data["id"]),
					episode: Episode.GetEpisode(Convert.ToInt32(data["episode"])),
					time: Convert.ToSingle(data["time"]),
					maxTime: Convert.ToSingle(data["maxTime"]),
					date: DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(data["date"])).LocalDateTime
				);
			}
			return null;
		}
	}
}