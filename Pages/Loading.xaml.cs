namespace AnimeViewer.Pages;

public partial class Loading
{
	public Loading()
	{
		InitializeComponent();
	}

	public void SetProgress(double progress)
	{
		Dispatcher.Invoke(() => { Progress.Value = progress; });
	}
}