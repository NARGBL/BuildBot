//the meat

function buildbot_activateloadsequence(%path)
{
	if(isObject(BuildBot_LoadingSO))
	{
		messageboxok("Warning!", "Loading already in progress, you silly whore!");
		return 0;
	}
	new scriptobject(BuildBot_LoadingSO);

	if($buildbot::autopause && !$buildbotquickload)
	{
		$buildbotautoresume = 1;
		buildbot_togpause();
	}
	$buildbotquickload = 0;

	if(buildbot_loadsavedata(%path))
	{
		BuildBot_LoadingSO.group = serverconnection.getcontrolobject().getcontrollingclient();
		BuildBot_LoadingSO.lastobj = BuildBot_LoadingSO.group.getobject(BuildBot_LoadingSO.group.getcount()-1);
		BuildBot_LoadingSO.unknownplants = 0;
		BuildBot_LoadingSO.plants = 0;
		BuildBot_LoadingSO.dir = getplayerdirection(); //forced to assume a direction

		if($buildbot::automove)
		{
			if($buildbot::altload)
			{
				commandtoserver('buybrick', 0, $uinametable[BuildBot_LoadingSO.brickdtb1]);
				commandtoserver('useinventory', 0);
			}
			else
			{
				commandtoserver('instantusebrick', $uinametable[BuildBot_LoadingSO.brickdtb1]);
			}
		}
		buildbot_generatenextbrick();
		return 1;
	}
	buildbot_stoploadsequence();
	return 0;
}

function buildbot_generatenextbrick()
{
	if(!isObject(BuildBot_LoadingSO))
	{
		echo("Warning!", "A Load is not in progress!");
		return 0;
	}

	if(buildbotgui2.isawake())
	{
		if(!$buildbot::overdrive)
			%text = "\n" @ BuildBot_LoadingSO.plants SPC "successfully planted.";
		buildbot_info.settext(BuildBot_LoadingSO.lastbrick @ "/" @ BuildBot_LoadingSO.brickcount SPC "bricks loaded so far."@%text);
	}

	if(BuildBot_LoadingSO.paused)
		return 0;
	$buildbotautoresume = 0;

	if(buildbot_loadingso.starttime $= "")
		buildbot_loadingso.starttime = getsimtime();

	if(getplayerdirection() != BuildBot_LoadingSO.dir && !BuildBot_LoadingSO.ignore && $buildbot::prompt)
	{
		buildbot_togpause();
		BuildBot_LoadingSO.ignore = 1;
		messageboxok("Warning!", "You have changed direction since you loaded this save! Results may not be satisfactory.\n\nUnpause to continue.");
		return 0;
	}

	if(BuildBot_LoadingSO.lastbrick == BuildBot_LoadingSO.brickcount)
	{
		buildbot_stoploadsequence();
		return;
	}
	%prevpos = BuildBot_LoadingSO.brickpos[BuildBot_LoadingSO.lastbrick];
	BuildBot_LoadingSO.lastbrick++;
	%id = $uinametable[BuildBot_LoadingSO.brickdtb[BuildBot_LoadingSO.lastbrick]];
	%pos = BuildBot_LoadingSO.brickpos[BuildBot_LoadingSO.lastbrick];
	%rot = BuildBot_LoadingSO.brickrot[BuildBot_LoadingSO.lastbrick];

	if(%prevpos $= "")
		%prevpos = %pos;
	%shiftx = getword(%pos,0)-getword(%prevpos,0);
	%shifty = getword(%pos,1)-getword(%prevpos,1);
	%shiftz = getword(%pos,2)-getword(%prevpos,2);

	if($buildbot::altload)
	{
		if(buildbot_loadingso.inventory[buildbot_loadingso.slot++] != %id)
		{
			for(%a=0; %a<10; %a++)
				commandtoserver('buybrick', %a, buildbot_loadingso.inventory[%a] = $uinametable[buildbot_loadingso.brickdtb[buildbot_loadingso.lastbrick + %a]]);
			buildbot_loadingso.slot = 0;
		}
		commandtoserver('useinventory', buildbot_loadingso.slot);
		commandtoserver('shiftbrick', %shiftx, 0, 0);
		commandtoserver('shiftbrick', 0, %shifty, 0);
		commandtoserver('shiftbrick', 0, 0, %shiftz);
	}
	else
	{
		commandtoserver('instantusebrick', %id);
		commandtoserver('shiftbrick', %shiftx, %shifty, %shiftz);
	}

	if(%id $= "")
	{
		buildbot_generatenextbrick();
		return 0;
	}

	if($buildbot::autocolor)
		commandtoserver('usespraycan', findclosestcolor(BuildBot_LoadingSO.colortable[BuildBot_LoadingSO.brickcol[BuildBot_LoadingSO.lastbrick]]));

	for(%a=0; %a<%rot; %a++)
		commandtoserver('rotatebrick', 1);
	commandtoserver('plantbrick');

	for(%a=0; %a<%rot; %a++)
		commandtoserver('rotatebrick', -1);
	BuildBot_LoadingSO.lastbricktime = getsimtime();

	if(!buildbot_isint($buildbot::delay))
		$buildbot::delay = 5;
	BuildBot_LoadingSO.waittick = schedule($buildbot::delay, false, eval, "buildbot_pendingresults();");
	return 1;
}

