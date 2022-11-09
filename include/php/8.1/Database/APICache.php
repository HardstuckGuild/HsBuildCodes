<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

class CacheEntry {
	public \DateTimeImmutable $CacheTime;
	public object             $Response;

	public function __construct(\DateTimeImmutable $cacheTime, object $response) {
		$this->CacheTime = $cacheTime;
		$this->Response  = $response;
	}
}

class APICache {
	public const CACHE_SECONDS = 30 * 60;

	/** @var CacheEntry[] */
	private static array $_cache = [];
	public static function Get(string $path, string $schemaVersion = 'latest') : object
	{
		$key = $path.$schemaVersion;

		$ret = null;
		$now = new \DateTimeImmutable();
		if(array_key_exists($key, APICache::$_cache))
		{
			$entry = APICache::$_cache[$key];
			if(date_diff($entry->CacheTime, $now)->s > APICache::CACHE_SECONDS)
			{
				$ret = API::RequestJson($path, schemaVersion: $schemaVersion);
				$entry->CacheTime = $now;
				$entry->Response = $ret;
			}
			else
			{
				$ret = $entry->Response;
			}
		}
		else
		{
			$ret = API::RequestJson($path, schemaVersion: $schemaVersion);
			APICache::$_cache[$key] = new CacheEntry($now, $ret);
		}
		return $ret;
	}

	public static function ResolveWeaponType(int $itemId) : WeaponType
	{
		$itemData = APICache::Get("/items/$itemId");
		assert($itemData->type === "Weapon", "Item is not a weapon:\n".json_encode($itemData));

		return WeaponType::fromName($itemData->details->type);
	}

	/// <returns><see cref="StatId::_UNDEFINED"/> if the item does not have stats</returns>
	/// <exception cref="InvalidOperationException">sad</exception>
	public static function ResolveStatId(int $itemId) : StatId
	{
		$itemData = APICache::Get("/items/$itemId");
		return $itemData->details->infix_upgrade?->id ?? StatId::_UNDEFINED;
	}

	public static function ResolvePosition(?int $traitId) : TraitLineChoice
	{
		if(!$traitId) return TraitLineChoice::NONE;

		$traitData = APICache::Get("/traits/$traitId");
		return TraitLineChoice::from($traitData->order + 1);
	}

	public static function ResolveSkillInfo(int $skillId) : object
	{
		// place to do overrides
		return APICache::Get("/skills/{$skillId}");
	}

	public static function ResolveWeaponSkill(BuildCode $code, WeaponSet $effectiveWeapons, int $skillIndex) : int
	{
		$weapon = null;
		if($skillIndex < 3)
		{
			if($effectiveWeapons->MainHand === WeaponType::_UNDEFINED) return SkillId::_UNDEFINED;

			//NOTE(Rennorb): this isnt outside of the if to allow early bail if the guard condition isnt met.
			$professionData = APICache::Get("/professions/{$code->Profession->name}");
			
			$weapon = $professionData->weapons->{$effectiveWeapons->MainHand->name};
		}
		else
		{
			if($effectiveWeapons->OffHand === WeaponType::_UNDEFINED && !Statics::IsTwoHanded($effectiveWeapons->MainHand))
				return SkillId::_UNDEFINED;

			//NOTE(Rennorb): this isnt outside of the if to allow early bail if the guard condition isnt met.
			$professionData = APICache::Get("/professions/{$code->Profession->name}");
			if($effectiveWeapons->OffHand !== WeaponType::_UNDEFINED)
				$weapon = $professionData->weapons->{$effectiveWeapons->OffHand->name};
			else
				$weapon = $professionData->weapons->{$effectiveWeapons->MainHand->name};

		}

		foreach($weapon->skills as $skill)
		{
			if($skill->slot === "Weapon_" . ($skillIndex + 1))
				return $skill->id;
		}
		
		return SkillId::_UNDEFINED;
	}

	private function __construct() {}
	private function __clone() {}
}
