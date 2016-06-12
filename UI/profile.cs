//gui profiles

new guicontrolprofile(GuiMetroScrollProfile)
{
	bitmap = "./MetroScroll.png";
	border = 0;
	opaque = 0;
};

return;

new guicontrolprofile(GuiMetroWindowProfile:guiwindowprofile)
{
	bitmap="./metrowindow.png";
	fonttype="Segoe WP Bold";
	fontsize="20";
	fillcolor = "0 0 0 0";
};

if(!isfile("base/client/ui/cache/Segoe WP bold_32.gft"))
	filecopy("./Segoe WP bold_32.gft", "base/client/ui/cache/Segoe WP bold_32.gft");

if(!isfile("base/client/ui/cache/Segoe WP_20.gft"))
	filecopy("./Segoe WP_20.gft", "base/client/ui/cache/Segoe WP_20.gft");
