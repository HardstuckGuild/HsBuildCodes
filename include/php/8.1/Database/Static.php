<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllSkills;

const FIRST_VERSIONED_VERSION = 3;
const CURRENT_VERSION = 3;
const OFFICIAL_CHAT_CODE_BYTE_LENGTH = 44;

const ALL_EQUIPMENT_COUNT = 16;
const ALL_INFUSION_COUNT = 21;

function DetermineCodeVersion(string $code) : int
{
	$vcValue = ord($code[0]);
	if($vcValue > count(TextLoader::INVERSE_CHARSET)) return -1;

	if(str_starts_with($code, 'v0_')) return 1;

	$potentialVersion = TextLoader::INVERSE_CHARSET[$vcValue];
	// version may be lower or uppercase
	if($potentialVersion >= 26) $potentialVersion -= 26;

	// NOTE(Rennorb): v1 codes start with the type indicator, which is never greater than 2. 
	// since this is also the first versioned version we can conclude tha values above the current version are invalid
	if($potentialVersion > CURRENT_VERSION) return -1;

	if($potentialVersion < FIRST_VERSIONED_VERSION) return 1;

	return $potentialVersion;
}

function ExistsAndIsTwoHanded(WeaponType $weaponType) {
	return $weaponType !== WeaponType::_UNDEFINED && IsTwoHanded($weaponType);
}
function IsTwoHanded(WeaponType $weaponType) : bool
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
			return false;

		case WeaponType::ShortBow:
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
			assert(false, "invalid weapon {$weaponType->name}");
			return false;
	}
}

function IsAquatic(WeaponType $weaponType) : bool
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

/** @remark This also handles unusual values from the characters api endpoint */
function ResolveLegend(Specialization $eliteSpec, ?string $str) : ?int
{ 
	switch ($str) {
		case "Legend1": return Legend::GLINT;
		case "Legend2": return Legend::SHIRO;
		case "Legend3": return Legend::JALIS;
		case "Legend4": return Legend::MALLYX;
		case "Legend5": return Legend::KALLA;
		case "Legend6": return Legend::VENTARI;
		default: return Overrides::ResolveLegend($eliteSpec, $str);
	};
}

function ResolveAltRevSkills(RevenantData $revData) : AllSkills
{
	$skills = new AllSkills();
	if($revData->Legend2 == Legend::_UNDEFINED) return $skills;

	$skills->Heal = match ($revData->Legend2)  {
		Legend::SHIRO   => SkillId::Enchanted_Daggers,
		Legend::VENTARI => SkillId::Project_Tranquility,
		Legend::MALLYX  => SkillId::Empowering_Misery,
		Legend::GLINT   => SkillId::Facet_of_Light,
		Legend::JALIS   => SkillId::Soothing_Stone1,
		Legend::KALLA   => SkillId::Breakrazors_Bastion,
		default => SkillId::_UNDEFINED,
	};
	$skills->Utility1 = $revData->AltUtilitySkill1;
	$skills->Utility2 = $revData->AltUtilitySkill2;
	$skills->Utility3 = $revData->AltUtilitySkill3;
	$skills->Elite = match ($revData->Legend2) {
		Legend::SHIRO   => SkillId::Jade_Winds1,
		Legend::VENTARI => SkillId::Energy_Expulsion1,
		Legend::MALLYX  => SkillId::Embrace_the_Darkness,
		Legend::GLINT   => SkillId::Facet_of_Chaos,
		Legend::JALIS   => SkillId::Rite_of_the_Great_Dwarf,
		Legend::KALLA   => SkillId::Soulcleaves_Summit,
		default => SkillId::_UNDEFINED,
	};

	return $skills;
}