function buildbot_loadsavedata(%path)
{
	if(!isobject(BuildBot_LoadingSO))
	{
		echo("Warning!", "Loading Sequence has not been started! Terminating...");
		return 0;
	}

	if(isfile(%path))
	{
		%file = new fileobject();
		%file.openforread(%path);
		%file.readline(); //warning
		%dcount = %file.readline(); //number telling us length of description

		for(%a=0; %a<%dcount; %a++)
			BuildBot_LoadingSO.description += %file.readline();

		for(%a=0; %a<64; %a++)
			BuildBot_LoadingSO.colortable[%a] = %file.readline();
		BuildBot_LoadingSO.linecount = getword(%file.readline(), 1);

		while(!%file.iseof())
		{
			if(getsubstr((%line=%file.readline()),0,2) !$= "+-" && (%pos=strpos(%line, "\"")) > -1)
			{
				BuildBot_LoadingSO.brickcount++;
				BuildBot_LoadingSO.brickdtb[BuildBot_LoadingSO.brickcount] = getsubstr(%line, 0, %pos);
				%subline = getsubstr(%line, %pos+2, (strlen(%line)-%pos) + 1);
				BuildBot_LoadingSO.brickpos[BuildBot_LoadingSO.brickcount] = buildbot_getshiftpos(%subline, $uinametable[BuildBot_LoadingSO.brickdtb[BuildBot_LoadingSO.brickcount]]);
				BuildBot_LoadingSO.brickrot[BuildBot_LoadingSO.brickcount] = getword(%subline,3);
				BuildBot_LoadingSO.brickcol[BuildBot_LoadingSO.brickcount] = getword(%subline,5);

				if(BuildBot_LoadingSO.brickpos[BuildBot_LoadingSO.brickcount] $= "0")
					BuildBot_LoadingSO.brickcount--;
			}
		}
		%file.close();
		%file.delete();

		if(!BuildBot_LoadingSO.brickcount)
		{
			messageboxok("Warning!", "File does not contain any brick data.\n\n"@%path);
			return 0;
		}

		if($buildbot::firstheight)
			messageboxok("Attention", "The height of the first brick is" SPC $uinametable[BuildBot_LoadingSO.brickdtb1].bricksizez SPC "Plates.");
		return 1;
	}
	messageboxok("File not found!\n\n"@%path);
	return 0;
}

