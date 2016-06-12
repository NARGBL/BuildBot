//frilly gui stuff

function togbuildbotgui(%x)
{
	if(%x)
	{
		if(isobject(BuildBot_LoadingSO))
		{
			if(buildbotgui2.isawake())
				canvas.popdialog(buildbotgui2);
			else
				canvas.pushdialog(buildbotgui2);
		}
		else
		{
			if(buildbotgui.isawake())
				canvas.popdialog(buildbotgui);
			else
			{
				canvas.pushdialog(buildbotgui);

				if(!isfile("config/client/buildbot/BuildBot Favorites.txt"))
				{
					%file = new fileobject();
					%file.openforwrite("config/client/buildbot/BuildBot Favorites.txt");
					%file.close();
					%file.delete();
					buildbot_help();
				}

				if(!$buildbotversionchecked)
				{
					$buildbotversionchecked = 1;
					buildbot_Versioncheck();
				}
			}
		}
		return 1;
	}
	return 0;
}

function buildbot_help()
{
	canvas.pushdialog(buildbotgui4);
	buildbot_helpabout();
	return 1;
}

function buildbot_helpabout()
{
	if(buildbotgui4.isawake())
	{
		buildbot_help.settext("BuildBot version: "@ $BuildBot_Version @"\nBy Nexus BL_ID 4833\n\nBuildBot is a system that reads save files and builds them through your client.\n\nBuildBot can also Duplicate stacks of bricks and save to be reused, as well as load specific events from save files.\n\nPlease send bug reports and suggestions/requests to me at hoff121324@gmail.com");
		return 1;
	}
	return 0;
}

function buildbot_helpinterface()
{
	if(buildbotgui4.isawake())
	{
		buildbot_help.settext("BuildBot uses an advanced system of guis: the Help GUI, the File Browser, the Loading GUI, the Event GUI, and the Options GUI.\nIn the File Browser, you can navigate to the file of your choice, and click load. You can add an opened file to favorites for quicker navigation.\nThe Loading GUI is used while a file has been loaded, and a Build is either starting, or has started.  You can pause or stop the load from this GUI.\nThe Event GUI has its own help button.\nThe Options GUI has many options are provide a large range of functionality and personalization.  For best results, it is reccommended that you use the default settings.");
		return 1;
	}
	return 0;
}

function buildbot_helploading()
{
	if(buildbotgui4.isawake())
	{
		buildbot_help.settext("When loading a build, as soon as you click the load button, you are committed to loading in that direction.  BuildBot uses some algorithms to compensate for your direction, and you should not turn when starting the load.\n\nTo start the load, place a ghost brick on the ground and load the file.  You may need to go to the Loading GUI and click Resume if you have autopause enabled.\n\nCaution!  If you have OverDrive mode enabled in the options interface, BuildBot will not check for errors and will not be able to adjust if the server does not support such fast building.");
		return 1;
	}
	return 0;
}

function buildbot_helpduplicating()
{
	if(buildbotgui4.isawake())
	{
		buildbot_help.settext("To save a build, you use your ghost brick to select the first brick, and press the button bound to duplicate.  The first brick is determined by the brick closest to the ghost brick.  You can only look for bricks of a certain datablock by selecting the Ghost Datablock Only option.  That brick will act as if you hit it with a duplicator, and then it will save the duplication to a file.  BuildBot assumes the closest ghost brick to your player is your ghost brick, so if another player has a ghost brick nearby, it may not work.  If you have autopause enabled, BuildBot will then load the duplication automatically.\nPlease try to be patient while the BuildBot gathers brick information.");
		return 1;
	}
	return 0;
}

function buildbot_togpause()
{
	if(!isobject(BuildBot_LoadingSO))
	{
		echo("A load is not in progress.");
		return 0;
	}

	if(BuildBot_LoadingSO.paused)
	{
		BuildBot_LoadingSO.paused = false;
		buildbot_generatenextbrick();
		buildbot_pause.settext("Pause Load");
		return 1;
	}
	BuildBot_LoadingSO.paused = true;
	buildbot_pause.settext("Resume Load");
	return 1;
}

