<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

class Statics {
	use Util\_Static;

	public const FIRST_VERSIONED_VERSION = 3;
	public const CURRENT_VERSION = 3;
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
		if($potentialVersion >= 26) $potentialVersion -= 26;

		// NOTE(Rennorb): v1 codes start with the type indicator, which is never greater than 2. 
		// since this is also the first versioned version we can conclude tha values above the current version are invalid
		if($potentialVersion > Statics::CURRENT_VERSION) return -1;

		if($potentialVersion < Statics::FIRST_VERSIONED_VERSION) return 1;

		return $potentialVersion;
	}

	public static function IsTwoHanded(int $weaponType) : bool
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

	public static function IsAquatic(int $weaponType) : bool
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

	public static function ResolveLegend(Specialization $eliteSpec, ?string $str) : ?Legend
	{ 
		switch ($str) {
			case "Legend1": return Legend::GLINT;
			case "Legend2": return Legend::SHIRO;
			case "Legend3": return Legend::JALIS;
			case "Legend4": return Legend::MALLYX;
			case "Legend5": return Legend::KALLA;
			case "Legend6": return Legend::VENTARI;
			case null: if($eliteSpec->SpecializationId === SpecializationId::Vindicator) return Legend::VINDICATOR;
			default: return null;
		};
	}

	public static function ResolveEffectiveWeapons(BuildCode $code, int $setNumber) : WeaponSet
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

	/** @remarks Also translates _UNDEFINED */
	public static function ResolveDummyItemForWeaponType(int $weaponType, int $statId) : int
	{
		return match ($statId) {
			StatId::Dragons1, StatId::Dragons2, StatId::Dragons3, StatId::Dragons4 => match($weaponType) {
				WeaponType::Axe        => ItemId::Suuns_Reaver      ,
				WeaponType::Dagger     => ItemId::Suuns_Razor       ,
				WeaponType::Mace       => ItemId::Suuns_Flanged_Mace,
				WeaponType::Pistol     => ItemId::Suuns_Revolver    ,
				WeaponType::Scepter    => ItemId::Suuns_Wand        ,
				WeaponType::Sword      => ItemId::Suuns_Blade       ,
				WeaponType::Focus      => ItemId::Suuns_Artifact    ,
				WeaponType::Shield     => ItemId::Suuns_Bastion     ,
				WeaponType::Torch      => ItemId::Suuns_Brazier     ,
				WeaponType::Warhorn    => ItemId::Suuns_Herald      ,
				WeaponType::Greatsword => ItemId::Suuns_Claymore    ,
				WeaponType::Hammer     => ItemId::Suuns_Warhammer   ,
				WeaponType::Longbow    => ItemId::Suuns_Greatbow    ,
				WeaponType::Rifle      => ItemId::Suuns_Musket      ,
				WeaponType::ShortBow   => ItemId::Suuns_Short_Bow   ,
				WeaponType::Staff      => ItemId::Suuns_Spire       ,
				WeaponType::HarpoonGun => ItemId::Suuns_Harpoon_Gun ,
				WeaponType::Spear      => ItemId::Suuns_Impaler     ,
				WeaponType::Trident    => ItemId::Suuns_Trident     ,
				default => ItemId::_UNDEFINED,
			},
			StatId::Ritualists1, StatId::Ritualists2, StatId::Ritualists3, StatId::Ritualists4 => match($weaponType) {
				WeaponType::Axe        => ItemId::Togos_Reaver      ,
				WeaponType::Dagger     => ItemId::Togos_Razor       ,
				WeaponType::Mace       => ItemId::Togos_Flanged_Mace,
				WeaponType::Pistol     => ItemId::Togos_Revolver    ,
				WeaponType::Scepter    => ItemId::Togos_Wand        ,
				WeaponType::Sword      => ItemId::Togos_Blade       ,
				WeaponType::Focus      => ItemId::Togos_Artifact    ,
				WeaponType::Shield     => ItemId::Togos_Bastion     ,
				WeaponType::Torch      => ItemId::Togos_Brazier     ,
				WeaponType::Warhorn    => ItemId::Togos_Herald      ,
				WeaponType::Greatsword => ItemId::Togos_Claymore    ,
				WeaponType::Hammer     => ItemId::Togos_Warhammer   ,
				WeaponType::Longbow    => ItemId::Togos_Greatbow    ,
				WeaponType::Rifle      => ItemId::Togos_Musket      ,
				WeaponType::ShortBow   => ItemId::Togos_Short_Bow   ,
				WeaponType::Staff      => ItemId::Togos_Spire       ,
				WeaponType::HarpoonGun => ItemId::Togos_Harpoon_Gun ,
				WeaponType::Spear      => ItemId::Togos_Impaler     ,
				WeaponType::Trident    => ItemId::Togos_Trident     ,
				default => ItemId::_UNDEFINED,
			},
			default => match($weaponType) {
				WeaponType::Axe        => ItemId::Mist_Lords_Axe          ,
				WeaponType::Dagger     => ItemId::Mist_Lords_Dagger       ,
				WeaponType::Mace       => ItemId::Mist_Lords_Mace         ,
				WeaponType::Pistol     => ItemId::Mist_Lords_Pistol       ,
				WeaponType::Scepter    => ItemId::Mist_Lords_Scepter      ,
				WeaponType::Sword      => ItemId::Mist_Lords_Sword        ,
				WeaponType::Focus      => ItemId::Mist_Lords_Focus        ,
				WeaponType::Shield     => ItemId::Mist_Lords_Shield       ,
				WeaponType::Torch      => ItemId::Mist_Lords_Torch        ,
				WeaponType::Warhorn    => ItemId::Mist_Lords_Warhorn      ,
				WeaponType::Greatsword => ItemId::Mist_Lords_Greatsword   ,
				WeaponType::Hammer     => ItemId::Mist_Lords_Hammer       ,
				WeaponType::Longbow    => ItemId::Mist_Lords_Longbow      ,
				WeaponType::Rifle      => ItemId::Mist_Lords_Rifle        ,
				WeaponType::ShortBow   => ItemId::Mist_Lords_Short_Bow    ,
				WeaponType::Staff      => ItemId::Mist_Lords_Staff        ,
				WeaponType::HarpoonGun => ItemId::Harpoon_Gun_of_the_Scion,
				WeaponType::Spear      => ItemId::Impaler_of_the_Scion    ,
				WeaponType::Trident    => ItemId::Trident_of_the_Scion    ,
				default => ItemId::_UNDEFINED,
			}
		};
	}

	/** @remarks Does not translate weapon items. Use Static::ResolveDummyItemForWeaponType(WeaponType, StatId) for that. */
	public static function ResolveDummyItemForEquipment(int $equipmentIndex, int $weightClass, int $statId) : int
	{
		return match ($statId) {
			StatId::Dragons1, StatId::Dragons2, StatId::Dragons3, StatId::Dragons4 => match($equipmentIndex) {
				0 => match ($weightClass) {
					WeightClass::Light  => ItemId::Suuns_Masque,
					WeightClass::Medium => ItemId::Suuns_Visage,
					WeightClass::Heavy  => ItemId::Suuns_Visor,
					default => ItemId::_UNDEFINED,
				},
				1 => match ($weightClass) {
					WeightClass::Light  => ItemId::Suuns_Epaulets,
					WeightClass::Medium => ItemId::Suuns_Shoulderguard,
					WeightClass::Heavy  => ItemId::Suuns_Pauldrons,
					default => ItemId::_UNDEFINED,
				},
				2 => match ($weightClass) {
					WeightClass::Light  => ItemId::Suuns_Doublet,
					WeightClass::Medium => ItemId::Suuns_Guise,
					WeightClass::Heavy  => ItemId::Suuns_Breastplate,
					default => ItemId::_UNDEFINED,
				},
				3 => match ($weightClass) {
					WeightClass::Light  => ItemId::Suuns_Wristguards,
					WeightClass::Medium => ItemId::Suuns_Grips,
					WeightClass::Heavy  => ItemId::Suuns_Warfists,
					default => ItemId::_UNDEFINED,
				},
				4 => match ($weightClass) {
					WeightClass::Light  => ItemId::Suuns_Breeches,
					WeightClass::Medium => ItemId::Suuns_Leggings,
					WeightClass::Heavy  => ItemId::Suuns_Tassets,
					default => ItemId::_UNDEFINED,
				},
				5 => match ($weightClass) {
					WeightClass::Light  => ItemId::Suuns_Footwear,
					WeightClass::Medium => ItemId::Suuns_Striders,
					WeightClass::Heavy  => ItemId::Suuns_Greaves,
					default => ItemId::_UNDEFINED,
				},
				 6 => ItemId::Ad_Infinitum               ,
				 7 => ItemId::Mists_Charged_Jade_Talisman,
				 8 => ItemId::Mists_Charged_Jade_Talisman,
				 9 => ItemId::Mists_Charged_Jade_Band    ,
				10 => ItemId::Mists_Charged_Jade_Band    ,
				15 => ItemId::Mists_Charged_Jade_Pendant ,
				default => ItemId::_UNDEFINED,
			},
			StatId::Ritualists1, StatId::Ritualists2, StatId::Ritualists3, StatId::Ritualists4 => match($equipmentIndex) {
				0 => match ($weightClass) {
					WeightClass::Light  => ItemId::Togos_Masque,
					WeightClass::Medium => ItemId::Togos_Visage,
					WeightClass::Heavy  => ItemId::Togos_Visor,
					default => ItemId::_UNDEFINED,
				},
				1 => match ($weightClass) {
					WeightClass::Light  => ItemId::Togos_Epaulets,
					WeightClass::Medium => ItemId::Togos_Shoulderguard,
					WeightClass::Heavy  => ItemId::Togos_Pauldrons,
					default => ItemId::_UNDEFINED,
				},
				2 => match ($weightClass) {
					WeightClass::Light  => ItemId::Togos_Doublet,
					WeightClass::Medium => ItemId::Togos_Guise,
					WeightClass::Heavy  => ItemId::Togos_Breastplate,
					default => ItemId::_UNDEFINED,
				},
				3 => match ($weightClass) {
					WeightClass::Light  => ItemId::Togos_Wristguards,
					WeightClass::Medium => ItemId::Togos_Grips,
					WeightClass::Heavy  => ItemId::Togos_Warfists,
					default => ItemId::_UNDEFINED,
				},
				4 => match ($weightClass) {
					WeightClass::Light  => ItemId::Togos_Breeches,
					WeightClass::Medium => ItemId::Togos_Leggings,
					WeightClass::Heavy  => ItemId::Togos_Tassets,
					default => ItemId::_UNDEFINED,
				},
				5 => match ($weightClass) {
					WeightClass::Light  => ItemId::Togos_Footwear,
					WeightClass::Medium => ItemId::Togos_Striders,
					WeightClass::Heavy  => ItemId::Togos_Greaves,
					default => ItemId::_UNDEFINED,
				},
				 6 => ItemId::Ad_Infinitum               ,
				 7 => ItemId::Mists_Charged_Jade_Talisman,
				 8 => ItemId::Mists_Charged_Jade_Talisman,
				 9 => ItemId::Mists_Charged_Jade_Band    ,
				10 => ItemId::Mists_Charged_Jade_Band    ,
				15 => ItemId::Mists_Charged_Jade_Pendant ,
				default => ItemId::_UNDEFINED,
			},
			default => match($equipmentIndex) {
				0 => match ($weightClass) {
					WeightClass::Light  => ItemId::Illustrious_Masque,
					WeightClass::Medium => ItemId::Illustrious_Visage,
					WeightClass::Heavy  => ItemId::Illustrious_Visor,
					default => ItemId::_UNDEFINED,
				},
				1 => match ($weightClass) {
					WeightClass::Light  => ItemId::Illustrious_Epaulets,
					WeightClass::Medium => ItemId::Illustrious_Shoulderguard,
					WeightClass::Heavy  => ItemId::Illustrious_Pauldrons,
					default => ItemId::_UNDEFINED,
				},
				2 => match ($weightClass) {
					WeightClass::Light  => ItemId::Illustrious_Doublet,
					WeightClass::Medium => ItemId::Illustrious_Guise,
					WeightClass::Heavy  => ItemId::Illustrious_Breastplate,
					default => ItemId::_UNDEFINED,
				},
				3 => match ($weightClass) {
					WeightClass::Light  => ItemId::Illustrious_Wristguards,
					WeightClass::Medium => ItemId::Illustrious_Grips,
					WeightClass::Heavy  => ItemId::Illustrious_Warfists,
					default => ItemId::_UNDEFINED,
				},
				4 => match ($weightClass) {
					WeightClass::Light  => ItemId::Illustrious_Breeches,
					WeightClass::Medium => ItemId::Illustrious_Leggings,
					WeightClass::Heavy  => ItemId::Illustrious_Tassets,
					default => ItemId::_UNDEFINED,
				},
				5 => match ($weightClass) {
					WeightClass::Light  => ItemId::Illustrious_Footwear,
					WeightClass::Medium => ItemId::Illustrious_Striders,
					WeightClass::Heavy  => ItemId::Illustrious_Greaves,
					default => ItemId::_UNDEFINED,
				},
				 6 => ItemId::Quiver_of_a_Thousand_Arrows,
				 7 => ItemId::Black_Ice_Earing           ,
				 8 => ItemId::Asgeirs_Talisman           ,
				 9 => ItemId::Black_Ice_Band             ,
				10 => ItemId::Mistborn_Band              ,
				15 => ItemId::Asgeirs_Amulet             ,
				default => ItemId::_UNDEFINED,
			}
		};
	}

	public static function ResolveWeightClass(int $profession) : int
	{
		return match($profession) {
			Profession::Guardian     => WeightClass::Heavy,
			Profession::Warrior      => WeightClass::Heavy,
			Profession::Engineer     => WeightClass::Medium,
			Profession::Ranger       => WeightClass::Medium,
			Profession::Thief        => WeightClass::Medium,
			Profession::Elementalist => WeightClass::Light,
			Profession::Mesmer       => WeightClass::Light,
			Profession::Necromancer  => WeightClass::Light,
			Profession::Revenant     => WeightClass::Heavy,
			default => WeightClass::_UNDEFINED,
		};
	}
}
