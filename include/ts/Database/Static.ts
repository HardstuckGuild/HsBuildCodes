import { BuildCode, Legend, Profession, Specialization, WeaponSet, WeaponSetNumber, WeaponType, WeightClass } from "../Structures";
import TextLoader from "../TextLoader";
import ItemId from "./ItemIds";
import SpecializationId from "./SpecializationIds";
import StatId from "./StatIds";

class Static {
	public static FIRST_VERSIONED_VERSION = 3;
	public static CURRENT_VERSION = 3;
	public static OFFICIAL_CHAT_CODE_BYTE_LENGTH = 44;

	public static ALL_EQUIPMENT_COUNT = 16;
	public static ALL_INFUSION_COUNT = 21;

	public static DetermineCodeVersion(code : string) : number
	{
		const vcValue = code.charCodeAt(0);
		if(vcValue > TextLoader.INVERSE_CHARSET.length) return -1;

		if(code.startsWith('v0_')) return 1;

		let potentialVersion = TextLoader.INVERSE_CHARSET[vcValue];
		// version may be lower or uppercase
		if(potentialVersion >= 26) potentialVersion -= 26;

		// NOTE(Rennorb): v1 codes start with the type indicator, which is never greater than 2. 
		// since this is also the first versioned version we can conclude tha values above the current version are invalid
		if(potentialVersion > Static.CURRENT_VERSION) return -1;

		if(potentialVersion < Static.FIRST_VERSIONED_VERSION) return 1;

		return potentialVersion;
	}

	public static IsTwoHanded(weaponType : WeaponType) : boolean
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
			case WeaponType.ShortBow:
				return false;

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
				console.error(false, "invalid weapon", weaponType);
				return false;
		}
	}

	public static IsAquatic(weaponType : WeaponType) : boolean
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
			case WeaponType.ShortBow:
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
				console.error(false, "invalid weapon", weaponType);
				return false;
		}
	}

	public static ResolveLegend(eliteSpec : Specialization, str : string|null) : Legend|null
	{ 
		switch (str) {
			case "Legend1": return Legend.GLINT;
			case "Legend2": return Legend.SHIRO;
			case "Legend3": return Legend.JALIS;
			case "Legend4": return Legend.MALLYX;
			case "Legend5": return Legend.KALLA;
			case "Legend6": return Legend.VENTARI;
			case null: if(eliteSpec.SpecializationId === SpecializationId.Vindicator) return Legend.VINDICATOR;
			default: return null;
		};
	}

	public static ResolveEffectiveWeapons(code : BuildCode, setNumber : WeaponSetNumber) : WeaponSet
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
			if(Static.IsTwoHanded(mainSet.MainHand)) {
				result.Sigil2 = mainSet.Sigil2;
				return result;
			}
		}
		else if(offSet.MainHand !== WeaponType._UNDEFINED)
		{
			if(Static.IsTwoHanded(offSet.MainHand))
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
	public static ResolveDummyItemForWeaponType(weaponType : WeaponType, statId : number) : ItemId
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
				case WeaponType.ShortBow   : return ItemId.Suuns_Short_Bow   ;
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
				case WeaponType.ShortBow   : return ItemId.Togos_Short_Bow   ;
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
				case WeaponType.ShortBow   : return ItemId.Mist_Lords_Short_Bow    ;
				case WeaponType.Staff      : return ItemId.Mist_Lords_Staff        ;
				case WeaponType.HarpoonGun : return ItemId.Harpoon_Gun_of_the_Scion;
				case WeaponType.Spear      : return ItemId.Impaler_of_the_Scion    ;
				case WeaponType.Trident    : return ItemId.Trident_of_the_Scion    ;
				default: return ItemId._UNDEFINED;
			}
		};
	}

	/** @remarks Does not translate weapon items. Use Static.ResolveDummyItemForWeaponType(WeaponType, StatId) for that. */
	public static ResolveDummyItemForEquipment(equipmentIndex : number, weightClass : WeightClass, statId : StatId) : ItemId
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
				case  6: return ItemId.Ad_Infinitum               ;
				case  7: return ItemId.Mists_Charged_Jade_Talisman;
				case  8: return ItemId.Mists_Charged_Jade_Talisman;
				case  9: return ItemId.Mists_Charged_Jade_Band    ;
				case 10: return ItemId.Mists_Charged_Jade_Band    ;
				case 15: return ItemId.Mists_Charged_Jade_Pendant ;
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
				case  6: return ItemId.Ad_Infinitum               ;
				case  7: return ItemId.Mists_Charged_Jade_Talisman;
				case  8: return ItemId.Mists_Charged_Jade_Talisman;
				case  9: return ItemId.Mists_Charged_Jade_Band    ;
				case 10: return ItemId.Mists_Charged_Jade_Band    ;
				case 15: return ItemId.Mists_Charged_Jade_Pendant ;
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
				case  6: return ItemId.Quiver_of_a_Thousand_Arrows;
				case  7: return ItemId.Black_Ice_Earing           ;
				case  8: return ItemId.Asgeirs_Talisman           ;
				case  9: return ItemId.Black_Ice_Band             ;
				case 10: return ItemId.Mistborn_Band              ;
				case 15: return ItemId.Asgeirs_Amulet             ;
			}
		};
		return ItemId._UNDEFINED;
	}

	public static ResolveWeightClass(profession : Profession) : WeightClass
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
}
export default Static;
