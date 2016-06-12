//Event Loading
//well here goes...

function buildbot_loadevents(%path)
{
	if(!isfile(%path))
		return;

	if(!isobject(BuildBot_WrenchSO))
	{
		new scriptobject(BuildBot_WrenchSO)
		{
			loadcount = 0;
		};
	}
	BuildBot_WrenchSO.loadedfile[%path] = true;
	BuildBot_WrenchSO.loadpath[BuildBot_WrenchSO.loadcount] = %path;
	BuildBot_WrenchSO.brickcount[BuildBot_WrenchSO.loadcount] = -1;
	%file = new fileobject();
	%file.openforread(%path);

	for(%i=0; %i<3; %i++)
		%file.readline();
	for(%i=0; %i<64; %i++)
		BuildBot_Wrenchso.coloridtable[BuildBot_WrenchSO.loadcount, %i] = %file.readline();
	%file.readline();

	while(!%file.iseof())
	{
		%line = %file.readline();

		if(getsubstr(%line, 0, 2) !$= "" && (%pos=strpos(%line, "\"")) > -1)
		{
			%name = "";
			%newbrick = 1;
			%dtb = getsubstr(%line, 0, %pos);
		}
		else if(getfield(%line, 0) $= "+-EVENT")
		{
			if(%newbrick)
			{
				%newbrick = 0;
				BuildBot_WrenchSO.brickcount[BuildBot_WrenchSO.loadcount]++;
				BuildBot_WrenchSO.eventcount[BuildBot_WrenchSO.loadcount, BuildBot_WrenchSO.brickcount[BuildBot_WrenchSO.loadcount]] = 0;
				BuildBot_Wrenchso.brickname[BuildBot_WrenchSO.loadcount, BuildBot_WrenchSO.brickcount[BuildBot_WrenchSO.loadcount]] = %name;
				BuildBot_Wrenchso.brickdtb[BuildBot_WrenchSO.loadcount, BuildBot_WrenchSO.brickcount[BuildBot_WrenchSO.loadcount]] = %dtb;
			}
			BuildBot_WrenchSO.eventline[BuildBot_WrenchSO.loadcount, BuildBot_WrenchSO.brickcount[BuildBot_WrenchSO.loadcount], BuildBot_WrenchSO.eventcount[BuildBot_WrenchSO.loadcount, BuildBot_WrenchSO.brickcount[BuildBot_WrenchSO.loadcount]]] = getfields(%line, 2);
			BuildBot_WrenchSO.eventcount[BuildBot_WrenchSO.loadcount, BuildBot_WrenchSO.brickcount[BuildBot_WrenchSO.loadcount]]++;
		}
		else if(getfield(%line, 0) $= "+-NTOBJECTNAME")
		{
			%name = getfield(%line, 1);
			%name = getsubstr(%name, 1, strlen(%name)-1);
		}
	}
	BuildBot_WrenchSO.brickcount[BuildBot_WrenchSO.loadcount]++;
	BuildBot_WrenchSO.loadcount++;
	%file.close();
	%file.delete();
}

function buildbot_compileevent(%load, %brick)
{
	if(!isobject(BuildBot_WrenchSO))
		return;
	echo("compiling: "@ %load SPC %brick);

	for(%i=0; %i<BuildBot_WrenchSO.eventcount[%load, %brick]; %i++)
	{
		%line = buildbot_buildeventline(BuildBot_WrenchSO.eventline[%load, %brick, %i], %load);

		if(getfield(BuildBot_WrenchSO.eventline[%load, %brick, %i], 4) !$= "" && getfield(%line, 4) $= "")
		{
			echo("Flagged line "@ %i @": "@ %line);
			%flagged = 1;
		}

		BuildBot_WrenchSO.loadedeventline[%i] = %line;
	}
	BuildBot_WrenchSO.Loadedeventlines = %i;
	buildbot_addevents();

	if($buildbot::prompt)
	{
		if(%flagged)
			messageboxok("Attention!", "Some loaded events have named targets that do not exist on the server!\n\nResults may be unsatisfactory.");
		else
			messageboxok("Good News!", "Event with "@ %i @" lines Loaded successfully.");
	}
}

function buildbot_addevents()
{
	if(!WrenchEventsDlg.isawake())
	{
		%flag = 1;
		canvas.pushdialog(WrenchEventsDlg);
	}
	wrenchEventsDlg.clear();

	for(%i=0; %i<BuildBot_WrenchSO.loadedeventlines; %i++)
		clientcmdaddevent(BuildBot_WrenchSO.loadedeventline[%i]);
	WrenchLock_Events.setvalue(1);
	WrenchEvents_ClickLock();

	if(%flag)
		canvas.popdialog(WrenchEventsDlg);
}

function buildbot_buildeventline(%line, %load)
{
	%enabled = getfield(%line, 0);
	%input = buildbot_matchinputevent(getfield(%line, 1));
	%delay = getfield(%line, 2);
	%classdata = buildbot_matchclassevent(getfield(%line, 3), %input);
	%class = getword(%classdata, 0);
	%classid = getword(%classdata, 1);
	%target = buildbot_matchnameevent(getfield(%line, 4));
	%output = buildbot_matchoutputevent(%class, getfield(%line, 5));
	%params = buildbot_buildeventparams(%class, %output, getfields(%line, 6), %load);
	return %enabled TAB %input TAB %delay TAB %classid TAB %target TAB %output TAB %params;
}

function buildbot_matchinputevent(%name)
{
	if(!$GotInputEvents) //o hurr i wonder what this means
		return -1;

	for(%a=0; %a<$InputEvent_CountfxDTSbrick; %a++)
	{
		if($InputEvent_NamefxDTSbrick_[%a] $= %name)
			return %a;
	}
	echo("could not match input event for "@ %name);
	return -1;
}

