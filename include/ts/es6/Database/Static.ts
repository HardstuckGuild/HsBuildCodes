import { BuildCode, Kind, Legend, Profession, RevenantData, Specialization, WeaponSet, WeaponSetNumber, WeaponType, WeightClass } from "../Structures";
import TextLoader from "../TextLoader";
import { Assert } from "../Util/Static";
import { AllSkills } from "../Util/UtilStructs";
import ItemId from "./ItemIds";
import Overrides from "./Overrides";
import SkillId from "./SkillIds";
import StatId from "./StatIds";

export const FIRST_VERSIONED_VERSION = 3;
export const CURRENT_VERSION = 4;
export const OFFICIAL_CHAT_CODE_BYTE_LENGTH = 44;

export const ALL_EQUIPMENT_COUNT = 16;
export const ALL_INFUSION_COUNT = 21;

export function DetermineCodeVersion(code : string) : number
{
	const vcValue = code.charCodeAt(0);
	if(vcValue > TextLoader.INVERSE_CHARSET.length) return -1;

	if(code.startsWith('v0_')) return 1;

	let potentialVersion = TextLoader.INVERSE_CHARSET[vcValue];
	// version may be lower or uppercase
	if(potentialVersion >= 26) potentialVersion -= 26;

	// NOTE(Rennorb): v1 codes start with the type indicator, which is never greater than 2. 
	// since this is also the first versioned version we can conclude tha values above the current version are invalid
	if(potentialVersion > CURRENT_VERSION) return -1;

	if(potentialVersion < FIRST_VERSIONED_VERSION) return 1;

	return potentialVersion;
}

export const ExistsAndIsTwoHanded = (weaponType : WeaponType) => weaponType != WeaponType._UNDEFINED && IsTwoHanded(weaponType);
export function IsTwoHanded(weaponType : WeaponType) : boolean
{
	switch(weaponType)
	{
		case WeaponType.Axe:
		case WeaponType.Dagger:
		case WeaponType.Mace:
		case WeaponType.Pistol:
		case WeaponType.Sword:
		case WeaponType.Scepter:
		case WeaponType.Focus:
		case WeaponType.Shield:
		case WeaponType.Torch:
		case WeaponType.Warhorn:
			return false;

		case WeaponType.Shortbow:
		case WeaponType.Greatsword:
		case WeaponType.Hammer:
		case WeaponType.Longbow:
		case WeaponType.Rifle:
		case WeaponType.Staff:
		case WeaponType.HarpoonGun:
		case WeaponType.Spear:
		case WeaponType.Trident:
			return true;

		default: 
			Assert(false, "invalid weapon", weaponType);
			return false;
	}
}

export function IsAquatic(weaponType : WeaponType) : boolean
{
	switch(weaponType)
	{
		case WeaponType.Axe:
		case WeaponType.Dagger:
		case WeaponType.Mace:
		case WeaponType.Pistol:
		case WeaponType.Sword:
		case WeaponType.Scepter:
		case WeaponType.Focus:
		case WeaponType.Shield:
		case WeaponType.Torch:
		case WeaponType.Warhorn:
		case WeaponType.Shortbow:
		case WeaponType.Greatsword:
		case WeaponType.Hammer:
		case WeaponType.Longbow:
		case WeaponType.Rifle:
		case WeaponType.Staff:
			return false;
		
		case WeaponType.HarpoonGun:
		case WeaponType.Spear:
		case WeaponType.Trident:
			return true;

		default:
			Assert(false, "invalid weapon", weaponType);
			return false;
	}
}

/** @remark This also handles unusual values from the characters api endpoint */
export function ResolveLegend(eliteSpec : Specialization, str : string|null) : Legend|null
{ 
	switch (str) {
		case "Legend1": return Legend.GLINT;
		case "Legend2": return Legend.SHIRO;
		case "Legend3": return Legend.JALIS;
		case "Legend4": return Legend.MALLYX;
		case "Legend5": return Legend.KALLA;
		case "Legend6": return Legend.VENTARI;
		default: return Overrides.ResolveLegend(eliteSpec, str);
	};
}

export function ResolveAltRevSkills(revData : RevenantData) : AllSkills
{
	const skills = new AllSkills();
	if(revData.Legend2 == Legend._UNDEFINED) return skills;

	switch (revData.Legend2)  {
		case Legend.SHIRO     : skills.Heal = SkillId.Enchanted_Daggers;   break;
		case Legend.VENTARI   : skills.Heal = SkillId.Project_Tranquility; break;
		case Legend.MALLYX    : skills.Heal = SkillId.Empowering_Misery;   break;
		case Legend.GLINT     : skills.Heal = SkillId.Facet_of_Light;      break;
		case Legend.JALIS     : skills.Heal = SkillId.Soothing_Stone1;     break;
		case Legend.KALLA     : skills.Heal = SkillId.Breakrazors_Bastion; break;
		case Legend.VINDICATOR: skills.Heal = SkillId.Selfish_Spirit;      break;
	};
	skills.Utility1 = revData.AltUtilitySkill1;
	skills.Utility2 = revData.AltUtilitySkill2;
	skills.Utility3 = revData.AltUtilitySkill3;
	switch (revData.Legend2) {
		case Legend.SHIRO     : skills.Elite = SkillId.Jade_Winds1;             break;
		case Legend.VENTARI   : skills.Elite = SkillId.Energy_Expulsion1;       break;
		case Legend.MALLYX    : skills.Elite = SkillId.Embrace_the_Darkness;    break;
		case Legend.GLINT     : skills.Elite = SkillId.Facet_of_Chaos;          break;
		case Legend.JALIS     : skills.Elite = SkillId.Rite_of_the_Great_Dwarf; break;
		case Legend.KALLA     : skills.Elite = SkillId.Soulcleaves_Summit;      break;
		case Legend.VINDICATOR: skills.Elite = SkillId.Spear_of_Archemorus;     break;
	};

	return skills;
}

