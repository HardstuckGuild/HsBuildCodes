<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

class LazyLoadMode {
	use Util\Enum;
	
	public const NONE = 0;
	public const OFFLINE_ONLY = 1;
	public const FULL = 2;
}