function buildbot_refresh(%path)
{
	buildbot_filelist.clear();
	buildbot_filelocation.settext(%path);

	for(%a = findFirstFile(%path @ "*"); %a!$=""; %a = findNextFile(%path @ "*"))
	{
		%c = false;
		%item = getsubstr(%a, strlen(%path), strlen(%a));

		if((%x=strpos(%item, "/")) > -1)
		{
			%item = getsubstr(%item, 0, %x+1);
			%text = "Folder:" TAB %item;

			for(%b=0; %b<buildbot_filelist.rowcount(); %b++)
			{
				if(buildbot_filelist.getrowtextbyid(%b) $= %text)
				{
					%c = true;
					break;
				}
			}
		}
		else
		{
			if($buildbot::filter && getsubstr(%item, strlen(%item)-4, 4) !$= ".bls")
				%c = true;
			else
				%text = "File:" TAB %item;
		}

		if(!%c)
			buildbot_filelist.addrow(buildbot_filelist.rowcount(), %text);
	}
	buildbot_filelist.sort(1);
	return 1;
}

function initbuildbot()
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

	if(buildbotgui.isawake())
	{
		canvas.popdialog(buildbotgui);

		if(!$buildbot::autoclose)
			canvas.pushdialog(buildbotgui2);
	}

	if($buildbot::prompt)
	{
		if(getplayerdirection() == 4)
		{
			messageboxok("Warning!", "Please spawn before using BuildBot.\n\nYou can disable this prompt in the options.");
			return 0;
		}
	}
	buildbot_activateloadsequence(%path);
	return 1;
}

function buildbot_favorites()
{
	if(!$buildbotfavs)
	{
		%ext = new GuiSwatchCtrl(buildbot_extension)
		{
			profile = "GuiDefaultProfile";
			horizSizing = "right";
			vertSizing = "height";
			position = "328 24";
			extent = "206 170";
			minExtent = "8 2";
			visible = "1";
			color = "0 0 0 0";

			new GuiMLTextCtrl()
			{
				profile = "GuiMLTextProfile";
				horizSizing = "right";
				vertSizing = "height";
				position = "90 2";
				extent = "64 28";
				minExtent = "8 2";
				visible = "1";
				lineSpacing = "2";
				allowColorChars = "0";
				maxChars = "-1";
				text = "<just:center><font:arial bold:14>Favories\nManager";
				maxBitmapHeight = "-1";
				selectable = "1";
			};

			new GuiBitmapButtonCtrl()
			{
				profile = "BlockButtonProfile";
				horizSizing = "right";
				vertSizing = "bottom";
				position = "46 8";
				extent = "40 18";
				minExtent = "8 2";
				visible = "1";
				command = "buildbot_add();";
				text = "Add >";
				groupNum = "-1";
				buttonType = "PushButton";
				bitmap = "base/client/ui/button1";
				lockAspectRatio = "0";
				alignLeft = "0";
				overflowImage = "0";
				mKeepCached = "0";
				mColor = "255 255 255 255";
			};

			new GuiBitmapButtonCtrl()
			{
				profile = "BlockButtonProfile";
				horizSizing = "right";
				vertSizing = "bottom";
				position = "0 8";
				extent = "40 18";
				minExtent = "8 2";
				visible = "1";
				command = "buildbot_use();";
				text = "< Use";
				groupNum = "-1";
				buttonType = "PushButton";
				bitmap = "base/client/ui/button1";
				lockAspectRatio = "0";
				alignLeft = "0";
				overflowImage = "0";
				mKeepCached = "0";
				mColor = "255 255 255 255";
			};

			new GuiBitmapButtonCtrl()
			{
				profile = "BlockButtonProfile";
				horizSizing = "right";
				vertSizing = "bottom";
				position = "166 8";
				extent = "40 18";
				minExtent = "8 2";
				visible = "1";
				command = "buildbot_delete();";
				text = "Delete";
				groupNum = "-1";
				buttonType = "PushButton";
				bitmap = "base/client/ui/button1";
				lockAspectRatio = "0";
				alignLeft = "0";
				overflowImage = "0";
				mKeepCached = "0";
				mColor = "255 0 0 255";
			};

			new GuiScrollCtrl()
			{
				profile = "GuiScrollProfile";
				horizSizing = "right";
				vertSizing = "bottom";
				position = "0 30";
				extent = "206 140";
				minExtent = "8 2";
				visible = "1";
				willFirstRespond = "0";
				hScrollBar = "alwaysOn";
				vScrollBar = "alwaysOn";
				constantThumbHeight = "0";
				childMargin = "0 0";
				rowHeight = "40";
				columnWidth = "30";

				new GuiTextListCtrl(buildbot_favlist)
				{
					profile = "GuiTextListProfile";
					horizSizing = "right";
					vertSizing = "bottom";
					position = "1 1";
					extent = "189 2";
					minExtent = "8 2";
					visible = "1";
					enumerate = "0";
					resizeCell = "1";
					columns = "0";
					fitParentWidth = "1";
					clipColumnText = "0";
				};
			};
		};
		buildbot_window.resize(getword(buildbot_window.getposition(), 0), getword(buildbot_window.getposition(), 1), 540, getword(buildbot_window.getextent(), 1));
		buildbot_window.add(%ext);
		$buildbotfavs = 1;
		buildbot_popfavs();
		buildbot_favbutton.settext("Favs <<");
	}
	else
	{
		buildbot_extension.delete();
		buildbot_window.resize(getword(buildbot_window.getposition(), 0), getword(buildbot_window.getposition(), 1), 326, getword(buildbot_window.getextent(), 1));
		$buildbotfavs = 0;
		buildbot_favbutton.settext("Favs >>");
	}
	return 1;
}