export function ResolveEffectiveWeapons(code : BuildCode, setNumber : WeaponSetNumber) : WeaponSet
{
	let mainSet : WeaponSet, offSet : WeaponSet;
	if(setNumber === WeaponSetNumber.Set1)
	{
		mainSet = code.WeaponSet1;
		offSet  = code.WeaponSet2;
	}
	else
	{
		mainSet = code.WeaponSet2;
		offSet  = code.WeaponSet1;
	}

	const result = new WeaponSet();

	if(mainSet.MainHand !== WeaponType._UNDEFINED)
	{
		result.MainHand = mainSet.MainHand;
		result.Sigil1   = mainSet.Sigil1;
		if(IsTwoHanded(mainSet.MainHand)) {
			result.Sigil2 = mainSet.Sigil2;
			return result;
		}
	}
	else if(offSet.MainHand !== WeaponType._UNDEFINED)
	{
		if(IsTwoHanded(offSet.MainHand))
		{
			if(mainSet.OffHand !== WeaponType._UNDEFINED)
			{
				result.OffHand = mainSet.OffHand;
				result.Sigil2  = mainSet.Sigil2;
				return result;
			}
			else
			{
				result.MainHand = offSet.MainHand;
				result.Sigil1   = offSet.Sigil1;
				result.Sigil2   = offSet.Sigil2;
				return result;
			}
		}
		else
		{
			result.MainHand = offSet.MainHand;
			result.Sigil1   = offSet.Sigil1;
		}
	}

	if(mainSet.OffHand !== WeaponType._UNDEFINED)
	{
		result.OffHand = mainSet.OffHand;
		result.Sigil2  = mainSet.Sigil2;
	}
	else if(offSet.OffHand !== WeaponType._UNDEFINED)
	{
		result.OffHand = offSet.OffHand;
		result.Sigil2  = offSet.Sigil2;
	}

	return result;
}

/** @remarks Also translates _UNDEFINED */
export function ResolveDummyItemForWeaponType(weaponType : WeaponType, statId : number) : ItemId
{
		switch (statId) {
		case StatId.Dragons1: case StatId.Dragons2: case StatId.Dragons3: case StatId.Dragons4: switch (weaponType) {
			case WeaponType.Axe        : return ItemId.Suuns_Reaver      ;
			case WeaponType.Dagger     : return ItemId.Suuns_Razor       ;
			case WeaponType.Mace       : return ItemId.Suuns_Flanged_Mace;
			case WeaponType.Pistol     : return ItemId.Suuns_Revolver    ;
			case WeaponType.Scepter    : return ItemId.Suuns_Wand        ;
			case WeaponType.Sword      : return ItemId.Suuns_Blade       ;
			case WeaponType.Focus      : return ItemId.Suuns_Artifact    ;
			case WeaponType.Shield     : return ItemId.Suuns_Bastion     ;
			case WeaponType.Torch      : return ItemId.Suuns_Brazier     ;
			case WeaponType.Warhorn    : return ItemId.Suuns_Herald      ;
			case WeaponType.Greatsword : return ItemId.Suuns_Claymore    ;
			case WeaponType.Hammer     : return ItemId.Suuns_Warhammer   ;
			case WeaponType.Longbow    : return ItemId.Suuns_Greatbow    ;
			case WeaponType.Rifle      : return ItemId.Suuns_Musket      ;
			case WeaponType.Shortbow   : return ItemId.Suuns_Short_Bow   ;
			case WeaponType.Staff      : return ItemId.Suuns_Spire       ;
			case WeaponType.HarpoonGun : return ItemId.Suuns_Harpoon_Gun ;
			case WeaponType.Spear      : return ItemId.Suuns_Impaler     ;
			case WeaponType.Trident    : return ItemId.Suuns_Trident     ;
			default: return ItemId._UNDEFINED;
		};
		case StatId.Ritualists1: case StatId.Ritualists2: case StatId.Ritualists3: case StatId.Ritualists4: switch (weaponType) {
			case WeaponType.Axe        : return ItemId.Togos_Reaver      ;
			case WeaponType.Dagger     : return ItemId.Togos_Razor       ;
			case WeaponType.Mace       : return ItemId.Togos_Flanged_Mace;
			case WeaponType.Pistol     : return ItemId.Togos_Revolver    ;
			case WeaponType.Scepter    : return ItemId.Togos_Wand        ;
			case WeaponType.Sword      : return ItemId.Togos_Blade       ;
			case WeaponType.Focus      : return ItemId.Togos_Artifact    ;
			case WeaponType.Shield     : return ItemId.Togos_Bastion     ;
			case WeaponType.Torch      : return ItemId.Togos_Brazier     ;
			case WeaponType.Warhorn    : return ItemId.Togos_Herald      ;
			case WeaponType.Greatsword : return ItemId.Togos_Claymore    ;
			case WeaponType.Hammer     : return ItemId.Togos_Warhammer   ;
			case WeaponType.Longbow    : return ItemId.Togos_Greatbow    ;
			case WeaponType.Rifle      : return ItemId.Togos_Musket      ;
			case WeaponType.Shortbow   : return ItemId.Togos_Short_Bow   ;
			case WeaponType.Staff      : return ItemId.Togos_Spire       ;
			case WeaponType.HarpoonGun : return ItemId.Togos_Harpoon_Gun ;
			case WeaponType.Spear      : return ItemId.Togos_Impaler     ;
			case WeaponType.Trident    : return ItemId.Togos_Trident     ;
			default: return ItemId._UNDEFINED;
		};
		default: switch (weaponType) {
			case WeaponType.Axe        : return ItemId.Mist_Lords_Axe          ;
			case WeaponType.Dagger     : return ItemId.Mist_Lords_Dagger       ;
			case WeaponType.Mace       : return ItemId.Mist_Lords_Mace         ;
			case WeaponType.Pistol     : return ItemId.Mist_Lords_Pistol       ;
			case WeaponType.Scepter    : return ItemId.Mist_Lords_Scepter      ;
			case WeaponType.Sword      : return ItemId.Mist_Lords_Sword        ;
			case WeaponType.Focus      : return ItemId.Mist_Lords_Focus        ;
			case WeaponType.Shield     : return ItemId.Mist_Lords_Shield       ;
			case WeaponType.Torch      : return ItemId.Mist_Lords_Torch        ;
			case WeaponType.Warhorn    : return ItemId.Mist_Lords_Warhorn      ;
			case WeaponType.Greatsword : return ItemId.Mist_Lords_Greatsword   ;
			case WeaponType.Hammer     : return ItemId.Mist_Lords_Hammer       ;
			case WeaponType.Longbow    : return ItemId.Mist_Lords_Longbow      ;
			case WeaponType.Rifle      : return ItemId.Mist_Lords_Rifle        ;
			case WeaponType.Shortbow   : return ItemId.Mist_Lords_Short_Bow    ;
			case WeaponType.Staff      : return ItemId.Mist_Lords_Staff        ;
			case WeaponType.HarpoonGun : return ItemId.Harpoon_Gun_of_the_Scion;
			case WeaponType.Spear      : return ItemId.Impaler_of_the_Scion    ;
			case WeaponType.Trident    : return ItemId.Trident_of_the_Scion    ;
			default: return ItemId._UNDEFINED;
		}
	};
}

