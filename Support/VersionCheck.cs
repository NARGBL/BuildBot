//version check system

function BuildBot_versioncheck(%x)
{
	if(!%x && isfile("./rtbinfo.txt") && isfile("add-ons/system_returntoblockland/client.cs"))
		return;
	$BBDownloader_manualconnect = %x;

	if(isobject(BuildBot_Downloader))
		BuildBot_Downloader.delete();

	new tcpobject(BuildBot_Downloader);
	$BBDownloader_downloadphase = 0;
	BuildBot_Downloader.connect("forum.blockland.us:80");
}

function BuildBot_Downloader::onConnected(%this)
{
	if($BBDownloader_downloadphase == 0)
	{
		//forum.blockland.us/index.php?topic=161834.720
		%req = "GET /index.php?topic=161834.720 HTTP/1.0\nHost: forum.blockland.us\n\n";
		%this.send(%req);
	}
	else
	{
		%req = "GET /index.php?action=dlattach;topic=161834.0;attach="@ $BBDownloader_versionfile SPC"HTTP/1.0\nHost: forum.blockland.us\n\n";
		%this.send(%req);
	}
}

function BuildBot_Downloader::onConnectFailed(%this)
{
	if($BBDownloader_manualconnect)
		messageboxok("Attention!", "Unable to connect to the online version information.\n\nYou should check online to make sure BuildBot is up to date.");
}

function BuildBot_Downloader::onDisconnect(%this)
{
	if(!$BBDownloader_connected)
	{
		if($BBDownloader_manualconnect)
			messageboxok("Attention!", "Unable to connect to the online version information.");
	}
	else
		$BBDownloader_connected = "";
}

function BuildBot_Downloader::onLine(%this, %line)
{
	if($BBDownloader_downloadphase == 0)
	{
		if(strpos(%line, "Versions.txt") > -1)
		{
			%subline = "http://forum.blockland.us/index.php?action=dlattach;topic=161834.0;attach=";
			$BBDownloader_versionfile = getsubstr(%line, strpos(%line, %subline)+strlen(%subline), strpos(%line, "\">")-(strpos(%line, %subline)+strlen(%subline)));

			if(BuildBot_isint($BBDownloader_versionfile))
			{
				$BBDownloader_connected = 1;
				BuildBot_Downloader.disconnect();
				$BBDownloader_downloadphase = 1;
				BuildBot_Downloader.connect("forum.blockland.us:80");
			}
			else
				BuildBot_Downloader.disconnect();
		}
	}
	else
	{
		if(getsubstr(%line, 0, 27) $= "NARG MOD VERSION BUILDBOT: ")
		{
			$BBDownloader_connected = 1;
			$BBDownloader_availableversion = getsubstr(%line, 27, strlen(%line)-25);

			if($BBDownloader_availableversion $= $buildbot_version)
				BuildBot_versionresult(1);
			else
				BuildBot_versionresult(0);
			BuildBot_Downloader.disconnect();
		}
	}
}

function BuildBot_versionresult(%x)
{
	if(%x)
	{
		if($BBDownloader_manualconnect)
			messageboxok("Good News!", "BuildBot is up to date.");
	}
	else
		messageboxok("Attention!", "There is a more current version of BuildBot!\n\nYour Version: "@$buildbot_version@"\n\nAvailable: "@$BBDownloader_availableversion);
}