function buildbot_up()
{
	%path = $buildbotpath;
	%x = 0;

	for(%a=0; %a<getcharcount($buildbotpath, "/")+isfile($buildbotpath); %a++)
		%x = strpos((%path = getsubstr(%path, %x, strlen(%path))), "/")+1;

	$buildbotpath = getsubstr($buildbotpath, 0, strlen($buildbotpath) - strlen(%path));
	buildbot_refresh($buildbotpath);
	return 1;
}

function buildbot_open()
{
	if((%row = buildbot_filelist.getselectedid()) >= 0)
	{
		$buildbotpath = $buildbotpath @ getfield(buildbot_filelist.getrowtextbyid(%row), 1);
		buildbot_refresh($buildbotpath);
		return 1;
	}
	return 0;
}

function buildbot_add()
{
	%path = buildbot_filelocation.getvalue();

	if(isfile(%path))
	{
		%file = new fileobject();

		if(isfile("config/client/buildbot/BuildBot Favorites.txt"))
		{
			%file.openforread("config/client/buildbot/BuildBot Favorites.txt");

			while(!%file.iseof())
			{
				if(%file.readline() $= %path)
				{
					messageboxok("Warning!", "File already in Favorites.\n\n"@%path);
					%file.close();
					%file.delete();
					return 0;
				}
			}
			%file.close();
		}
		%file.openforappend("config/client/buildbot/BuildBot Favorites.txt");
		%file.writeline(%path);
		%file.close();
		%file.delete();
		buildbot_popfavs();
		return 1;
	}
	messageboxok("Warning!", "File not found.\n\n"@%path);
	return 0;
}

function buildbot_use()
{
	if((%row = buildbot_favlist.getselectedid()) >= 0)
	{
		buildbot_filelocation.settext(buildbot_favlist.getrowtextbyid(%row));
		return 1;
	}
	return 0;
}

function buildbot_delete()
{
	if((%row = buildbot_favlist.getselectedid()) >= 0)
	{
		%path = buildbot_favlist.getrowtextbyid(%row);
		%file = new fileobject();
		%file.openforread("config/client/buildbot/BuildBot Favorites.txt");

		while(!%file.iseof())
		{
			if((%line = %file.readline()) !$= %path)
				%data[%a++] = %line;
		}
		%file.close();
		%file.openforwrite("config/client/buildbot/BuildBot Favorites.txt");

		for(%a=%a; %a>0; %a--)
			%file.writeline(%data[%a]);
		%file.close();
		%file.delete();
		buildbot_popfavs();
		return 1;
	}
	return 0;
}