/** @remarks Does not translate weapon items. Use ResolveDummyItemForWeaponType(WeaponType, StatId) for that. */
export function ResolveDummyItemForEquipment(equipmentIndex : number, weightClass : WeightClass, statId : StatId) : ItemId
{
	switch (statId) {
		case StatId.Dragons1: case StatId.Dragons2: case StatId.Dragons3: case StatId.Dragons4: switch (equipmentIndex) {
			case 0: switch (weightClass) {
				case WeightClass.Light : return ItemId.Suuns_Masque;
				case WeightClass.Medium: return ItemId.Suuns_Visage;
				case WeightClass.Heavy : return ItemId.Suuns_Visor;
			};
			case 1: switch (weightClass) {
				case WeightClass.Light : return ItemId.Suuns_Epaulets;
				case WeightClass.Medium: return ItemId.Suuns_Shoulderguard;
				case WeightClass.Heavy : return ItemId.Suuns_Pauldrons;
			};
			case 2: switch (weightClass) {
				case WeightClass.Light : return ItemId.Suuns_Doublet;
				case WeightClass.Medium: return ItemId.Suuns_Guise;
				case WeightClass.Heavy : return ItemId.Suuns_Breastplate;
			};
			case 3: switch (weightClass) {
				case WeightClass.Light : return ItemId.Suuns_Wristguards;
				case WeightClass.Medium: return ItemId.Suuns_Grips;
				case WeightClass.Heavy : return ItemId.Suuns_Warfists;
			};
			case 4: switch (weightClass) {
				case WeightClass.Light : return ItemId.Suuns_Breeches;
				case WeightClass.Medium: return ItemId.Suuns_Leggings;
				case WeightClass.Heavy : return ItemId.Suuns_Tassets;
			};
			case 5: switch (weightClass) {
				case WeightClass.Light : return ItemId.Suuns_Footwear;
				case WeightClass.Medium: return ItemId.Suuns_Striders;
				case WeightClass.Heavy : return ItemId.Suuns_Greaves;
			};
			case  6: return ItemId.Ad_Infinitum;
			case  7: return ItemId.Mists_Charged_Jade_Talisman;
			case  8: return ItemId.Mists_Charged_Jade_Talisman;
			case  9: return ItemId.Mists_Charged_Jade_Band_Attuned_Infused;
			case 10: return ItemId.Mists_Charged_Jade_Band_Attuned_Infused;
			case 15: return ItemId.Mists_Charged_Jade_Pendant;
		};
		case StatId.Ritualists1: case StatId.Ritualists2: case StatId.Ritualists3: case StatId.Ritualists4: switch(equipmentIndex) {
			case 0: switch (weightClass) {
				case WeightClass.Light : return ItemId.Togos_Masque;
				case WeightClass.Medium: return ItemId.Togos_Visage;
				case WeightClass.Heavy : return ItemId.Togos_Visor;
			};
			case 1: switch (weightClass) {
				case WeightClass.Light : return ItemId.Togos_Epaulets;
				case WeightClass.Medium: return ItemId.Togos_Shoulderguard;
				case WeightClass.Heavy : return ItemId.Togos_Pauldrons;
			};
			case 2: switch (weightClass) {
				case WeightClass.Light : return ItemId.Togos_Doublet;
				case WeightClass.Medium: return ItemId.Togos_Guise;
				case WeightClass.Heavy : return ItemId.Togos_Breastplate;
			};
			case 3: switch (weightClass) {
				case WeightClass.Light : return ItemId.Togos_Wristguards;
				case WeightClass.Medium: return ItemId.Togos_Grips;
				case WeightClass.Heavy : return ItemId.Togos_Warfists;
			};
			case 4: switch (weightClass) {
				case WeightClass.Light : return ItemId.Togos_Breeches;
				case WeightClass.Medium: return ItemId.Togos_Leggings;
				case WeightClass.Heavy : return ItemId.Togos_Tassets;
			};
			case 5: switch (weightClass) {
				case WeightClass.Light : return ItemId.Togos_Footwear;
				case WeightClass.Medium: return ItemId.Togos_Striders;
				case WeightClass.Heavy : return ItemId.Togos_Greaves;
			};
			case  6: return ItemId.Ad_Infinitum;
			case  7: return ItemId.Mists_Charged_Jade_Talisman;
			case  8: return ItemId.Mists_Charged_Jade_Talisman;
			case  9: return ItemId.Mists_Charged_Jade_Band_Attuned_Infused;
			case 10: return ItemId.Mists_Charged_Jade_Band_Attuned_Infused;
			case 15: return ItemId.Mists_Charged_Jade_Pendant;
		};
		default: switch (equipmentIndex) {
			case 0: switch (weightClass) {
				case WeightClass.Light : return ItemId.Illustrious_Masque;
				case WeightClass.Medium: return ItemId.Illustrious_Visage;
				case WeightClass.Heavy : return ItemId.Illustrious_Visor;
			};
			case 1: switch (weightClass) {
				case WeightClass.Light : return ItemId.Illustrious_Epaulets;
				case WeightClass.Medium: return ItemId.Illustrious_Shoulderguard;
				case WeightClass.Heavy : return ItemId.Illustrious_Pauldrons;
			};
			case 2: switch (weightClass) {
				case WeightClass.Light : return ItemId.Illustrious_Doublet;
				case WeightClass.Medium: return ItemId.Illustrious_Guise;
				case WeightClass.Heavy : return ItemId.Illustrious_Breastplate;
			};
			case 3: switch (weightClass) {
				case WeightClass.Light : return ItemId.Illustrious_Wristguards;
				case WeightClass.Medium: return ItemId.Illustrious_Grips;
				case WeightClass.Heavy : return ItemId.Illustrious_Warfists;
			};
			case 4: switch (weightClass) {
				case WeightClass.Light : return ItemId.Illustrious_Breeches;
				case WeightClass.Medium: return ItemId.Illustrious_Leggings;
				case WeightClass.Heavy : return ItemId.Illustrious_Tassets;
			};
			case 5: switch (weightClass) {
				case WeightClass.Light : return ItemId.Illustrious_Footwear;
				case WeightClass.Medium: return ItemId.Illustrious_Striders;
				case WeightClass.Heavy : return ItemId.Illustrious_Greaves;
			};
			case  6: return ItemId.Quiver_of_a_Thousand_Arrows_Infused;
			case  7: return ItemId.Black_Ice_Earing;
			case  8: return ItemId.Asgeirs_Talisman;
			case  9: return ItemId.Black_Ice_Band_Attuned_Infused;
			case 10: return ItemId.Mistborn_Band_Attuned_Infused;
			case 15: return ItemId.Asgeirs_Amulet;
		}
	};
	return ItemId._UNDEFINED;
}

