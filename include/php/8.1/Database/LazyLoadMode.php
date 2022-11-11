<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

enum LazyLoadMode : int {
	case NONE = 0;
	case OFFLINE_ONLY = 1;
	case FULL = 2;
}