function buildbot_matchoutputevent(%class, %event)
{
	for(%a=0; %a<$OutputEvent_Count[%class]; %a++)
	{
		if($OutputEvent_name[%class, %a] $= %event)
			return %a;
	}
	echo("could not match output event for "@ %event @" of class "@ %class);
	return -1;
}

function buildbot_matchclassevent(%class, %input) //mostly just checking for stuff and things that happen when rykuta lays a turd
{
	//for(%a=0; %a<getwordcount($OutputEvent_ClassList); %a++)
	//{
	//	if(getword($OutputEvent_ClassList, %a) $= %class)
	//		return %class;
	//}

	for(%a=0; %a<getfieldcount($inputevent_TargetListfxdtsbrick_[%input]); %a++)
	{
		if(getword(getfield($inputevent_TargetListfxdtsbrick_[%input], %a), 0) $= %class)
			return getword(getfield($inputevent_TargetListfxdtsbrick_[%input], %a), 1) SPC %a;
	}
	echo("could not match class event for "@ %class @" of input "@ %input);
	return "fxdtsbrick 0"; //default
}

function buildbot_matchnameevent(%name)
{
	if(%name $= "")
		return "";

	for(%a=0; %a<serverconnection.ntnamecount; %a++)
	{
		if(serverconnection.ntname[%a] $= %name)
			return %a;
	}
	echo("could not match name for "@ %name);
	return "";
}

function buildbot_buildeventparams(%class, %event, %fields, %load)
{
	%params = $outputevent_parameterlist[%class, %event];
	echo(" ===init matching===");
	echo("MATCH: "@ %class SPC %event);
	echo("IN: "@ %fields);
	echo("STORED: "@ %params);

	for(%a=0; %a<getfieldcount(%params); %a++)
	{
		%parameter = getword(getfield(%params, %a), 0);
		%parameter2 = getword(getfield(%params, %a), 1);
		%data = getfield(%fields, %a);

		if(%parameter $= "datablock")
			%data = buildbot_matchclassdatablock(%parameter2, %data);
		else if(%parameter $= "paintcolor")
			%data = findclosestcolor(BuildBot_Wrenchso.coloridtable[%load, %data]);
		echo("RESULT: "@%data);
		%fields = setfield(%fields, %a, %data);
	}
	echo("set field values: "@ %fields);
	return %fields;
}

function buildbot_matchclassdatablock(%class, %datablock)
{
	for(%a=0; %a<BuildBot_DatablockClassTable.classcount; %a++)
	{
		if(BuildBot_DatablockClassTable.classdata[%a] $= %class)
			break;
	}

	if(%a >= BuildBot_DatablockClassTable.classcount)
	{
		echo("class not found: "@ %class);
		return;
	}

	for(%b=0; %b<BuildBot_DatablockClassTable.datablockcount[%a]; %b++)
	{
		if(BuildBot_DatablockClassTable.datablockname[BuildBot_DatablockClassTable.classdatablock[%a, %b]] $= %datablock)
			return BuildBot_DatablockClassTable.classdatablock[%a, %b];
	}
	echo("could not match datablock id for "@ %datablock @" of class "@ %class);
	return "";
}

function buildbot_builddatablockclasstable()
{
	if(serverconnection.builtdatablockclasstable || !isobject(serverconnection))
		return;

	if(isobject(BuildBot_DatablockClassTable))
		BuildBot_DatablockClassTable.delete();

	new scriptobject(BuildBot_DatablockClassTable)
	{
		classcount = 0;
	};

	if(isfile("add-ons/client_buildbot/support/sounduidata.txt"))
	{
		%file = new fileobject();
		%file.openforread("add-ons/client_buildbot/support/sounduidata.txt");

		while(!%file.iseof())
		{
			%line = %file.readline();
			BuildBot_DatablockClassTable.sounduitable[getfield(%line, 0)] = getfield(%line, 1);
		}
		%file.close();
		%file.delete();
	}

	for(%a=3; %a<4099; %a++)
	{
		if(!isobject(%a))
			break;
		%class = %a.getclassname();

		if(%class $= "AudioProfile")
		{
			%class = "Sound";

			if(%a.uiname $= "")
			{
				%name = getsubstr(%a.filename, 0, strlen(%a.filename)-4);

				while((%pos=strpos(%name, "/")) > -1)
					%name = getsubstr(%name, %pos+1, strlen(%name)-(%pos+1));

				if(BuildBot_DatablockClassTable.sounduitable[%name] !$= "")
					%name = BuildBot_DatablockClassTable.sounduitable[%name];
			}
			else
				%name = %a.uiname;
		}
		else if(%a.uiname $= "") //im allergic to these ones
				continue;
		else
			%name = %a.uiname;
		BuildBot_DatablockClassTable.datablockname[%a] = %name;

		for(%b=0; %b<BuildBot_DatablockClassTable.classcount; %b++)
		{
			if(BuildBot_DatablockClassTable.classdata[%b] $= %class)
				break;
		}

		if(%b >= BuildBot_DatablockClassTable.classcount)
		{
			BuildBot_DatablockClassTable.classdata[%b] = %class;
			BuildBot_DatablockClassTable.datablockcount[%b] = 0;
			BuildBot_DatablockClassTable.classcount++;
		}

		BuildBot_DatablockClassTable.classdatablock[%b, BuildBot_DatablockClassTable.datablockcount[%b]] = %a;
		BuildBot_DatablockClassTable.datablockcount[%b]++;
	}
	serverconnection.builtdatablockclasstable = 1;
}