export function ResolveWeightClass(profession : Profession) : WeightClass
{
	switch (profession) {
		case Profession.Guardian    : return WeightClass.Heavy;
		case Profession.Warrior     : return WeightClass.Heavy;
		case Profession.Engineer    : return WeightClass.Medium;
		case Profession.Ranger      : return WeightClass.Medium;
		case Profession.Thief       : return WeightClass.Medium;
		case Profession.Elementalist: return WeightClass.Light;
		case Profession.Mesmer      : return WeightClass.Light;
		case Profession.Necromancer : return WeightClass.Light;
		case Profession.Revenant    : return WeightClass.Heavy;
		default: return WeightClass._UNDEFINED;
	};
}

/**
 * @return True if the build code can have attributes at the given index. Useful for looping over all stats.
 * @remark Does not check out of bounds. */
export function HasAttributeSlot(code : BuildCode, index : number) : boolean {
	switch (index) {
		case 11: return code.WeaponSet1.MainHand != WeaponType._UNDEFINED;
		case 12: return code.WeaponSet1.OffHand  != WeaponType._UNDEFINED;
		case 13: return code.WeaponSet2.MainHand != WeaponType._UNDEFINED;
		case 14: return code.WeaponSet2.OffHand  != WeaponType._UNDEFINED;
		default: return true;
	};
}

/**
 * @return True if the build code can have infusions at the given index. Useful for looping over all infusions.
 * @remark Does not check out of bounds. */
export function HasInfusionSlot(code : BuildCode, index : number) : boolean {
	switch (index) {
		case 16: return code.WeaponSet1.MainHand != WeaponType._UNDEFINED;
		case 17: return code.WeaponSet1.OffHand  != WeaponType._UNDEFINED || ExistsAndIsTwoHanded(code.WeaponSet1.MainHand);
		case 18: return code.WeaponSet2.MainHand != WeaponType._UNDEFINED;
		case 19: return code.WeaponSet2.OffHand  != WeaponType._UNDEFINED || ExistsAndIsTwoHanded(code.WeaponSet2.MainHand);
		default: return true;
	};
}

