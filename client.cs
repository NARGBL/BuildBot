//============
//  BuildBot
//------------
// Created By
// Nexus 4833
//------------
// Please Use
// Responsibly
//============
																																																			//if(getnumkeyid()!=4833)return;
$BuildBot_Version = "3.2 FULL";

//We don't like duplicate guis now do we?
if(isobject(buildbotgui))
	buildbotgui.delete();

if(isobject(buildbotgui2))
	buildbotgui2.delete();

if(isobject(buildbotgui3))
	buildbotgui3.delete();

if(isobject(buildbotgui4))
	buildbotgui4.delete();

if(isobject(buildbotgui5))
	buildbotgui4.delete();

if(isobject(buildbotgui6))
	buildbotgui4.delete();

if(isobject(buildbotgui7))
	buildbotgui4.delete();

if(ispackage(buildbot))
	deactivatepackage(buildbot);

//support
exec("./Support/Package.cs");
exec("./Support/Support.cs");
exec("./Support/VersionCheck.cs");

//Bots
exec("./Bots/BuildBot.cs");
exec("./Bots/EventBot.cs");
exec("./Bots/DuplicatorBot.cs");

//interface
exec("./UI/Profile.cs");
exec("./Interface/MainInterface.cs");
exec("./Interface/EventInterface.cs");
exec("./Interface/Guis/BuildBotGUI.gui");
exec("./Interface/Guis/BuildBotGUI2.gui");
exec("./Interface/Guis/BuildBotGUI3.gui");
exec("./Interface/Guis/BuildBotGUI4.gui");
exec("./Interface/Guis/BuildBotGUI5.gui");
exec("./Interface/Guis/BuildBotGUI6.gui");
exec("./Interface/Guis/BuildBotGUI7.gui");

//non-function code
if(!$buildbotbinds) //in case they try to exec this again
{
	$remapdivision[$remapcount] = "BuildBot";
	$remapname[$remapcount] = "OpenGUI";
	$remapcmd[$remapcount] = "togbuildbotgui";
	$remapcount++;

	$remapname[$remapcount] = "QuickLoad";
	$remapcmd[$remapcount] = "buildbot_quickload";
	$remapcount++;

	$remapname[$remapcount] = "Duplicate Selection";
	$remapcmd[$remapcount] = "buildbot_generatedup";
	$remapcount++;
	$buildbotbinds = true;
}
activatepackage(buildbot);
buildbot_defaults();

if(isfile("config/client/buildbot/options.cs"))
{
	exec("config/client/buildbot/options.cs");
	buildbot_updateoptions();
}
buildbot_refresh($buildbotpath=$buildbot::home);
