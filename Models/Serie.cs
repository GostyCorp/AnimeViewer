using System;
using System.Collections.Generic;

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

		public Serie(int id, string title, string titleEnglish, string titleRomanji, string titleFrench, string titleOther, string urlVf, string urlVostFr, string urlImage, string genre)
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
		}

		public List<Episode> getEpisodes(Type type)
		{
			string sql = $"SELECT * FROM Episode WHERE Serie = {Id} AND type = {type};";
			return new List<Episode>();
		}
	}
}