/** @return int Demoted rune/sigil. If the item is neither the original item is just returned. */
export function LegendaryToSuperior(item : ItemId) : ItemId 
{
	switch (item) {
		case ItemId.Legendary_Rune_of_the_Afflicted   : return ItemId.Superior_Rune_of_the_Afflicted   ;
		case ItemId.Legendary_Rune_of_the_Lich        : return ItemId.Superior_Rune_of_the_Lich        ;
		case ItemId.Legendary_Rune_of_the_Traveler    : return ItemId.Superior_Rune_of_the_Traveler    ;
		case ItemId.Legendary_Rune_of_the_Flock       : return ItemId.Superior_Rune_of_the_Flock       ;
		case ItemId.Legendary_Rune_of_the_Dolyak      : return ItemId.Superior_Rune_of_the_Dolyak      ;
		case ItemId.Legendary_Rune_of_the_Pack        : return ItemId.Superior_Rune_of_the_Pack        ;
		case ItemId.Legendary_Rune_of_Infiltration    : return ItemId.Superior_Rune_of_Infiltration    ;
		case ItemId.Legendary_Rune_of_Mercy           : return ItemId.Superior_Rune_of_Mercy           ;
		case ItemId.Legendary_Rune_of_Vampirism       : return ItemId.Superior_Rune_of_Vampirism       ;
		case ItemId.Legendary_Rune_of_Strength        : return ItemId.Superior_Rune_of_Strength        ;
		case ItemId.Legendary_Rune_of_Rage            : return ItemId.Superior_Rune_of_Rage            ;
		case ItemId.Legendary_Rune_of_Speed           : return ItemId.Superior_Rune_of_Speed           ;
		case ItemId.Legendary_Rune_of_the_Eagle       : return ItemId.Superior_Rune_of_the_Eagle       ;
		case ItemId.Legendary_Rune_of_Rata_Sum        : return ItemId.Superior_Rune_of_Rata_Sum        ;
		case ItemId.Legendary_Rune_of_Hoelbrak        : return ItemId.Superior_Rune_of_Hoelbrak        ;
		case ItemId.Legendary_Rune_of_Divinity        : return ItemId.Superior_Rune_of_Divinity        ;
		case ItemId.Legendary_Rune_of_the_Grove       : return ItemId.Superior_Rune_of_the_Grove       ;
		case ItemId.Legendary_Rune_of_Scavenging      : return ItemId.Superior_Rune_of_Scavenging      ;
		case ItemId.Legendary_Rune_of_the_Citadel     : return ItemId.Superior_Rune_of_the_Citadel     ;
		case ItemId.Legendary_Rune_of_the_Earth       : return ItemId.Superior_Rune_of_the_Earth       ;
		case ItemId.Legendary_Rune_of_the_Fire        : return ItemId.Superior_Rune_of_the_Fire        ;
		case ItemId.Legendary_Rune_of_the_Air         : return ItemId.Superior_Rune_of_the_Air         ;
		case ItemId.Legendary_Rune_of_the_Ice         : return ItemId.Superior_Rune_of_the_Ice         ;
		case ItemId.Legendary_Rune_of_the_Ogre        : return ItemId.Superior_Rune_of_the_Ogre        ;
		case ItemId.Legendary_Rune_of_the_Undead      : return ItemId.Superior_Rune_of_the_Undead      ;
		case ItemId.Legendary_Rune_of_the_Krait       : return ItemId.Superior_Rune_of_the_Krait       ;
		case ItemId.Legendary_Rune_of_Balthazar       : return ItemId.Superior_Rune_of_Balthazar       ;
		case ItemId.Legendary_Rune_of_Dwayna          : return ItemId.Superior_Rune_of_Dwayna          ;
		case ItemId.Legendary_Rune_of_Melandru        : return ItemId.Superior_Rune_of_Melandru        ;
		case ItemId.Legendary_Rune_of_Lyssa           : return ItemId.Superior_Rune_of_Lyssa           ;
		case ItemId.Legendary_Rune_of_Grenth          : return ItemId.Superior_Rune_of_Grenth          ;
		case ItemId.Legendary_Rune_of_the_Privateer   : return ItemId.Superior_Rune_of_the_Privateer   ;
		case ItemId.Legendary_Rune_of_the_Golemancer  : return ItemId.Superior_Rune_of_the_Golemancer  ;
		case ItemId.Legendary_Rune_of_the_Centaur     : return ItemId.Superior_Rune_of_the_Centaur     ;
		case ItemId.Legendary_Rune_of_the_Wurm        : return ItemId.Superior_Rune_of_the_Wurm        ;
		case ItemId.Legendary_Rune_of_Svanir          : return ItemId.Superior_Rune_of_Svanir          ;
		case ItemId.Legendary_Rune_of_the_Flame_Legion: return ItemId.Superior_Rune_of_the_Flame_Legion;
		case ItemId.Legendary_Rune_of_the_Elementalist: return ItemId.Superior_Rune_of_the_Elementalist;
		case ItemId.Legendary_Rune_of_the_Mesmer      : return ItemId.Superior_Rune_of_the_Mesmer      ;
		case ItemId.Legendary_Rune_of_the_Necromancer : return ItemId.Superior_Rune_of_the_Necromancer ;
		case ItemId.Legendary_Rune_of_the_Engineer    : return ItemId.Superior_Rune_of_the_Engineer    ;
		case ItemId.Legendary_Rune_of_the_Ranger      : return ItemId.Superior_Rune_of_the_Ranger      ;
		case ItemId.Legendary_Rune_of_the_Thief       : return ItemId.Superior_Rune_of_the_Thief       ;
		case ItemId.Legendary_Rune_of_the_Warrior     : return ItemId.Superior_Rune_of_the_Warrior     ;
		case ItemId.Legendary_Rune_of_the_Guardian    : return ItemId.Superior_Rune_of_the_Guardian    ;
		case ItemId.Legendary_Rune_of_the_Trooper     : return ItemId.Superior_Rune_of_the_Trooper     ;
		case ItemId.Legendary_Rune_of_the_Adventurer  : return ItemId.Superior_Rune_of_the_Adventurer  ;
		case ItemId.Legendary_Rune_of_the_Brawler     : return ItemId.Superior_Rune_of_the_Brawler     ;
		case ItemId.Legendary_Rune_of_the_Scholar     : return ItemId.Superior_Rune_of_the_Scholar     ;
		case ItemId.Legendary_Rune_of_the_Water       : return ItemId.Superior_Rune_of_the_Water       ;
		case ItemId.Legendary_Rune_of_the_Monk        : return ItemId.Superior_Rune_of_the_Monk        ;
		case ItemId.Legendary_Rune_of_the_Aristocracy : return ItemId.Superior_Rune_of_the_Aristocracy ;
		case ItemId.Legendary_Rune_of_the_Nightmare   : return ItemId.Superior_Rune_of_the_Nightmare   ;
		case ItemId.Legendary_Rune_of_the_Forgeman    : return ItemId.Superior_Rune_of_the_Forgeman    ;
		case ItemId.Legendary_Rune_of_the_Baelfire    : return ItemId.Superior_Rune_of_the_Baelfire    ;
		case ItemId.Legendary_Rune_of_Sanctuary       : return ItemId.Superior_Rune_of_Sanctuary       ;
		case ItemId.Legendary_Rune_of_Orr             : return ItemId.Superior_Rune_of_Orr             ;
		case ItemId.Legendary_Rune_of_the_Mad_King    : return ItemId.Superior_Rune_of_the_Mad_King    ;
		case ItemId.Legendary_Rune_of_Altruism        : return ItemId.Superior_Rune_of_Altruism        ;
		case ItemId.Legendary_Rune_of_Exuberance      : return ItemId.Superior_Rune_of_Exuberance      ;
		case ItemId.Legendary_Rune_of_Tormenting      : return ItemId.Superior_Rune_of_Tormenting      ;
		case ItemId.Legendary_Rune_of_Perplexity      : return ItemId.Superior_Rune_of_Perplexity      ;
		case ItemId.Legendary_Rune_of_the_Sunless     : return ItemId.Superior_Rune_of_the_Sunless     ;
		case ItemId.Legendary_Rune_of_Antitoxin       : return ItemId.Superior_Rune_of_Antitoxin       ;
		case ItemId.Legendary_Rune_of_Resistance      : return ItemId.Superior_Rune_of_Resistance      ;
		case ItemId.Legendary_Rune_of_the_Trapper     : return ItemId.Superior_Rune_of_the_Trapper     ;
		case ItemId.Legendary_Rune_of_Radiance        : return ItemId.Superior_Rune_of_Radiance        ;
		case ItemId.Legendary_Rune_of_Evasion         : return ItemId.Superior_Rune_of_Evasion         ;
		case ItemId.Legendary_Rune_of_the_Defender    : return ItemId.Superior_Rune_of_the_Defender    ;
		case ItemId.Legendary_Rune_of_Snowfall        : return ItemId.Superior_Rune_of_Snowfall        ;
		case ItemId.Legendary_Rune_of_the_Revenant    : return ItemId.Superior_Rune_of_the_Revenant    ;
		case ItemId.Legendary_Rune_of_the_Druid       : return ItemId.Superior_Rune_of_the_Druid       ;
		case ItemId.Legendary_Rune_of_Leadership      : return ItemId.Superior_Rune_of_Leadership      ;
		case ItemId.Legendary_Rune_of_the_Reaper      : return ItemId.Superior_Rune_of_the_Reaper      ;
		case ItemId.Legendary_Rune_of_the_Scrapper    : return ItemId.Superior_Rune_of_the_Scrapper    ;
		case ItemId.Legendary_Rune_of_the_Berserker   : return ItemId.Superior_Rune_of_the_Berserker   ;
		case ItemId.Legendary_Rune_of_the_Daredevil   : return ItemId.Superior_Rune_of_the_Daredevil   ;
		case ItemId.Legendary_Rune_of_Thorns          : return ItemId.Superior_Rune_of_Thorns          ;
		case ItemId.Legendary_Rune_of_the_Chronomancer: return ItemId.Superior_Rune_of_the_Chronomancer;
		case ItemId.Legendary_Rune_of_Durability      : return ItemId.Superior_Rune_of_Durability      ;
		case ItemId.Legendary_Rune_of_the_Dragonhunter: return ItemId.Superior_Rune_of_the_Dragonhunter;
		case ItemId.Legendary_Rune_of_the_Herald      : return ItemId.Superior_Rune_of_the_Herald      ;
		case ItemId.Legendary_Rune_of_the_Tempest     : return ItemId.Superior_Rune_of_the_Tempest     ;
		case ItemId.Legendary_Rune_of_Surging         : return ItemId.Superior_Rune_of_Surging         ;
		case ItemId.Legendary_Rune_of_Natures_Bounty  : return ItemId.Superior_Rune_of_Natures_Bounty  ;
		case ItemId.Legendary_Rune_of_the_Holosmith   : return ItemId.Superior_Rune_of_the_Holosmith   ;
		case ItemId.Legendary_Rune_of_the_Deadeye     : return ItemId.Superior_Rune_of_the_Deadeye     ;
		case ItemId.Legendary_Rune_of_the_Firebrand   : return ItemId.Superior_Rune_of_the_Firebrand   ;
		case ItemId.Legendary_Rune_of_the_Cavalier    : return ItemId.Superior_Rune_of_the_Cavalier    ;
		case ItemId.Legendary_Rune_of_the_Weaver      : return ItemId.Superior_Rune_of_the_Weaver      ;
		case ItemId.Legendary_Rune_of_the_Renegade    : return ItemId.Superior_Rune_of_the_Renegade    ;
		case ItemId.Legendary_Rune_of_the_Scourge     : return ItemId.Superior_Rune_of_the_Scourge     ;
		case ItemId.Legendary_Rune_of_the_Soulbeast   : return ItemId.Superior_Rune_of_the_Soulbeast   ;
		case ItemId.Legendary_Rune_of_the_Mirage      : return ItemId.Superior_Rune_of_the_Mirage      ;
		case ItemId.Legendary_Rune_of_the_Rebirth     : return ItemId.Superior_Rune_of_the_Rebirth     ;
		case ItemId.Legendary_Rune_of_the_Spellbreaker: return ItemId.Superior_Rune_of_the_Spellbreaker;
		case ItemId.Legendary_Rune_of_the_Stars       : return ItemId.Superior_Rune_of_the_Stars       ;
		case ItemId.Legendary_Rune_of_the_Zephyrite   : return ItemId.Superior_Rune_of_the_Zephyrite   ;
		case ItemId.Legendary_Rune_of_Fireworks       : return ItemId.Superior_Rune_of_Fireworks       ;

		case ItemId.Legendary_Sigil_of_Fire             : return ItemId.Superior_Sigil_of_Fire             ;
		case ItemId.Legendary_Sigil_of_Water            : return ItemId.Superior_Sigil_of_Water            ;
		case ItemId.Legendary_Sigil_of_Air              : return ItemId.Superior_Sigil_of_Air              ;
		case ItemId.Legendary_Sigil_of_Ice              : return ItemId.Superior_Sigil_of_Ice              ;
		case ItemId.Legendary_Sigil_of_Earth            : return ItemId.Superior_Sigil_of_Earth            ;
		case ItemId.Legendary_Sigil_of_Rage             : return ItemId.Superior_Sigil_of_Rage             ;
		case ItemId.Legendary_Sigil_of_Strength         : return ItemId.Superior_Sigil_of_Strength         ;
		case ItemId.Legendary_Sigil_of_Frailty          : return ItemId.Superior_Sigil_of_Frailty          ;
		case ItemId.Legendary_Sigil_of_Blood            : return ItemId.Superior_Sigil_of_Blood            ;
		case ItemId.Legendary_Sigil_of_Purity           : return ItemId.Superior_Sigil_of_Purity           ;
		case ItemId.Legendary_Sigil_of_Nullification    : return ItemId.Superior_Sigil_of_Nullification    ;
		case ItemId.Legendary_Sigil_of_Bloodlust        : return ItemId.Superior_Sigil_of_Bloodlust        ;
		case ItemId.Legendary_Sigil_of_Corruption       : return ItemId.Superior_Sigil_of_Corruption       ;
		case ItemId.Legendary_Sigil_of_Perception       : return ItemId.Superior_Sigil_of_Perception       ;
		case ItemId.Legendary_Sigil_of_Life             : return ItemId.Superior_Sigil_of_Life             ;
		case ItemId.Legendary_Sigil_of_Demons           : return ItemId.Superior_Sigil_of_Demons           ;
		case ItemId.Legendary_Sigil_of_Benevolence      : return ItemId.Superior_Sigil_of_Benevolence      ;
		case ItemId.Legendary_Sigil_of_Speed            : return ItemId.Superior_Sigil_of_Speed            ;
		case ItemId.Legendary_Sigil_of_Luck             : return ItemId.Superior_Sigil_of_Luck             ;
		case ItemId.Legendary_Sigil_of_Stamina          : return ItemId.Superior_Sigil_of_Stamina          ;
		case ItemId.Legendary_Sigil_of_Restoration      : return ItemId.Superior_Sigil_of_Restoration      ;
		case ItemId.Legendary_Sigil_of_Hydromancy       : return ItemId.Superior_Sigil_of_Hydromancy       ;
		case ItemId.Legendary_Sigil_of_Leeching         : return ItemId.Superior_Sigil_of_Leeching         ;
		case ItemId.Legendary_Sigil_of_Vision           : return ItemId.Superior_Sigil_of_Vision           ;
		case ItemId.Legendary_Sigil_of_Battle           : return ItemId.Superior_Sigil_of_Battle           ;
		case ItemId.Legendary_Sigil_of_Geomancy         : return ItemId.Superior_Sigil_of_Geomancy         ;
		case ItemId.Legendary_Sigil_of_Energy           : return ItemId.Superior_Sigil_of_Energy           ;
		case ItemId.Legendary_Sigil_of_Doom             : return ItemId.Superior_Sigil_of_Doom             ;
		case ItemId.Legendary_Sigil_of_Agony            : return ItemId.Superior_Sigil_of_Agony            ;
		case ItemId.Legendary_Sigil_of_Force            : return ItemId.Superior_Sigil_of_Force            ;
		case ItemId.Legendary_Sigil_of_Accuracy         : return ItemId.Superior_Sigil_of_Accuracy         ;
		case ItemId.Legendary_Sigil_of_Peril            : return ItemId.Superior_Sigil_of_Peril            ;
		case ItemId.Legendary_Sigil_of_Smoldering       : return ItemId.Superior_Sigil_of_Smoldering       ;
		case ItemId.Legendary_Sigil_of_Hobbling         : return ItemId.Superior_Sigil_of_Hobbling         ;
		case ItemId.Legendary_Sigil_of_Chilling         : return ItemId.Superior_Sigil_of_Chilling         ;
		case ItemId.Legendary_Sigil_of_Venom            : return ItemId.Superior_Sigil_of_Venom            ;
		case ItemId.Legendary_Sigil_of_Debility         : return ItemId.Superior_Sigil_of_Debility         ;
		case ItemId.Legendary_Sigil_of_Paralyzation     : return ItemId.Superior_Sigil_of_Paralyzation     ;
		case ItemId.Legendary_Sigil_of_Undead_Slaying   : return ItemId.Superior_Sigil_of_Undead_Slaying   ;
		case ItemId.Legendary_Sigil_of_Centaur_Slaying  : return ItemId.Superior_Sigil_of_Centaur_Slaying  ;
		case ItemId.Legendary_Sigil_of_Grawl_Slaying    : return ItemId.Superior_Sigil_of_Grawl_Slaying    ;
		case ItemId.Legendary_Sigil_of_Icebrood_Slaying : return ItemId.Superior_Sigil_of_Icebrood_Slaying ;
		case ItemId.Legendary_Sigil_of_Destroyer_Slaying: return ItemId.Superior_Sigil_of_Destroyer_Slaying;
		case ItemId.Legendary_Sigil_of_Ogre_Slaying     : return ItemId.Superior_Sigil_of_Ogre_Slaying     ;
		case ItemId.Legendary_Sigil_of_Serpent_Slaying  : return ItemId.Superior_Sigil_of_Serpent_Slaying  ;
		case ItemId.Legendary_Sigil_of_Elemental_Slaying: return ItemId.Superior_Sigil_of_Elemental_Slaying;
		case ItemId.Legendary_Sigil_of_Demon_Slaying    : return ItemId.Superior_Sigil_of_Demon_Slaying    ;
		case ItemId.Legendary_Sigil_of_Wrath            : return ItemId.Superior_Sigil_of_Wrath            ;
		case ItemId.Legendary_Sigil_of_Mad_Scientists   : return ItemId.Superior_Sigil_of_Mad_Scientists   ;
		case ItemId.Legendary_Sigil_of_Smothering       : return ItemId.Superior_Sigil_of_Smothering       ;
		case ItemId.Legendary_Sigil_of_Justice          : return ItemId.Superior_Sigil_of_Justice          ;
		case ItemId.Legendary_Sigil_of_Dreams           : return ItemId.Superior_Sigil_of_Dreams           ;
		case ItemId.Legendary_Sigil_of_Sorrow           : return ItemId.Superior_Sigil_of_Sorrow           ;
		case ItemId.Legendary_Sigil_of_Ghost_Slaying    : return ItemId.Superior_Sigil_of_Ghost_Slaying    ;
		case ItemId.Legendary_Sigil_of_Celerity         : return ItemId.Superior_Sigil_of_Celerity         ;
		case ItemId.Legendary_Sigil_of_Impact           : return ItemId.Superior_Sigil_of_Impact           ;
		case ItemId.Legendary_Sigil_of_the_Night        : return ItemId.Superior_Sigil_of_the_Night        ;
		case ItemId.Legendary_Sigil_of_Karka_Slaying    : return ItemId.Superior_Sigil_of_Karka_Slaying    ;
		case ItemId.Legendary_Sigil_of_Generosity       : return ItemId.Superior_Sigil_of_Generosity       ;
		case ItemId.Legendary_Sigil_of_Bursting         : return ItemId.Superior_Sigil_of_Bursting         ;
		case ItemId.Legendary_Sigil_of_Renewal          : return ItemId.Superior_Sigil_of_Renewal          ;
		case ItemId.Legendary_Sigil_of_Malice           : return ItemId.Superior_Sigil_of_Malice           ;
		case ItemId.Legendary_Sigil_of_Torment          : return ItemId.Superior_Sigil_of_Torment          ;
		case ItemId.Legendary_Sigil_of_Momentum         : return ItemId.Superior_Sigil_of_Momentum         ;
		case ItemId.Legendary_Sigil_of_Cleansing        : return ItemId.Superior_Sigil_of_Cleansing        ;
		case ItemId.Legendary_Sigil_of_Cruelty          : return ItemId.Superior_Sigil_of_Cruelty          ;
		case ItemId.Legendary_Sigil_of_Incapacitation   : return ItemId.Superior_Sigil_of_Incapacitation   ;
		case ItemId.Legendary_Sigil_of_Blight           : return ItemId.Superior_Sigil_of_Blight           ;
		case ItemId.Legendary_Sigil_of_Mischief         : return ItemId.Superior_Sigil_of_Mischief         ;
		case ItemId.Legendary_Sigil_of_Draining         : return ItemId.Superior_Sigil_of_Draining         ;
		case ItemId.Legendary_Sigil_of_Ruthlessness     : return ItemId.Superior_Sigil_of_Ruthlessness     ;
		case ItemId.Legendary_Sigil_of_Agility          : return ItemId.Superior_Sigil_of_Agility          ;
		case ItemId.Legendary_Sigil_of_Concentration    : return ItemId.Superior_Sigil_of_Concentration    ;
		case ItemId.Legendary_Sigil_of_Absorption       : return ItemId.Superior_Sigil_of_Absorption       ;
		case ItemId.Legendary_Sigil_of_Rending          : return ItemId.Superior_Sigil_of_Rending          ;
		case ItemId.Legendary_Sigil_of_Transference     : return ItemId.Superior_Sigil_of_Transference     ;
		case ItemId.Legendary_Sigil_of_Bounty           : return ItemId.Superior_Sigil_of_Bounty           ;
		case ItemId.Legendary_Sigil_of_Frenzy           : return ItemId.Superior_Sigil_of_Frenzy           ;
		case ItemId.Legendary_Sigil_of_Severance        : return ItemId.Superior_Sigil_of_Severance        ;
		case ItemId.Legendary_Sigil_of_the_Stars        : return ItemId.Superior_Sigil_of_the_Stars        ;
		case ItemId.Legendary_Sigil_of_Hologram_Slaying : return ItemId.Superior_Sigil_of_Hologram_Slaying ;

		default: return item;
	}
}

