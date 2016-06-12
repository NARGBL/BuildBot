//Wrench Event Loader (WEL) Interfacing

function buildbot_welload()
{
	%load = $buildbot_welload;
	%brick = buildbot_wellist.getselectedid();
	echo("preparing to load: "@ %load SPC %brick);

	if(%load $= "" || %brick $= "")
		return;
	canvas.popdialog(buildbotgui5);
	buildbot_compileevent(%load, %brick);
}

function buildbot_welhelp()
{
	messageboxok("BuildBot Event Loader Help", "To use the BuildBot wrench event loader, you must first load a save file by navigating to it in the main buildbot GUI, selecting the blue Load button, and then opting to load events.\n\nIn this GUI, you then click on the already loaded file you want to look for events in, and then the specific brick.\n\nThe events will be loaded to your wrench clipboard.");
}

function buildbot_welback()
{
	$buildbot_lastwelclick = "";
	$buildbotwellistclick = 0;

	buildbot_welrefresh("");
}

function buildbot_welclickrefresh()
{
	buildbot_welrefresh($buildbot_welload);
}

function buildbot_welrefresh(%load)
{
	buildbot_weldisplay.settext("");
	buildbot_wellist.clear();
	$buildbot_welload = %load;

	if(%load $= "")
	{
		for(%i=0; %i<BuildBot_WrenchSO.loadcount; %i++)
			buildbot_wellist.addrow(%i, buildbot_getlastfilepart(BuildBot_WrenchSO.loadpath[%i]) TAB BuildBot_WrenchSO.brickcount[%i]);
	}
	else
	{
		for(%i=0; %i<BuildBot_WrenchSO.brickcount[%load]; %i++)
		{
			if(BuildBot_WrenchSO.brickname[%load, %i] !$= "")
				%name = BuildBot_WrenchSO.brickname[%load, %i];
			else
				%name = BuildBot_WrenchSO.brickdtb[%load, %i];
			buildbot_wellist.addrow(%i, %name TAB BuildBot_WrenchSO.eventcount[%load, %i]);
		}
	}
	buildbot_welinfo();
}

function buildbot_clickloadevents()
{
	%path = buildbot_filelocation.getvalue();

	if(!isfile(%path))
	{
		if((%row = buildbot_filelist.getselectedid()) >= 0)
			%path = $buildbotpath @ getfield(buildbot_filelist.getrowtextbyid(%row), 1);

		if(!isfile(%path))
		{
			messageboxok("Warning!", "File not found.\n\n"@%path);
			return 0;
		}
	}
	buildbot_loadevents(%path);

	if(buildbotgui.isawake())
		canvas.popdialog(buildbotgui);

	canvas.pushdialog(buildbotgui5);
	buildbot_welrefresh(BuildBot_WrenchSO.loadcount-1);
}

function buildbot_Wellist::onselect()
{
	echo("selected: "@ buildbot_wellist.getselectedid());

	if(getsimtime() - $buildbotwellistclick < 500 && buildbot_wellist.getselectedid() == $buildbot_lastwelclick)
	{
		if($buildbot_welload !$= "")
		{
			buildbot_welload();
			$buildbot_lastwelclick = "";
			$buildbotwellistclick = 0;
		}
		else
			buildbot_openwel();
	}
	else
	{
		$buildbotwellistclick = getsimtime();
		$buildbot_lastwelclick = buildbot_wellist.getselectedid();
	}
	buildbot_welinfo();
}

function buildbot_welinfo()
{
	%load = $buildbot_welload;
	//%font = "<font:lucidia console:16>";

	if(%load $= "")
	{
		%load = buildbot_wellist.getselectedid();

		if(%load < 0)
		{
			BuildBot_weldisplay.settext(%font @"Click on a save file in the list to proceed.");
			return;
		}
		%line0 = "Folder:         "@ getsubstr(BuildBot_WrenchSO.loadpath[%load], 0, strlen(BuildBot_WrenchSO.loadpath[%load])-strlen(buildbot_getlastfilepart(BuildBot_WrenchSO.filepath[%load])));
		%line1 = "Name:           "@ buildbot_getlastfilepart(BuildBot_WrenchSO.loadpath[%load]);
		%line2 = "Evented Bricks: "@ BuildBot_WrenchEventSO.brickcount[%load];
		%line3 = "   -----=====-----";
		%line4 = "Double Click on this file to view the list of bricks in the file that contain wrench events.";
		BuildBot_weldisplay.settext(%font @ %line0 NL %line1 NL %line2 NL %line3 NL %line4);
	}
	else
	{
		%brick = buildbot_wellist.getselectedid();

		if(%brick < 0)
		{
			BuildBot_weldisplay.settext(%font @"Click on a brick in the list to view information about it.");
			return;
		}
		%name = BuildBot_WrenchSO.brickname[%load, %brick];

		if(%name $= "")
			%name = "[unnamed brick]";
		%line0 = "Save File:       "@ BuildBot_WrenchSO.loadpath[%load];
		%line1 = "Brick Name:      "@ %name;
		%line2 = "Brick Datablock: "@ BuildBot_WrenchSO.brickdtb[%load, %brick];
		%line3 = "Event Lines:     "@ BuildBot_WrenchSO.eventcount[%load, %brick];
		%line4 = "   -----=====-----";

		for(%i=0; %i<BuildBot_WrenchSO.eventcount[%load, %brick]; %i++)
			%line[%i+5] = strreplace(BuildBot_WrenchSO.eventline[%load, %brick, %i], "\t", ">");

		for(%a=1; %a<(%i+5); %a++)
			%line0 = %line0 NL %line[%a];
		BuildBot_weldisplay.settext(%font @ %line0);
	}
}

function buildbot_openwel()
{
	$buildbot_lastwelclick = "";
	$buildbotwellistclick = 0;

	if((%row = buildbot_wellist.getselectedid()) >= 0)
	{
		buildbot_welrefresh(%row);
		return 1;
	}
	return 0;
}
