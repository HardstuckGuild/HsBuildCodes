import { AllEquipmentInfusions, AllEquipmentStats, AllSkills, SpecializationChoices, TraitLineChoices } from "./Util/UtilStructs";

import ItemId from "./Database/ItemIds";
import SkillId from "./Database/SkillIds";
import SpecializationId from "./Database/SpecializationIds";

export class BuildCode {
	public Version             : number                = 0;
	public Kind                : Kind                  = Kind._UNDEFINED;
	public Profession          : Profession            = Profession._UNDEFINED;
	public Specializations     : SpecializationChoices = new SpecializationChoices();
	public WeaponSet1          : WeaponSet             = new WeaponSet();
	public WeaponSet2          : WeaponSet             = new WeaponSet();
	public SlotSkills          : AllSkills             = new AllSkills();
	public Rune                : ItemId                = ItemId._UNDEFINED;
	/** Note: For simplicity, pvp codes only have their amulet id set on the amulet. */
	public EquipmentAttributes : AllEquipmentStats     = new AllEquipmentStats();
	public Infusions           : AllEquipmentInfusions = new AllEquipmentInfusions();
	public Food                : ItemId                = ItemId._UNDEFINED;
	public Utility             : ItemId                = ItemId._UNDEFINED;
	public ProfessionSpecific  : IProfessionSpecific   = ProfessionSpecific.NONE.Instance;
	public Arbitrary           : IArbitrary            = Arbitrary.NONE.Instance;
}

export enum Kind {
	_UNDEFINED = 0,
	PvP        = 26 + 15, // ord('p') - ord('a');
	WvW        = 26 + 22, // ord('w') - ord('a');
	PvE        = 26 + 14, // ord('o') - ord('a');
}

//NOTE(Rennorb): names match official API
export enum Profession {
	_UNDEFINED   = 0,
	Guardian     = 1,
	Warrior      = 2,
	Engineer     = 3,
	Ranger       = 4,
	Thief        = 5,
	Elementalist = 6,
	Mesmer       = 7,
	Necromancer  = 8,
	Revenant     = 9,
}

export enum WeightClass {
	_UNDEFINED = 0,
	Light      = 1,
	Medium     = 2,
	Heavy      = 3,
}

export class Specialization {
	public SpecializationId : SpecializationId;
	public Choices          : TraitLineChoices;

	public constructor(specId : SpecializationId = SpecializationId._UNDEFINED, choices : TraitLineChoices = new TraitLineChoices()) {
		this.SpecializationId = specId;
		this.Choices          = choices;
	}
}

export enum TraitSlot {
	Adept       = 0,
	Master      = 1,
	GrandMaster = 2,
}

export enum TraitLineChoice {
	NONE   = 0,
	TOP    = 1,
	MIDDLE = 2,
	BOTTOM = 3,
}

export enum WeaponSetNumber {
	_UNDEFINED = 0,
	Set1       = 1,
	Set2       = 2,
}

/** @remarks All fields might be _UNDEFINED. Twohanded weapons only set the MainHand, OffHand must be WeaponType._UNDEFINED in that case. */
export class WeaponSet {
	public MainHand : WeaponType = WeaponType._UNDEFINED;
	public OffHand  : WeaponType = WeaponType._UNDEFINED;
	public Sigil1   : ItemId     =     ItemId._UNDEFINED;
	public Sigil2   : ItemId     =     ItemId._UNDEFINED;

	public HasAny() : boolean { return (this.MainHand | this.OffHand) != 0; }
}

//NOTE(Rennorb): names match official API
export enum WeaponType {
	_UNDEFINED =  0,
	_FIRST     =  1,
	
	Nothing    =  0, // thief weapon mapping
	Axe        =  1,
	Dagger     =  2,
	Mace       =  3,
	Pistol     =  4,
	Sword      =  5,
	Scepter    =  6,
	Focus      =  7,
	Shield     =  8,
	Torch      =  9,
	Warhorn    = 10,

	Shortbow   = 11,
	Greatsword = 12,
	Hammer     = 13,
	Longbow    = 14,
	Rifle      = 15,
	Staff      = 16,
	HarpoonGun = 17,
	Spear      = 18,
	Trident    = 19,
}

export interface IProfessionSpecific { }

export namespace ProfessionSpecific {

	export class NONE implements IProfessionSpecific {
		public static Instance = new NONE();
	}

}

export class RangerData implements IProfessionSpecific {
	/** @remarks Is PetId._UNDEFINED if the pet is not set. */
	public Pet1 : PetId = PetId._UNDEFINED;
	/** @remarks Is PetId._UNDEFINED if the pet is not set. */
	public Pet2 : PetId = PetId._UNDEFINED;
}

export enum PetId {
	_UNDEFINED = 0,
}

export class RevenantData implements IProfessionSpecific {
	public Legend1 : Legend = Legend._FIRST;
	/** @remarks Is Legend._UNDEFINED if the Legend is not set. */
	public Legend2 : Legend = Legend._UNDEFINED;

	/** @remarks Is SkillId._UNDEFINED if the second Legend is not set. */
	public AltUtilitySkill1 : SkillId = SkillId._UNDEFINED;
	/** @remarks Is SkillId._UNDEFINED if the second Legend is not set. */
	public AltUtilitySkill2 : SkillId = SkillId._UNDEFINED;
	/** @remarks Is SkillId._UNDEFINED if the second Legend is not set. */
	public AltUtilitySkill3 : SkillId = SkillId._UNDEFINED;
}

export enum Legend {
	_UNDEFINED = 0,
	_FIRST = 1,
	/** Assasin */
	SHIRO = 1,
	/** Dragon */
	GLINT = 2,
	/** Deamon */
	MALLYX = 3,
	/** Dwarf */
	JALIS = 4,
	/** Centaur */
	VENTARI = 5,
	/** Renegate */
	KALLA = 6,
	/** Alliance */
	VINDICATOR = 7,
}

export interface IArbitrary { }

export namespace Arbitrary {

	export class NONE implements IArbitrary {
		public static Instance = new NONE();
	}

}
