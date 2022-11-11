<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Tests;

class TestUtilities {
	/** @var array<string, string> */
	public static array $CodesInvalid  = [];
	/** @var array<string, string> */
	public static array $CodesV1       = [];
	/** @var array<string, string> */
	public static array $CodesV2       = [];
	/** @var array<string, string> */
	public static array $CodesIngame   = [];
	/** @var array<string, string> */
	public static array $CodesV2Binary = [];

	static function __construct_static() {
		$dict = null;
		$currentDict = null;
		$currentKey = null;
		$currentAccumulator = '';
		foreach(file(__DIR__."/../../common/codes.ini") as $line_)
		{
			$comment = strpos($line_, ';');
			$line = trim($comment !== false ? substr($line_, 0, $comment) : $line_);
			if(empty($line)) continue;

			if(str_starts_with($line, '[') && str_ends_with($line, ']'))
			{
				$currentDict = substr($line, 1, strlen($line) - 2);
				switch ($currentDict) {
					case "Invalid" : $dict = &TestUtilities::$CodesInvalid; break;
					case "V1"      : $dict = &TestUtilities::$CodesV1; break;
					case "V2"      : $dict = &TestUtilities::$CodesV2; break;
					case "Ingame"  : $dict = &TestUtilities::$CodesIngame; break;
					case "V2Binary": $dict = &TestUtilities::$CodesV2Binary; break;
				};
			}
			else
			{
				if($currentDict !== 'V2Binary')
				{
					list($key, $value) = explode('=', $line, 2);
					$dict[trim($key)] = trim($value);
				}
				else if($line === "<end>")
				{
					$dict[$currentKey] = $currentAccumulator;
					$currentAccumulator = '';
				}
				else
				{
					$split = strpos($line, '=');
					if($split !== false)
					{
						$currentKey = trim(substr($line, 0, $split));
					}
					else
					{
						$currentAccumulator .= $line;
					}
				}
			}
		}
	}
}

TestUtilities::__construct_static();