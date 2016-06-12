//duplicating
//method of finding bricks based on original tool_duplicator

function buildbot_generatedup(%x)
{
	if(!%x)
		return 0;

	if(!isobject(serverconnection))

	{

		echo("Warning! You are not connected to a server.");

		return 0;

	}


	if(!isobject(%pl = serverconnection.getcontrolobject()))
	{
		echo("Warning! You are not spawned.");
		return 0;
	}
	%group = %pl.getgroup(); //I'm sure there is a better way, but meh.

	%ppos = %pl.getposition();

	for(%a=0; %a<%group.getcount(); %a++)

	{

		if((%b=%group.getobject(%a)).getclassname() $= "fxDTSBrick")

		{

			if(%b.isplanted() $= "0")

			{

				%gdist = vectordist(%b.getposition(), %ppos);

				if(%gdist < %bestgdist || %bestgdist $= "")
				{
					%bestgdist = %gdist;
					%ghost = %b;
				}
			}

		}

	}


	if(%ghost $= "")
	{
		echo("No ghost bricks were found!");
		return 0;
	}
	%pos = %ghost.getposition();

	for(%a=0; %a<%group.getcount();  %a++)

	{

		if((%b=%group.getobject(%a)).getclassname() $= "fxDTSBrick" && %b.isplanted())

		{

			if(!$buildbot::dupfilter || %b.getdatablock() == %ghost.getdatablock())
			{
				%bpos = %b.getposition();

				%dist = vectorDist(%pos, %bpos);

				if(%dist < %bestdist || %bestdist $= "")
				{

					%bestdist = %dist;

					%bestid = %b;

				}
			}

		}

	}


	if(%bestid $= "")
	{
		echo("No bricks were found.");
		return 0;
	}

	if(isobject(buildbot_SavingSO))
		buildbot_SavingSO.delete();

	new scriptobject(buildbot_SavingSO)
	{
		brickcount = 1;
		brick0 = %bestid;
		brickid[%bestid] = true;
	};

	if(!buildbot_isint($buildbot::maxdup))
		buildbot_maxdup.settext($buildbot::maxdup = 2000);

	for(%a=0; %a<buildbot_SavingSO.brickcount; %a++)
	{
		if(%a >= $buildbot::maxdup)
		{
			buildbot_SavingSO.brickcount = %a;
			break;
		}
		%homebrick = buildbot_SavingSO.brick[%a];

		if(%a) //don't want to get bricks below first one.
		{
			for(%b=0; %b<%homebrick.getnumdownbricks(); %b++)
			{
				%testbrick = %homebrick.getdownbrick(%b);

				if(buildbot_SavingSO.brickid[%testbrick] $= "")
				{
					buildbot_SavingSO.brick[buildbot_SavingSO.brickcount] = %testbrick;
					buildbot_SavingSO.brickid[%testbrick] = true;
					buildbot_SavingSO.brickcount++;
				}
			}
		}

		for(%b=0; %b<%homebrick.getnumupbricks(); %b++)
		{
			%testbrick = %homebrick.getupbrick(%b);

			if(buildbot_SavingSO.brickid[%testbrick] $= "")
			{
				buildbot_SavingSO.brick[buildbot_SavingSO.brickcount] = %testbrick;
				buildbot_SavingSO.brickid[%testbrick] = true;
				buildbot_SavingSO.brickcount++;
			}
		}
	}

	if($buildbot::automove)
	{
		%shifta = buildbot_getshiftpos(%pos, %ghost.getdatablock());
		%shiftb = buildbot_getshiftpos(%bestid.getposition(), %bestid.getdatablock());
		%shiftx = getword(%shiftb, 0) - getword(%shifta, 0);
		%shifty = getword(%shiftb, 1) - getword(%shifta, 1);
		%shiftz = getword(%shiftb, 2) - getword(%shifta, 2);

		if(%dir = getplayerdirection())
		{
			if(%dir == 1)
			{
				%a = %shifty;
				%shifty = %shiftx * -1;
				%shiftx = %a;
			}
			else if(%dir == 2)
			{
				%shiftx = %shiftx * -1;
				%shifty = %shifty * -1;
			}
			else if(%dir == 3)
			{
				%a = %shiftx;
				%shiftx = %shifty * -1;
				%shifty = %a;
			}
		}

		if($buildbot::altload)
		{
			commandtoserver('buybrick', 0, %bestid.getdatablock());
			commandtoserver('useinventory', 0);
			commandtoserver('shiftbrick', %shiftx, 0, 0);
			commandtoserver('shiftbrick', 0, %shifty, 0);
			commandtoserver('shiftbrick', 0, 0, %shiftz);
		}
		else
		{
			commandtoserver('instantusebrick', %bestid.getdatablock());
			commandtoserver('shiftbrick', %shiftx, %shifty, %shiftz);
		}
	}
	buildbot_dupprompt();
}

