function getghostdirection() //unused, since cuboid bricks do not have all 4 directions
{
	if(!isobject(serverconnection))
	{
		echo("Warning!", "You are not connected to a server.");
		return -1;
	}
	%client = serverconnection.getcontrolobject().getcontrollingclient(); //I'm sure there is a better way, but meh.

	for(%a=%client.getcount(); %a>0; %a--)
	{
		if((%b=%client.getobject(%a)).getclassname() $= "fxDTSBrick")
		{
			if(%b.isplanted() $= "0")
				return %b.getangleid();
		}
	}
	return -1;
}

function getplayerdirection()
{
	if(isObject(%p = serverconnection.getcontrolobject()))
	{
		%va = getword(%p.getforwardvector(),0);
		%vb = getword(%p.getforwardvector(),1);

		if(mabs(%va) > mabs(%vb))
		{
			if(%va > 0)
				return 0;
			else
				return 2;
		}
		else
		{
			if(%vb > 0)
				return 1;
			else
				return 3;
		}
	}
	else
		return 4;
}

function findclosestcolor(%x)
{
	for(%a=0; %a<64; %a++)
	{
		%match = mabs(getword(getcoloridtable(%a),0) - getword(%x,0)) + mabs(getword(getcoloridtable(%a),1) - getword(%x,1)) + mabs(getword(getcoloridtable(%a),2) - getword(%x,2)) + mabs(getword(getcoloridtable(%a),3) - getword(%x,3))*4;

		if(%match < %bestmatch || %bestmatch $= "")
		{
			%bestmatch = %match;
			%bestid = %a;
		}
	}
	return %bestid;
}

function buildbot_getshiftpos(%pos, %dtb)
{
	if(!buildbot_isint(%sx = %dtb.bricksizex) || !buildbot_isint(%sz = %dtb.bricksizez))
	{
		if($buildbot::prompt)
			echo("Bad Datablock sent:"@ %dtb);
		return 0;
	}
	%x = getword(%pos, 0)*2;
	%y = getword(%pos, 1)*2;
	%mz = getword(%pos, 2)*5;
	%dir = getplayerdirection();

	if(!buildbot_isint(%x) && buildbot_isint(%y) && %sx !$= "1")
	{
		if(%dir $= "1")
			%y -= 1;
		else if(%dir $= "2" || %dir $= "3")
		{
			%x += 1;
			%y -= 1;
		}
	}
	%z = mceil(%mz);

	if(buildbot_isint(%mz) || (!buildbot_isint(%mz) && buildbot_isint(%sz/2))) //accounting for duplicator error
		%z++;
	return mfloor(%x) SPC mfloor(%y) SPC %z;
}

function buildbot_isint(%x)
{
	if(%x $= mfloor(%x)) //also makes it sure it is a string
		return 1;
	return 0;
}

function buildbot_getlastfilepart(%path) //I thought this one was pretty clever, If I do say so myself
{
	strreplace(%path, "/", "\t");
	%str = getfield(%path, getfieldcount(%path)-1);
	return %str;
}