function buildbot_pendingresults()
{
	if(!isobject(BuildBot_LoadingSO))
	{
		echo("Warning!", "A load is not in progress!");
		return 0;
	}

	if(!isobject(serverconnection))
	{
		echo("Lost Connection to the server! Terminating load...");
		buildbot_stoploadsequence();
		return 0;
	}

	if(iseventpending(BuildBot_LoadingSO.waittick))
		cancel(BuildBot_LoadingSO.waittick);

	if(!$buildbot::overdrive)
	{
		if(BuildBot_LoadingSO.brickerror || getsimtime() - BuildBot_LoadingSO.lastbricktime > 500 + $buildbot::delay)
		{
			BuildBot_LoadingSO.brickcount++;
			BuildBot_LoadingSO.floats++;
			BuildBot_LoadingSO.brickfloaterror = false;
			BuildBot_LoadingSO.brickdtb[BuildBot_LoadingSO.brickcount] = BuildBot_LoadingSO.brickdtb[BuildBot_LoadingSO.lastbrick];
			BuildBot_LoadingSO.brickpos[BuildBot_LoadingSO.brickcount] = BuildBot_LoadingSO.brickpos[BuildBot_LoadingSO.lastbrick];
			BuildBot_LoadingSO.brickrot[BuildBot_LoadingSO.brickcount] = BuildBot_LoadingSO.brickrot[BuildBot_LoadingSO.lastbrick];
			BuildBot_LoadingSO.brickcol[BuildBot_LoadingSO.brickcount] = BuildBot_LoadingSO.brickcol[BuildBot_LoadingSO.lastbrick];
			BuildBot_LoadingSO.consecutiveerrors++;

			if(BuildBot_LoadingSO.consecutiveerrors >= ((BuildBot_LoadingSO.brickcount-BuildBot_LoadingSO.floats)-BuildBot_LoadingSO.plants))
			{
				buildbot_stoploadsequence();
				return;
			}
			%next = true;
			BuildBot_LoadingSO.brickerror = false;
		}
		else
		{
			for(%a=BuildBot_LoadingSO.group.getcount()-1; %a>-1; %a--)
			{
				%id = BuildBot_LoadingSO.group.getobject(%a);

				if(%id <= BuildBot_LoadingSO.lastobj)
				{
					%next = false;
					break;
				}
				else if(%id > BuildBot_LoadingSO.lastobj && %id.getdatablock().uiname == BuildBot_LoadingSO.brickdtb[BuildBot_LoadingSO.lastbrick])
				{
					BuildBot_LoadingSO.lastobj = %id;
					BuildBot_LoadingSO.plants++;
					BuildBot_LoadingSO.consecutiveerrors = 0;
					%next = true;
					break;
				}
				%next = false;
			}
		}
	}
	else
		%next = true;

	if(%next)
		buildbot_generatenextbrick();
	else
		BuildBot_LoadingSO.waittick = schedule(5, false, eval, "buildbot_pendingresults();");
	return 1;
}

function buildbot_stoploadsequence()
{
	if(!isObject(BuildBot_LoadingSO))
	{
		echo("Warning!", "No Loading sequence is active!");
		return 0;
	}

	if(buildbotgui2.isawake())
		canvas.popdialog(buildbotgui2);
	buildbot_pause.settext("Resume");
	buildbot_info.settext("");

	if($buildbot::overdrive)
	{
		if($buildbot::prompt)
			messageboxok("Loading complete", "Loaded" SPC BuildBot_Loadingso.brickcount SPC "bricks in" SPC mfloor(((getsimtime()-buildbot_loadingso.starttime)/1000)*10)/10 SPC "seconds.\n\nYou can disable this prompt in the options.");
		else
			echo("Loaded" SPC BuildBot_Loadingso.brickcount SPC "bricks in" SPC mfloor(((getsimtime()-buildbot_loadingso.starttime)/1000)*10)/10 SPC "seconds.");
	}
	else
	{
		if($buildbot::prompt)
			messageboxok("Loading Complete!", "Loaded" SPC BuildBot_LoadingSO.plants SPC "out of" SPC BuildBot_LoadingSO.brickcount - BuildBot_LoadingSO.floats SPC "bricks.\n\n" @ BuildBot_LoadingSO.unknownplants SPC "bricks are reported MIA for some reason\n\nYou can disable this prompt in the options.");
		else
		{
			echo("Loaded" SPC BuildBot_LoadingSO.plants SPC "out of" SPC BuildBot_LoadingSO.brickcount - BuildBot_LoadingSO.floats SPC "bricks.");
			echo(BuildBot_LoadingSO.unknownplants SPC "bricks are reported MIA for some reason");
		}
	}

	if(iseventpending(BuildBot_LoadingSO.pendingresults))
		cancel(BuildBot_LoadingSO.pendingresults);
	BuildBot_LoadingSO.delete();
	return 1;
}

function buildbot_quickload(%x)
{
	if(%x)
	{
		if($buildbot::prompt)
		{
			if(getplayerdirection() == 4)
			{
				messageboxok("Warning!", "Please spawn before using BuildBot.");
				return 0;
			}
		}
		$buildbotquickload = 1;
		buildbot_activateloadsequence($buildbot::quickload);
		return 1;
	}
	return 0;
}