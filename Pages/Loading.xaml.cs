using System.Windows;

namespace AnimeViewer.Pages
{
	public partial class Loading : Window
	{
		public Loading()
		{
			InitializeComponent();
		}

		public void SetProgress(double progress)
		{
			Dispatcher.Invoke(() =>
			{
				Progress.Value = progress;
			});
		}
	}
}