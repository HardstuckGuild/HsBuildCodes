<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

class Statics
{
	public const FIRST_VERSIONED_VERSION = 2;
	public const CURRENT_VERSION = 2;
	public const OFFICIAL_CHAT_CODE_BYTE_LENGTH = 44;

	public const ALL_EQUIPMENT_COUNT = 16;
	public const ALL_INFUSION_COUNT = 21;

	public static function DetermineCodeVersion(string $code) : int
	{
		$vcValue = ord($code[0]);
		if($vcValue > count(TextLoader::INVERSE_CHARSET)) return -1;

		if(str_starts_with($code, 'v0_')) return 1;

		$potentialVersion = TextLoader::INVERSE_CHARSET[$vcValue];
		// version may be lower or uppercase
		if($potentialVersion > 26) $potentialVersion -= 26;

		// NOTE(Rennorb): v1 codes start with the type indicator, which is never greater than 2. 
		// since this is also the first versioned version we can conclude tha values above the current version are invalid
		if($potentialVersion > Statics::CURRENT_VERSION) return -1;

		if($potentialVersion < Statics::FIRST_VERSIONED_VERSION) return 1;

		return $potentialVersion;
	}

	public static function IsTwoHanded(WeaponType $weaponType) : bool
	{
		switch($weaponType)
		{
			case WeaponType::Axe:
			case WeaponType::Dagger:
			case WeaponType::Mace:
			case WeaponType::Pistol:
			case WeaponType::Sword:
			case WeaponType::Scepter:
			case WeaponType::Focus:
			case WeaponType::Shield:
			case WeaponType::Torch:
			case WeaponType::Warhorn:
			case WeaponType::ShortBow:
				return false;

			case WeaponType::Greatsword:
			case WeaponType::Hammer:
			case WeaponType::Longbow:
			case WeaponType::Rifle:
			case WeaponType::Staff:
			case WeaponType::HarpoonGun:
			case WeaponType::Spear:
			case WeaponType::Trident:
				return true;

			default: 
				assert(false, "invalid weapon {$weaponType}");
				return false;
		}
	}

	public static function IsAquatic(WeaponType $weaponType) : bool
	{
		switch($weaponType)
		{
			case WeaponType::Axe:
			case WeaponType::Dagger:
			case WeaponType::Mace:
			case WeaponType::Pistol:
			case WeaponType::Sword:
			case WeaponType::Scepter:
			case WeaponType::Focus:
			case WeaponType::Shield:
			case WeaponType::Torch:
			case WeaponType::Warhorn:
			case WeaponType::ShortBow:
			case WeaponType::Greatsword:
			case WeaponType::Hammer:
			case WeaponType::Longbow:
			case WeaponType::Rifle:
			case WeaponType::Staff:
				return false;
			
			case WeaponType::HarpoonGun:
			case WeaponType::Spear:
			case WeaponType::Trident:
				return true;

			default:
				assert(false, "invalid weapon {$weaponType}");
				return false;
		}
	}

	public static function ResolveLegend(?Specialization $eliteSpec, ?string $str) : ?Legend
	{ 
		switch ($str) {
			case "Legend1": return Legend::GLINT;
			case "Legend2": return Legend::SHIRO;
			case "Legend3": return Legend::JALIS;
			case "Legend4": return Legend::MALLYX;
			case "Legend5": return Legend::KALLA;
			case "Legend6": return Legend::VENTARI;
			case null: if($eliteSpec?->SpecializationId === SpecializationId::Vindicator) return Legend::VINDICATOR;
			default: return null;
		};
	}

	public static function ResolveEffectiveWeapons(BuildCode $code, WeaponSetNumber $setNumber) : WeaponSet
	{
		if($setNumber === WeaponSetNumber::Set1)
		{
			$mainSet = $code->WeaponSet1;
			$offSet  = $code->WeaponSet2;
		}
		else
		{
			$mainSet = $code->WeaponSet2;
			$offSet  = $code->WeaponSet1;
		}

		$result = new WeaponSet();

		if($mainSet->MainHand !== WeaponType::_UNDEFINED)
		{
			$result->MainHand = $mainSet->MainHand;
			$result->Sigil1   = $mainSet->Sigil1;
			if(Statics::IsTwoHanded($mainSet->MainHand)) {
				$result->Sigil2 = $mainSet->Sigil2;
				return $result;
			}
		}
		else if($offSet->MainHand !== WeaponType::_UNDEFINED)
		{
			if(Statics::IsTwoHanded($offSet->MainHand))
			{
				if($mainSet->OffHand !== WeaponType::_UNDEFINED)
				{
					$result->OffHand = $mainSet->OffHand;
					$result->Sigil2  = $mainSet->Sigil2;
					return $result;
				}
				else
				{
					$result->MainHand = $offSet->MainHand;
					$result->Sigil1   = $offSet->Sigil1;
					$result->Sigil2   = $offSet->Sigil2;
					return $result;
				}
			}
			else
			{
				$result->MainHand = $offSet->MainHand;
				$result->Sigil1   = $offSet->Sigil1;
			}
		}

		if($mainSet->OffHand !== WeaponType::_UNDEFINED)
		{
			$result->OffHand = $mainSet->OffHand;
			$result->Sigil2  = $mainSet->Sigil2;
		}
		else if($offSet->OffHand !== WeaponType::_UNDEFINED)
		{
			$result->OffHand = $offSet->OffHand;
			$result->Sigil2  = $offSet->Sigil2;
		}

		return $result;
	}

	private function __construct() {}
	private function __clone() {}
}