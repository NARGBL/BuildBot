package buildbot
{
	function clientcmdservermessage(%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l)
	{
		if(isObject(BuildBot_LoadingSO))
		{
			if(strpos(%a,"MsgPlantError") > -1)
				BuildBot_LoadingSO.brickerror = true;
		}
		return parent::clientcmdservermessage(%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l);
	}

	function plantbrick(%x)
	{
		if(isobject(buildbot_LoadingSO) && %x)
		{
			if(buildbot_LoadingSO.paused)
			{
				if($buildbot::plantstart && $buildbotautoresume)
				{
					$buildbotautoresume = 0;
					buildbot_togpause();
					return 1;
				}
			}
			else if($buildbot::lock)
				return 0;
		}
		return parent::plantbrick(%x);
	}

	function buildbotgui5::onwake(%this, %idk, %whocares, %notme)
	{
		buildbot_welrefresh($buildbot_welload);
	}

	function yaw(%x)
	{
		if(isobject(BuildBot_LoadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
				return 0;
		return parent::yaw(%x);
	}

	function mousefire(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::mousefire(%x);
	}

	function shiftbrickaway(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::shiftbrickaway(%x);
	}

	function shiftbricktowards(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::shiftbricktowards(%x);
	}

	function shiftbrickleft(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::shiftbrickleft(%x);
	}

	function shiftbrickright(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::shiftbrickright(%x);
	}

	function shiftbrickup(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::shiftbrickup(%x);
	}

	function shiftbrickdown(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::shiftbrickdown(%x);
	}

	function shiftbrickthirdup(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::shiftbrickthirdup(%x);
	}

	function shiftbrickthirddown(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::shiftbrickthirddown(%x);
	}

	function supershiftbrickawayproxy(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::supershiftbrickawayproxy(%x);
	}

	function supershiftbricktowardsproxy(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::supershiftbricktowardsproxy(%x);
	}

	function supershiftbrickleftproxy(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::supershiftbrickleftproxy(%x);
	}

	function supershiftbrickrightproxy(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::supershiftbrickrightproxy(%x);
	}

	function supershiftbrickupproxy(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::supershiftbrickupproxy(%x);
	}

	function supershiftbrickdownproxy(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::supershiftbrickdownproxy(%x);
	}

	function playbackbuildmacro(%x)
	{
		if(isobject(buildbot_loadingSO) && !BuildBot_Loadingso.paused && $buildbot::lock)
			return 0;
		parent::playbackbuildmacro(%x);
	}
};