function buildbot_dupprompt()
{
	if(!isobject(BuildBot_SavingSO))
		return;

	canvas.pushdialog(BuildBotgui7);
	buildbot_dupinput.settext($buildbot::duppath);
}

function buildbot_saveduptofile(%path)
{
	%file = new fileobject(); //make this easy for everyone, and just file it
	%x = "";

	if(%path $= "")
		%path = $buildbot::duppath;

	while(isfile(%path @ %x @ ".bls"))
		%x++;
	%path = %path @ %x @ ".bls";
	%file.openforwrite(%path);
	%file.writeline("This is a Blockland save file.  You probably shouldn't modify it cause you'll screw it up.");
	%file.writeline("1");
	%file.writeLine("Build Bot Duplication");

	for(%a=0; %a<64; %a++)
		%file.writeline(getcoloridtable(%a));
	%file.writeline("LineCount" SPC buildbot_SavingSO.brickcount);

	for(%a=0; %a<buildbot_SavingSO.brickcount; %a++)
	{
		%b = buildbot_SavingSO.brick[%a];
		%file.writeline(%b.getdatablock().uiname @ "\"" SPC %b.getposition() SPC %b.getangleid() SPC %b.isbaseplate() SPC %b.getcolorid() SPC ((%b.getdatablock().subcategory $= "Prints") ? getprinttexture(%b.getprintid()):"") SPC %b.getcolorfxid() SPC %b.getshapefxid() SPC %b.israycasting() SPC %b.iscolliding() SPC %b.isrendering());
	}
	%file.close();
	%file.delete();

	if(!isfile(%path))
	{
		if($buildbot::prompt)
			messageboxok("Warning!", "Failed to save to file:\n\n"@ %path @"\n\nConsider changing the path in the options GUI.");
		echo("Failed to save to file:\n\n"@ %path);
		return 0;
	}
	else
	{
		if($buildbot::prompt)
		{
			if($buildbot::autopause)
				messageboxyesno("Attention!", buildbot_SavingSO.brickcount SPC "Bricks have been successfully duplicated and saved to file.\n\n"@%path @"\n\nWould you like to load this file now?", "$buildbot::autopause=1;$buildbotautoresume=1;buildbot_activateloadsequence(\""@%path@"\");", "");
			else
				messageboxok("Attention!", buildbot_SavingSO.brickcount SPC "Bricks have been successfully duplicated and saved to file.\n\n"@%path);
		}
		else
		{
			$buildbot::autopause = 1;
			$buildbotautoresume = 1;
			buildbot_activateloadsequence($buildbot::duppath @ %x @ ".bls");
		}
		echo(buildbot_SavingSO.brickcount SPC "Bricks have been successfully duplicated and saved to file.");
		echo(%path);
	}
	buildbot_SavingSO.delete();
	return 1;
}

function buildbot_clickdupsave()
{
	%return = buildbot_saveduptofile(buildbot_dupinput.getvalue());

	if(%return)
		canvas.popdialog(buildbotgui7);
}
