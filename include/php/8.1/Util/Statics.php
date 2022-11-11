<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Util;

trait _Static {
	private function __construct() {}
	private function __clone() {}
}

trait FromName {
	public static function fromName(string $name) : static
	{
		$reflect = new \ReflectionEnum(static::class);
		return $reflect->getCase($name)->getValue();
	}
}

trait First {
	public static function _FIRST() : int
	{ return 1; }
}

trait Enum {
	use _Static;

	public static function TryGetName(int $value) {
		foreach(get_class_vars(static::class) as $name => $value) {
			if($value === $value)
				return $name;
		}
		return '__not_defined';
	}

	public static function TryGetValue(string $name) : int
	{
		if(property_exists(static::class, $name))
			return static::class::$$name;
		else
			return 0;
	}

	public static function GetValue(string $name) : int
	{
			return static::class::$$name;
	}
}
