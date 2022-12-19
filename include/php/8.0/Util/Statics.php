<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Util;

trait _Static {
	private function __construct() {}
	private function __clone() {}
}

trait First {
	public static function _FIRST() : int
	{ return 1; }
}

trait Enum {
	use _Static;

	public static function TryGetName(int $value) {
		foreach((new \ReflectionClass(static::class))->getConstants() as $name => $enumValue) {
			if($value === $enumValue)
				return $name;
		}
		return "__not_defined($value)";
	}

	public static function TryGetValue(string $name) : int
	{
		if(defined(static::class.'::'.$name))
			return constant(static::class.'::'.$name);
		else
			return 0;
	}

	public static function GetValue(string $name) : int
	{
		return constant(static::class.'::'.$name);
	}
}
