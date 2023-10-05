namespace AnimeViewer.Models
{
	public class Episode
	{
		public int Id { get; set; }
		public Serie Serie { get; set; }
		public int Number { get; set; }
		public Type Type { get; set; }
		public string Url { get; set; }
		public string UrlImage { get; set; }

		public Episode(Serie serie, int number, Type type, string url, string urlImage, int id = -1)
		{
			Id = id;
			Serie = serie;
			Number = number;
			Type = type;
			Url = url;
			UrlImage = urlImage;
		}
	}

	public enum Type
	{
		Vostfr,
		Vf
	}
}