function buildbot_popfavs()
{
	buildbot_favlist.clear();

	if(isfile("config/client/buildbot/BuildBot Favorites.txt"))
	{
		%file = new fileobject();
		%file.openforread("config/client/buildbot/BuildBot Favorites.txt");

		while(!%file.iseof())
			buildbot_favlist.addrow(buildbot_favlist.rowcount(), %file.readline());
		%file.close();
		%file.delete();
	}
	return 1;
}

function buildbot_filelist::onselect()
{
	if($buildbot::doubleclick)
	{
		if(getsimtime() - $buildbotfileclick < 500)
		{
			buildbot_open();
			$buildbotfileclick = 0;
			return 1;
		}
		$buildbotfileclick = getsimtime();
		return 0;
	}
	buildbot_open();
	return 1;
}

function buildbot_favlist::onselect()
{
	if(getsimtime() - $buildbotfavclick < 500)
	{
		buildbot_use();
		$buildbotfavclick = 0;
		return 1;
	}
	$buildbotfavclick = getsimtime();
	return 0;
}

function buildbot_defaults()
{
	$buildbot::autocolor = 1;
	$buildbot::autopause = 1;
	$buildbot::autoclose = 0;
	$buildbot::prompt = 1;
	$buildbot::doubleclick = 1;
	$buildbot::altload = 0;
	$buildbot::firstheight = 0;
	$buildbot::dupfilter = 0;
	$buildbot::opmaxdup = 1;
	$buildbot::plantstart = 1;
	$buildbot::automove = 1;
	$buildbot::overdrive = 0;
	$buildbot::lock = 1;
	$buildbot::home = "saves/";
	$buildbot::delay = 5;
	$buildbot::quickload = "saves/bedroom/demo house.bls";
	$buildbot::maxdup = 2000;
	$buildbot::duppath = "saves/duplications/buildbot duplication";
	buildbot_updateoptions();
}

function buildbot_updateoptions()
{
	buildbot_opcolor.setvalue($buildbot::autocolor);
	buildbot_oppause.setvalue($buildbot::autopause);
	buildbot_opclose.setvalue($buildbot::autoclose);
	buildbot_opprompt.setvalue($buildbot::prompt);
	buildbot_opclick.setvalue($buildbot::doubleclick);
	buildbot_opload.setvalue($buildbot::altload);
	buildbot_opfilter.setvalue($buildbot::filter);
	buildbot_opheight.setvalue($buildbot::firstheight);
	buildbot_opdupfilter.setvalue($buildbot::dupfilter);
	buildbot_opmaxdup.setvalue($buildbot::opmaxdup);
	buildbot_opstart.setvalue($buildbot::plantstart);
	buildbot_opmove.setvalue($buildbot::automove);
	buildbot_opoverdrive.setvalue($buildbot::overdrive);
	buildbot_oplock.setvalue($buildbot::lock);
	buildbot_home.settext($buildbot::home);
	buildbot_delay.settext($buildbot::delay);
	buildbot_quickload.settext($buildbot::quickload);
	buildbot_maxdup.settext($buildbot::maxdup);
	buildbot_duppath.settext($buildbot::duppath);
}

function buildbot_apply()
{
	$buildbot::home = buildbot_home.getvalue();
	$buildbot::quickload = buildbot_quickload.getvalue();
	$buildbot::delay = buildbot_delay.getvalue();
	$buildbot::maxdup = buildbot_maxdup.getvalue();
	$buildbot::duppath = buildbot_duppath.getvalue();
	export("$buildbot::*", "config/client/buildbot/options.cs");
	canvas.popdialog(buildbotgui3);
}

function buildbot_clickload()
{
	canvas.pushdialog(buildbotgui6);
}
