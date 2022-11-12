<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

class APICache {
	use Util\_Static;

	public static ICache $CacheImpl;
	
	public static function Get(string $path, string $schemaVersion = 'latest') : object 
	{
		if(!isset(APICache::$CacheImpl))
			APICache::$CacheImpl = new DefaultCacheImpl();
		return APICache::$CacheImpl->Get($path, $schemaVersion);
	}

	public static function ResolveWeaponType(int $itemId) : WeaponType
	{
		$itemData = APICache::Get("/items/$itemId");
		assert($itemData->type === "Weapon", "Item is not a weapon:\n".json_encode($itemData));

		return WeaponType::fromName($itemData->details->type);
	}

	/** 
	 * @return StatId StatId::_UNDEFINED if the item does not have stats.
	 */
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
}