export enum CompressionOptions {
	NONE                       = 0,
	REARRANGE_INFUSIONS        = 1 << 0,
	SUBSTITUTE_INFUSIONS       = 1 << 1,
	REMOVE_NON_STAT_INFUSIONS  = 1 << 2,
	REMOVE_SWIM_SPEED_INFUSION = 1 << 3,
	ALL = 0xffffffff
}

export function Compress(code : BuildCode, options : CompressionOptions) : void
{
	if(options & CompressionOptions.REMOVE_SWIM_SPEED_INFUSION)
	{
		let name = ItemId[code.Infusions.Helmet];
		if(name !== undefined && name.includes("Swim_Speed_Infusion")) //something something @performance
			code.Infusions.Helmet = ItemId._UNDEFINED;
	}

	if(options & CompressionOptions.REMOVE_NON_STAT_INFUSIONS)
	{
		for(let i = 0; i < ALL_INFUSION_COUNT - 1; i++) //NOTE(Rennorb): skip the amulet with - 1, as enrichments can't be moved
		{
			switch (code.Infusions[i]) {
				case ItemId.Agony_Infusion_01: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_02: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_03: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_04: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_05: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_06: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_07: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_08: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_09: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_10: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_11: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_12: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_13: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_14: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_15: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_16: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_17: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_18: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_19: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_20: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_21: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_22: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_23: code.Infusions[i] = ItemId._UNDEFINED; break;
				case ItemId.Agony_Infusion_24: code.Infusions[i] = ItemId._UNDEFINED; break;
			};
		}
	}

	if(options & CompressionOptions.SUBSTITUTE_INFUSIONS)
	{
		for(let i = 0; i < ALL_INFUSION_COUNT - 1; i++) //NOTE(Rennorb): skip the amulet with - 1, as enrichments can't be moved
		{
			let old = code.Infusions[i];
			let old_name = ItemId[old];
			if(old == ItemId._UNDEFINED || old_name === undefined) continue;

			let last_underscore_pos = old_name.lastIndexOf('_');
			if(last_underscore_pos == -1) continue;

			switch (old_name.substring(last_underscore_pos + 1)) {
				case "Concentration": code.Infusions[i] = ItemId.WvW_Infusion_Concentration; break;
				case "Malign"       : code.Infusions[i] = ItemId.WvW_Infusion_Malign;        break;
				case "Expertise"    : code.Infusions[i] = ItemId.WvW_Infusion_Expertise;     break;
				case "Healing"      : code.Infusions[i] = ItemId.WvW_Infusion_Healing;       break;
				case "Mighty"       : code.Infusions[i] = ItemId.WvW_Infusion_Mighty;        break;
				case "Power"        : code.Infusions[i] = old_name.endsWith("Healing_Power") ? ItemId.WvW_Infusion_Healing : ItemId.WvW_Infusion_Mighty; break;
				case "Precise"      :
				case "Precision"    : code.Infusions[i] = ItemId.WvW_Infusion_Precise;       break;
				case "Resilient"    :
				case "Toughness"    : code.Infusions[i] = ItemId.WvW_Infusion_Resilient;     break;
				case "Vital"        :
				case "Vitality"     : code.Infusions[i] = ItemId.WvW_Infusion_Vital;         break;
				default: 
					if(old_name.endsWith("Condition_Damage"))
						code.Infusions[i] = ItemId.WvW_Infusion_Malign;
			};
		}
	}

	if(options & CompressionOptions.REARRANGE_INFUSIONS && code.Kind != Kind.PvP)
	{
		let infusions = {};
		for(let i = 0; i < ALL_INFUSION_COUNT - 1; i++) //NOTE(Rennorb): skip the amulet with - 1, as enrichments can't be moved
		{
			let item = code.Infusions[i];
			if(item === ItemId._UNDEFINED) continue;
			infusions[item] = (infusions[item] || 0) + 1;
		}


		let remaining = 0;
		let current_inf = ItemId._UNDEFINED;
		let keys = Object.keys(infusions);
		let current_key = -1;
		function NextInfusion() : ItemId
		{
			if(remaining === 0)
			{
				current_key++;
				if(current_key < keys.length)
				{
					current_inf = parseInt(keys[current_key]);
					remaining = infusions[current_inf];
				}
				else
				{
					current_inf = ItemId._UNDEFINED;
					remaining = ALL_INFUSION_COUNT;
				}
			}

			remaining--;
			return current_inf;
		}

		if(code.Infusions.Amulet === ItemId._UNDEFINED)
		{
			for(let i = 0; i < ALL_INFUSION_COUNT - 1; i++)
				if(HasInfusionSlot(code, i))
					code.Infusions[i] = NextInfusion();
		}
		else
		{
			for(let i = ALL_INFUSION_COUNT - 1; i >= 0; i--)
				if(HasInfusionSlot(code, i))
					code.Infusions[i] = NextInfusion();
		}
	}
}