function ResolveEffectiveWeapons(BuildCode $code, WeaponSetNumber $setNumber) : WeaponSet
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
		if(IsTwoHanded($mainSet->MainHand)) {
			$result->Sigil2 = $mainSet->Sigil2;
			return $result;
		}
	}
	else if($offSet->MainHand !== WeaponType::_UNDEFINED)
	{
		if(IsTwoHanded($offSet->MainHand))
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
function ResolveDummyItemForWeaponType(int $weaponType, int $statId) : int
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
function ResolveDummyItemForEquipment(int $equipmentIndex, int $weightClass, int $statId) : int
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

function ResolveWeightClass(Profession $profession) : WeightClass
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

/** @return True if the build code can have attributes at the given index. Useful for looping over all stats.
 *  @remark Does not check out of bounds. */
function HasAttributeSlot(BuildCode $code, int $index) : bool {
	return match ($index) {
		11 => $code->WeaponSet1->MainHand != WeaponType::_UNDEFINED,
		12 => $code->WeaponSet1->OffHand  != WeaponType::_UNDEFINED,
		13 => $code->WeaponSet2->MainHand != WeaponType::_UNDEFINED,
		14 => $code->WeaponSet2->OffHand  != WeaponType::_UNDEFINED,
		default => true,
	};
}

/** @return True if the build code can have infusions at the given index. Useful for looping over all infusions.
 *  @remark Does not check out of bounds. */
function HasInfusionSlot(BuildCode $code, int $index) : bool {
	return match ($index) {
		16 => $code->WeaponSet1->MainHand != WeaponType::_UNDEFINED,
		17 => $code->WeaponSet1->OffHand  != WeaponType::_UNDEFINED || ExistsAndIsTwoHanded($code->WeaponSet1->MainHand),
		18 => $code->WeaponSet2->MainHand != WeaponType::_UNDEFINED,
		19 => $code->WeaponSet2->OffHand  != WeaponType::_UNDEFINED || ExistsAndIsTwoHanded($code->WeaponSet2->MainHand),
		default => true,
	};
}

/** @return int Demoted rune/sigil. If the item is neither the original item is just returned. */
function LegendaryToSuperior(int $item) : int { return match ($item) {
	ItemId::Legendary_Rune_of_the_Afflicted    => ItemId::Superior_Rune_of_the_Afflicted   ,
	ItemId::Legendary_Rune_of_the_Lich         => ItemId::Superior_Rune_of_the_Lich        ,
	ItemId::Legendary_Rune_of_the_Traveler     => ItemId::Superior_Rune_of_the_Traveler    ,
	ItemId::Legendary_Rune_of_the_Flock        => ItemId::Superior_Rune_of_the_Flock       ,
	ItemId::Legendary_Rune_of_the_Dolyak       => ItemId::Superior_Rune_of_the_Dolyak      ,
	ItemId::Legendary_Rune_of_the_Pack         => ItemId::Superior_Rune_of_the_Pack        ,
	ItemId::Legendary_Rune_of_Infiltration     => ItemId::Superior_Rune_of_Infiltration    ,
	ItemId::Legendary_Rune_of_Mercy            => ItemId::Superior_Rune_of_Mercy           ,
	ItemId::Legendary_Rune_of_Vampirism        => ItemId::Superior_Rune_of_Vampirism       ,
	ItemId::Legendary_Rune_of_Strength         => ItemId::Superior_Rune_of_Strength        ,
	ItemId::Legendary_Rune_of_Rage             => ItemId::Superior_Rune_of_Rage            ,
	ItemId::Legendary_Rune_of_Speed            => ItemId::Superior_Rune_of_Speed           ,
	ItemId::Legendary_Rune_of_the_Eagle        => ItemId::Superior_Rune_of_the_Eagle       ,
	ItemId::Legendary_Rune_of_Rata_Sum         => ItemId::Superior_Rune_of_Rata_Sum        ,
	ItemId::Legendary_Rune_of_Hoelbrak         => ItemId::Superior_Rune_of_Hoelbrak        ,
	ItemId::Legendary_Rune_of_Divinity         => ItemId::Superior_Rune_of_Divinity        ,
	ItemId::Legendary_Rune_of_the_Grove        => ItemId::Superior_Rune_of_the_Grove       ,
	ItemId::Legendary_Rune_of_Scavenging       => ItemId::Superior_Rune_of_Scavenging      ,
	ItemId::Legendary_Rune_of_the_Citadel      => ItemId::Superior_Rune_of_the_Citadel     ,
	ItemId::Legendary_Rune_of_the_Earth        => ItemId::Superior_Rune_of_the_Earth       ,
	ItemId::Legendary_Rune_of_the_Fire         => ItemId::Superior_Rune_of_the_Fire        ,
	ItemId::Legendary_Rune_of_the_Air          => ItemId::Superior_Rune_of_the_Air         ,
	ItemId::Legendary_Rune_of_the_Ice          => ItemId::Superior_Rune_of_the_Ice         ,
	ItemId::Legendary_Rune_of_the_Ogre         => ItemId::Superior_Rune_of_the_Ogre        ,
	ItemId::Legendary_Rune_of_the_Undead       => ItemId::Superior_Rune_of_the_Undead      ,
	ItemId::Legendary_Rune_of_the_Krait        => ItemId::Superior_Rune_of_the_Krait       ,
	ItemId::Legendary_Rune_of_Balthazar        => ItemId::Superior_Rune_of_Balthazar       ,
	ItemId::Legendary_Rune_of_Dwayna           => ItemId::Superior_Rune_of_Dwayna          ,
	ItemId::Legendary_Rune_of_Melandru         => ItemId::Superior_Rune_of_Melandru        ,
	ItemId::Legendary_Rune_of_Lyssa            => ItemId::Superior_Rune_of_Lyssa           ,
	ItemId::Legendary_Rune_of_Grenth           => ItemId::Superior_Rune_of_Grenth          ,
	ItemId::Legendary_Rune_of_the_Privateer    => ItemId::Superior_Rune_of_the_Privateer   ,
	ItemId::Legendary_Rune_of_the_Golemancer   => ItemId::Superior_Rune_of_the_Golemancer  ,
	ItemId::Legendary_Rune_of_the_Centaur      => ItemId::Superior_Rune_of_the_Centaur     ,
	ItemId::Legendary_Rune_of_the_Wurm         => ItemId::Superior_Rune_of_the_Wurm        ,
	ItemId::Legendary_Rune_of_Svanir           => ItemId::Superior_Rune_of_Svanir          ,
	ItemId::Legendary_Rune_of_the_Flame_Legion => ItemId::Superior_Rune_of_the_Flame_Legion,
	ItemId::Legendary_Rune_of_the_Elementalist => ItemId::Superior_Rune_of_the_Elementalist,
	ItemId::Legendary_Rune_of_the_Mesmer       => ItemId::Superior_Rune_of_the_Mesmer      ,
	ItemId::Legendary_Rune_of_the_Necromancer  => ItemId::Superior_Rune_of_the_Necromancer ,
	ItemId::Legendary_Rune_of_the_Engineer     => ItemId::Superior_Rune_of_the_Engineer    ,
	ItemId::Legendary_Rune_of_the_Ranger       => ItemId::Superior_Rune_of_the_Ranger      ,
	ItemId::Legendary_Rune_of_the_Thief        => ItemId::Superior_Rune_of_the_Thief       ,
	ItemId::Legendary_Rune_of_the_Warrior      => ItemId::Superior_Rune_of_the_Warrior     ,
	ItemId::Legendary_Rune_of_the_Guardian     => ItemId::Superior_Rune_of_the_Guardian    ,
	ItemId::Legendary_Rune_of_the_Trooper      => ItemId::Superior_Rune_of_the_Trooper     ,
	ItemId::Legendary_Rune_of_the_Adventurer   => ItemId::Superior_Rune_of_the_Adventurer  ,
	ItemId::Legendary_Rune_of_the_Brawler      => ItemId::Superior_Rune_of_the_Brawler     ,
	ItemId::Legendary_Rune_of_the_Scholar      => ItemId::Superior_Rune_of_the_Scholar     ,
	ItemId::Legendary_Rune_of_the_Water        => ItemId::Superior_Rune_of_the_Water       ,
	ItemId::Legendary_Rune_of_the_Monk         => ItemId::Superior_Rune_of_the_Monk        ,
	ItemId::Legendary_Rune_of_the_Aristocracy  => ItemId::Superior_Rune_of_the_Aristocracy ,
	ItemId::Legendary_Rune_of_the_Nightmare    => ItemId::Superior_Rune_of_the_Nightmare   ,
	ItemId::Legendary_Rune_of_the_Forgeman     => ItemId::Superior_Rune_of_the_Forgeman    ,
	ItemId::Legendary_Rune_of_the_Baelfire     => ItemId::Superior_Rune_of_the_Baelfire    ,
	ItemId::Legendary_Rune_of_Sanctuary        => ItemId::Superior_Rune_of_Sanctuary       ,
	ItemId::Legendary_Rune_of_Orr              => ItemId::Superior_Rune_of_Orr             ,
	ItemId::Legendary_Rune_of_the_Mad_King     => ItemId::Superior_Rune_of_the_Mad_King    ,
	ItemId::Legendary_Rune_of_Altruism         => ItemId::Superior_Rune_of_Altruism        ,
	ItemId::Legendary_Rune_of_Exuberance       => ItemId::Superior_Rune_of_Exuberance      ,
	ItemId::Legendary_Rune_of_Tormenting       => ItemId::Superior_Rune_of_Tormenting      ,
	ItemId::Legendary_Rune_of_Perplexity       => ItemId::Superior_Rune_of_Perplexity      ,
	ItemId::Legendary_Rune_of_the_Sunless      => ItemId::Superior_Rune_of_the_Sunless     ,
	ItemId::Legendary_Rune_of_Antitoxin        => ItemId::Superior_Rune_of_Antitoxin       ,
	ItemId::Legendary_Rune_of_Resistance       => ItemId::Superior_Rune_of_Resistance      ,
	ItemId::Legendary_Rune_of_the_Trapper      => ItemId::Superior_Rune_of_the_Trapper     ,
	ItemId::Legendary_Rune_of_Radiance         => ItemId::Superior_Rune_of_Radiance        ,
	ItemId::Legendary_Rune_of_Evasion          => ItemId::Superior_Rune_of_Evasion         ,
	ItemId::Legendary_Rune_of_the_Defender     => ItemId::Superior_Rune_of_the_Defender    ,
	ItemId::Legendary_Rune_of_Snowfall         => ItemId::Superior_Rune_of_Snowfall        ,
	ItemId::Legendary_Rune_of_the_Revenant     => ItemId::Superior_Rune_of_the_Revenant    ,
	ItemId::Legendary_Rune_of_the_Druid        => ItemId::Superior_Rune_of_the_Druid       ,
	ItemId::Legendary_Rune_of_Leadership       => ItemId::Superior_Rune_of_Leadership      ,
	ItemId::Legendary_Rune_of_the_Reaper       => ItemId::Superior_Rune_of_the_Reaper      ,
	ItemId::Legendary_Rune_of_the_Scrapper     => ItemId::Superior_Rune_of_the_Scrapper    ,
	ItemId::Legendary_Rune_of_the_Berserker    => ItemId::Superior_Rune_of_the_Berserker   ,
	ItemId::Legendary_Rune_of_the_Daredevil    => ItemId::Superior_Rune_of_the_Daredevil   ,
	ItemId::Legendary_Rune_of_Thorns           => ItemId::Superior_Rune_of_Thorns          ,
	ItemId::Legendary_Rune_of_the_Chronomancer => ItemId::Superior_Rune_of_the_Chronomancer,
	ItemId::Legendary_Rune_of_Durability       => ItemId::Superior_Rune_of_Durability      ,
	ItemId::Legendary_Rune_of_the_Dragonhunter => ItemId::Superior_Rune_of_the_Dragonhunter,
	ItemId::Legendary_Rune_of_the_Herald       => ItemId::Superior_Rune_of_the_Herald      ,
	ItemId::Legendary_Rune_of_the_Tempest      => ItemId::Superior_Rune_of_the_Tempest     ,
	ItemId::Legendary_Rune_of_Surging          => ItemId::Superior_Rune_of_Surging         ,
	ItemId::Legendary_Rune_of_Natures_Bounty   => ItemId::Superior_Rune_of_Natures_Bounty  ,
	ItemId::Legendary_Rune_of_the_Holosmith    => ItemId::Superior_Rune_of_the_Holosmith   ,
	ItemId::Legendary_Rune_of_the_Deadeye      => ItemId::Superior_Rune_of_the_Deadeye     ,
	ItemId::Legendary_Rune_of_the_Firebrand    => ItemId::Superior_Rune_of_the_Firebrand   ,
	ItemId::Legendary_Rune_of_the_Cavalier     => ItemId::Superior_Rune_of_the_Cavalier    ,
	ItemId::Legendary_Rune_of_the_Weaver       => ItemId::Superior_Rune_of_the_Weaver      ,
	ItemId::Legendary_Rune_of_the_Renegade     => ItemId::Superior_Rune_of_the_Renegade    ,
	ItemId::Legendary_Rune_of_the_Scourge      => ItemId::Superior_Rune_of_the_Scourge     ,
	ItemId::Legendary_Rune_of_the_Soulbeast    => ItemId::Superior_Rune_of_the_Soulbeast   ,
	ItemId::Legendary_Rune_of_the_Mirage       => ItemId::Superior_Rune_of_the_Mirage      ,
	ItemId::Legendary_Rune_of_the_Rebirth      => ItemId::Superior_Rune_of_the_Rebirth     ,
	ItemId::Legendary_Rune_of_the_Spellbreaker => ItemId::Superior_Rune_of_the_Spellbreaker,
	ItemId::Legendary_Rune_of_the_Stars        => ItemId::Superior_Rune_of_the_Stars       ,
	ItemId::Legendary_Rune_of_the_Zephyrite    => ItemId::Superior_Rune_of_the_Zephyrite   ,
	ItemId::Legendary_Rune_of_Fireworks        => ItemId::Superior_Rune_of_Fireworks       ,

	ItemId::Legendary_Sigil_of_Fire              => ItemId::Superior_Sigil_of_Fire             ,
	ItemId::Legendary_Sigil_of_Water             => ItemId::Superior_Sigil_of_Water            ,
	ItemId::Legendary_Sigil_of_Air               => ItemId::Superior_Sigil_of_Air              ,
	ItemId::Legendary_Sigil_of_Ice               => ItemId::Superior_Sigil_of_Ice              ,
	ItemId::Legendary_Sigil_of_Earth             => ItemId::Superior_Sigil_of_Earth            ,
	ItemId::Legendary_Sigil_of_Rage              => ItemId::Superior_Sigil_of_Rage             ,
	ItemId::Legendary_Sigil_of_Strength          => ItemId::Superior_Sigil_of_Strength         ,
	ItemId::Legendary_Sigil_of_Frailty           => ItemId::Superior_Sigil_of_Frailty          ,
	ItemId::Legendary_Sigil_of_Blood             => ItemId::Superior_Sigil_of_Blood            ,
	ItemId::Legendary_Sigil_of_Purity            => ItemId::Superior_Sigil_of_Purity           ,
	ItemId::Legendary_Sigil_of_Nullification     => ItemId::Superior_Sigil_of_Nullification    ,
	ItemId::Legendary_Sigil_of_Bloodlust         => ItemId::Superior_Sigil_of_Bloodlust        ,
	ItemId::Legendary_Sigil_of_Corruption        => ItemId::Superior_Sigil_of_Corruption       ,
	ItemId::Legendary_Sigil_of_Perception        => ItemId::Superior_Sigil_of_Perception       ,
	ItemId::Legendary_Sigil_of_Life              => ItemId::Superior_Sigil_of_Life             ,
	ItemId::Legendary_Sigil_of_Demons            => ItemId::Superior_Sigil_of_Demons           ,
	ItemId::Legendary_Sigil_of_Benevolence       => ItemId::Superior_Sigil_of_Benevolence      ,
	ItemId::Legendary_Sigil_of_Speed             => ItemId::Superior_Sigil_of_Speed            ,
	ItemId::Legendary_Sigil_of_Luck              => ItemId::Superior_Sigil_of_Luck             ,
	ItemId::Legendary_Sigil_of_Stamina           => ItemId::Superior_Sigil_of_Stamina          ,
	ItemId::Legendary_Sigil_of_Restoration       => ItemId::Superior_Sigil_of_Restoration      ,
	ItemId::Legendary_Sigil_of_Hydromancy        => ItemId::Superior_Sigil_of_Hydromancy       ,
	ItemId::Legendary_Sigil_of_Leeching          => ItemId::Superior_Sigil_of_Leeching         ,
	ItemId::Legendary_Sigil_of_Vision            => ItemId::Superior_Sigil_of_Vision           ,
	ItemId::Legendary_Sigil_of_Battle            => ItemId::Superior_Sigil_of_Battle           ,
	ItemId::Legendary_Sigil_of_Geomancy          => ItemId::Superior_Sigil_of_Geomancy         ,
	ItemId::Legendary_Sigil_of_Energy            => ItemId::Superior_Sigil_of_Energy           ,
	ItemId::Legendary_Sigil_of_Doom              => ItemId::Superior_Sigil_of_Doom             ,
	ItemId::Legendary_Sigil_of_Agony             => ItemId::Superior_Sigil_of_Agony            ,
	ItemId::Legendary_Sigil_of_Force             => ItemId::Superior_Sigil_of_Force            ,
	ItemId::Legendary_Sigil_of_Accuracy          => ItemId::Superior_Sigil_of_Accuracy         ,
	ItemId::Legendary_Sigil_of_Peril             => ItemId::Superior_Sigil_of_Peril            ,
	ItemId::Legendary_Sigil_of_Smoldering        => ItemId::Superior_Sigil_of_Smoldering       ,
	ItemId::Legendary_Sigil_of_Hobbling          => ItemId::Superior_Sigil_of_Hobbling         ,
	ItemId::Legendary_Sigil_of_Chilling          => ItemId::Superior_Sigil_of_Chilling         ,
	ItemId::Legendary_Sigil_of_Venom             => ItemId::Superior_Sigil_of_Venom            ,
	ItemId::Legendary_Sigil_of_Debility          => ItemId::Superior_Sigil_of_Debility         ,
	ItemId::Legendary_Sigil_of_Paralyzation      => ItemId::Superior_Sigil_of_Paralyzation     ,
	ItemId::Legendary_Sigil_of_Undead_Slaying    => ItemId::Superior_Sigil_of_Undead_Slaying   ,
	ItemId::Legendary_Sigil_of_Centaur_Slaying   => ItemId::Superior_Sigil_of_Centaur_Slaying  ,
	ItemId::Legendary_Sigil_of_Grawl_Slaying     => ItemId::Superior_Sigil_of_Grawl_Slaying    ,
	ItemId::Legendary_Sigil_of_Icebrood_Slaying  => ItemId::Superior_Sigil_of_Icebrood_Slaying ,
	ItemId::Legendary_Sigil_of_Destroyer_Slaying => ItemId::Superior_Sigil_of_Destroyer_Slaying,
	ItemId::Legendary_Sigil_of_Ogre_Slaying      => ItemId::Superior_Sigil_of_Ogre_Slaying     ,
	ItemId::Legendary_Sigil_of_Serpent_Slaying   => ItemId::Superior_Sigil_of_Serpent_Slaying  ,
	ItemId::Legendary_Sigil_of_Elemental_Slaying => ItemId::Superior_Sigil_of_Elemental_Slaying,
	ItemId::Legendary_Sigil_of_Demon_Slaying     => ItemId::Superior_Sigil_of_Demon_Slaying    ,
	ItemId::Legendary_Sigil_of_Wrath             => ItemId::Superior_Sigil_of_Wrath            ,
	ItemId::Legendary_Sigil_of_Mad_Scientists    => ItemId::Superior_Sigil_of_Mad_Scientists   ,
	ItemId::Legendary_Sigil_of_Smothering        => ItemId::Superior_Sigil_of_Smothering       ,
	ItemId::Legendary_Sigil_of_Justice           => ItemId::Superior_Sigil_of_Justice          ,
	ItemId::Legendary_Sigil_of_Dreams            => ItemId::Superior_Sigil_of_Dreams           ,
	ItemId::Legendary_Sigil_of_Sorrow            => ItemId::Superior_Sigil_of_Sorrow           ,
	ItemId::Legendary_Sigil_of_Ghost_Slaying     => ItemId::Superior_Sigil_of_Ghost_Slaying    ,
	ItemId::Legendary_Sigil_of_Celerity          => ItemId::Superior_Sigil_of_Celerity         ,
	ItemId::Legendary_Sigil_of_Impact            => ItemId::Superior_Sigil_of_Impact           ,
	ItemId::Legendary_Sigil_of_the_Night         => ItemId::Superior_Sigil_of_the_Night        ,
	ItemId::Legendary_Sigil_of_Karka_Slaying     => ItemId::Superior_Sigil_of_Karka_Slaying    ,
	ItemId::Legendary_Sigil_of_Generosity        => ItemId::Superior_Sigil_of_Generosity       ,
	ItemId::Legendary_Sigil_of_Bursting          => ItemId::Superior_Sigil_of_Bursting         ,
	ItemId::Legendary_Sigil_of_Renewal           => ItemId::Superior_Sigil_of_Renewal          ,
	ItemId::Legendary_Sigil_of_Malice            => ItemId::Superior_Sigil_of_Malice           ,
	ItemId::Legendary_Sigil_of_Torment           => ItemId::Superior_Sigil_of_Torment          ,
	ItemId::Legendary_Sigil_of_Momentum          => ItemId::Superior_Sigil_of_Momentum         ,
	ItemId::Legendary_Sigil_of_Cleansing         => ItemId::Superior_Sigil_of_Cleansing        ,
	ItemId::Legendary_Sigil_of_Cruelty           => ItemId::Superior_Sigil_of_Cruelty          ,
	ItemId::Legendary_Sigil_of_Incapacitation    => ItemId::Superior_Sigil_of_Incapacitation   ,
	ItemId::Legendary_Sigil_of_Blight            => ItemId::Superior_Sigil_of_Blight           ,
	ItemId::Legendary_Sigil_of_Mischief          => ItemId::Superior_Sigil_of_Mischief         ,
	ItemId::Legendary_Sigil_of_Draining          => ItemId::Superior_Sigil_of_Draining         ,
	ItemId::Legendary_Sigil_of_Ruthlessness      => ItemId::Superior_Sigil_of_Ruthlessness     ,
	ItemId::Legendary_Sigil_of_Agility           => ItemId::Superior_Sigil_of_Agility          ,
	ItemId::Legendary_Sigil_of_Concentration     => ItemId::Superior_Sigil_of_Concentration    ,
	ItemId::Legendary_Sigil_of_Absorption        => ItemId::Superior_Sigil_of_Absorption       ,
	ItemId::Legendary_Sigil_of_Rending           => ItemId::Superior_Sigil_of_Rending          ,
	ItemId::Legendary_Sigil_of_Transference      => ItemId::Superior_Sigil_of_Transference     ,
	ItemId::Legendary_Sigil_of_Bounty            => ItemId::Superior_Sigil_of_Bounty           ,
	ItemId::Legendary_Sigil_of_Frenzy            => ItemId::Superior_Sigil_of_Frenzy           ,
	ItemId::Legendary_Sigil_of_Severance         => ItemId::Superior_Sigil_of_Severance        ,
	ItemId::Legendary_Sigil_of_the_Stars         => ItemId::Superior_Sigil_of_the_Stars        ,
	ItemId::Legendary_Sigil_of_Hologram_Slaying  => ItemId::Superior_Sigil_of_Hologram_Slaying ,

	default => $item,
};
}

enum CompressionOptions : int {
	case NONE                       = 0;
	case REARRANGE_INFUSIONS        = 1 << 0;
	case SUBSTITUTE_INFUSIONS       = 1 << 1;
	case REMOVE_NON_STAT_INFUSIONS  = 1 << 2;
	case REMOVE_SWIM_SPEED_INFUSION = 1 << 3;

	case ALL = 0xffffffff;
}

function Compress(BuildCode $code, CompressionOptions $options) : void
{
	if($options->value & CompressionOptions::REMOVE_SWIM_SPEED_INFUSION->value)
	{
		$name = ItemId::TryGetName($code->Infusions->Helmet);
		if(str_starts_with($name, '__not_defined')) //something something @performance
			$code->Infusions->Helmet = ItemId::_UNDEFINED;
	}

	if($options->value & CompressionOptions::REMOVE_NON_STAT_INFUSIONS->value)
	{
		for($i = 0; $i < ALL_INFUSION_COUNT - 1; $i++) //NOTE(Rennorb): skip the amulet with - 1, as enrichments can't be moved
		{
			$old = $code->Infusions[$i];
			$code->Infusions[$i] = match ($old) {
				ItemId::Agony_Infusion_01 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_02 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_03 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_04 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_05 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_06 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_07 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_08 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_09 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_10 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_11 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_12 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_13 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_14 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_15 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_16 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_17 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_18 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_19 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_20 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_21 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_22 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_23 => ItemId::_UNDEFINED,
				ItemId::Agony_Infusion_24 => ItemId::_UNDEFINED,
				default => $old,
			};
		}
	}

	if($options->value & CompressionOptions::SUBSTITUTE_INFUSIONS->value)
	{
		for($i = 0; $i < ALL_INFUSION_COUNT - 1; $i++) //NOTE(Rennorb): skip the amulet with - 1, as enrichments can't be moved
		{
			$old = $code->Infusions[$i];
			$old_name = ItemId::TryGetName($old);
			if($old === ItemId::_UNDEFINED || str_starts_with($old_name, '__not_defined')) continue;

			$last_underscore_pos = strrpos($old_name, '_');
			if($last_underscore_pos === false) continue;

			$code->Infusions[$i] = match (substr($old_name, $last_underscore_pos + 1)) {
				"Concentration"    => ItemId::WvW_Infusion_Concentration,
				"Condition_Damage" => ItemId::WvW_Infusion_Malign,
				"Expertise"        => ItemId::WvW_Infusion_Expertise,
				"Healing_Power"    => ItemId::WvW_Infusion_Healing,
				"Power"            => str_ends_with($old_name, "Healing_Power") ? ItemId::WvW_Infusion_Healing : ItemId::WvW_Infusion_Mighty,
				"Mighty"           => ItemId::WvW_Infusion_Mighty,
				"Precision",
				"Precise"          => ItemId::WvW_Infusion_Precise,
				"Toughness",
				"Resilient"        => ItemId::WvW_Infusion_Resilient,
				"Vitality",
				"Vital"            => ItemId::WvW_Infusion_Vital,
				default => str_ends_with($old_name, "Condition_Damage") ? ItemId::WvW_Infusion_Malign : $old,
			};
		}
	}

	if($options->value & CompressionOptions::REARRANGE_INFUSIONS->value && $code->Kind !== Kind::PvP)
	{
		$infusions = [];
		for($i = 0; $i < ALL_INFUSION_COUNT - 1; $i++) //NOTE(Rennorb): skip the amulet with - 1, as enrichments can't be moved
		{
			$item = $code->Infusions[$i];
			if($item === ItemId::_UNDEFINED) continue;
			$infusions[$item] = array_key_exists($item, $infusions) ? $infusions[$item] + 1 : 1;
		}


		$remaining = 0;
		$current_inf = ItemId::_UNDEFINED;
		$keys = array_keys($infusions);
		$current_key = -1;
		$NextInfusion = function() use(&$remaining, &$current_inf, &$current_key, $keys, $infusions) : int
		{
			if($remaining === 0)
			{
				$current_key++;
				if($current_key < count($keys))
				{
					$current_inf = $keys[$current_key];
					$remaining = $infusions[$current_inf];
				}
				else
				{
					$current_inf = ItemId::_UNDEFINED;
					$remaining = ALL_INFUSION_COUNT;
				}
			}

			$remaining--;
			return $current_inf;
		};

		if($code->Infusions->Amulet === ItemId::_UNDEFINED)
		{
			for($i = 0; $i < ALL_INFUSION_COUNT - 1; $i++)
				if(HasInfusionSlot($code, $i))
					$code->Infusions[$i] = $NextInfusion();
		}
		else
		{
			for($i = ALL_INFUSION_COUNT - 1; $i >= 0; $i--)
				if(HasInfusionSlot($code, $i))
					$code->Infusions[$i] = $NextInfusion();
		}
	}
}
