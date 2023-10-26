const bridge = chrome.webview.hostObjects.bridge;

removeAds = function()
{
	// Remove ads
	let anchorElements = document.body.querySelectorAll('a');
	anchorElements.forEach(function(anchorElement)
	{
		anchorElement.remove();
	});
	let scriptElements = document.body.querySelectorAll('script');
	if(scriptElements.length > 0)
	{
		scriptElements[scriptElements.length - 1].remove();
	}
}

setProgressTime = function()
{
	let time = jwplayer().getPosition();
	let maxTime = jwplayer().getDuration();
	bridge.SetVideoTime(time, maxTime);
}

getProgressTime = function()
{
	bridge.GetVideoTime();
	chrome.webview.addEventListener("message", event => {
		let time = event.data;
		if(time == null)
			return false;
		jwplayer().seek(time);
	});
}
AnimeViewerJS = function()
{
	// Fullscreen
	document.getElementsByClassName('jw-icon-fullscreen')[1].addEventListener('click', function()
	{
		bridge.FullScreenToggle();
	});
	document.addEventListener('keydown', function(event)
	{
		if(event.key === "Escape")
		{
			bridge.FullScreenToggle();
		}
	});

	removeAds();

	setTimeout(function()
	{
		getProgressTime();
	}, 1000);

	// Auto play
	jwplayer().play();
	jwplayer().setVolume(50);

	setInterval(function()
	{
		setProgressTime();
	}, 3000)
}

for(let i = 0; i < 3; i++)
{
	let time = i*1000+500;
	setTimeout(function()
	{
		if(jwplayer().getState() !== 'playing')
			AnimeViewerJS();
	}, time);
}