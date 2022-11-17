<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

class APICache {
	use Util\_Static;

	public static ICache $CacheImpl;
	
	public static function Get(string $path, string $schemaVersion = 'latest') : mixed 
	{
		if(!isset(APICache::$CacheImpl))
			APICache::$CacheImpl = new DefaultCacheImpl();
		return APICache::$CacheImpl->Get($path, $schemaVersion);
	}

	public static function ResolveWeaponType(int $itemId) : int
	{
		$itemData = APICache::Get("/items/$itemId");
		assert($itemData->type === "Weapon", "Item is not a weapon:\n".json_encode($itemData));

		return WeaponType::GetValue($itemData->details->type);
	}

	/** 
	 * @return int StatId::_UNDEFINED if the item does not have stats.
	 */
	public static function ResolveStatId(int $itemId) : int
	{
		$itemData = APICache::Get("/items/$itemId");
		return $itemData->details->infix_upgrade?->id ?? StatId::_UNDEFINED;
	}

	public static function ResolvePosition(?int $traitId) : int
	{
		if(!$traitId) return TraitLineChoice::NONE;

		$traitData = APICache::Get("/traits/$traitId");
		return $traitData->order + 1;
	}

	public static function ResolveWeaponSkill(BuildCode $code, WeaponSet $effectiveWeapons, int $skillIndex) : int
	{
		$weapon = null;
		if($skillIndex < 3)
		{
			if($effectiveWeapons->MainHand === WeaponType::_UNDEFINED) return SkillId::_UNDEFINED;

			//NOTE(Rennorb): this isnt outside of the if to allow early bail if the guard condition isnt met.
			$professionData = APICache::Get("/professions/".Profession::TryGetName($code->Profession));
			
			$weapon = $professionData->weapons->{WeaponType::TryGetName($effectiveWeapons->MainHand)};
		}
		else
		{
			if($effectiveWeapons->OffHand === WeaponType::_UNDEFINED
				&& ($effectiveWeapons->MainHand === WeaponType::_UNDEFINED || !Statics::IsTwoHanded($effectiveWeapons->MainHand)))
				return SkillId::_UNDEFINED;

			//NOTE(Rennorb): this isnt outside of the if to allow early bail if the guard condition isnt met.
			$professionData = APICache::Get("/professions/".Profession::TryGetName($code->Profession));
			if($effectiveWeapons->OffHand !== WeaponType::_UNDEFINED)
				$weapon = $professionData->weapons->{WeaponType::TryGetName($effectiveWeapons->OffHand)};
			else
				$weapon = $professionData->weapons->{WeaponType::TryGetName($effectiveWeapons->MainHand)};

		}

		foreach($weapon->skills as $skill)
		{
			if($skill->slot === "Weapon_" . ($skillIndex + 1))
				return $skill->id;
		}
		
		return SkillId::_UNDEFINED;
	}

	/** @return TraitId::_UNDEFINED If spec is empty or trait choice is empty */
	public static function ResolveTrait(Specialization $spec, int $traitSlot) : int
	{
		if($spec->SpecializationId === SpecializationId::_UNDEFINED) return TraitId::_UNDEFINED;
		$traitPos = $spec->Choices[$traitSlot];
		if($traitPos === TraitLineChoice::NONE) return TraitId::_UNDEFINED;

		$allSpecializationData = APICache::Get('/specializations?ids=all');

		foreach($allSpecializationData as $specialization)
		{
			if($specialization->id !== $spec->SpecializationId) continue;

			return $specialization->major_traits[$traitSlot * 3 + $traitPos - 1];
		}

		return TraitId::_UNDEFINED;
	}
}
