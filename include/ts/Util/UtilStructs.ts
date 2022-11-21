import { Specialization, TraitLineChoice } from "../Structures";

import ItemId from "../Database/ItemIds";
import StatId from "../Database/StatIds";
import SkillId from "../Database/SkillIds";

export class SpecializationChoices {
	public Choice1 : Specialization = new Specialization();
	public Choice2 : Specialization = new Specialization();
	public Choice3 : Specialization = new Specialization();

	public [0] = this.Choice1;
	public [1] = this.Choice2;
	public [2] = this.Choice3;
}

export class TraitLineChoices {
	public Adept       : TraitLineChoice = TraitLineChoice.NONE;
	public Master      : TraitLineChoice = TraitLineChoice.NONE;
	public Grandmaster : TraitLineChoice = TraitLineChoice.NONE;

	public [0] = this.Adept;
	public [1] = this.Master;
	public [2] = this.Grandmaster;
}

export class AllSkills {
	public Heal     : SkillId = SkillId._UNDEFINED;
	public Utility1 : SkillId = SkillId._UNDEFINED;
	public Utility2 : SkillId = SkillId._UNDEFINED;
	public Utility3 : SkillId = SkillId._UNDEFINED;
	public Elite    : SkillId = SkillId._UNDEFINED;

	public [0] = this.Heal;
	public [1] = this.Utility1;
	public [2] = this.Utility2;
	public [3] = this.Utility3;
	public [4] = this.Elite;
}

export class AllEquipmentStats {
	public Helmet             : StatId = StatId._UNDEFINED;
	public Shoulders          : StatId = StatId._UNDEFINED;
	public Chest              : StatId = StatId._UNDEFINED;
	public Gloves             : StatId = StatId._UNDEFINED;
	public Leggings           : StatId = StatId._UNDEFINED;
	public Boots              : StatId = StatId._UNDEFINED;
	public BackItem           : StatId = StatId._UNDEFINED;
	public Accessory1         : StatId = StatId._UNDEFINED;
	public Accessory2         : StatId = StatId._UNDEFINED;
	public Ring1              : StatId = StatId._UNDEFINED;
	public Ring2              : StatId = StatId._UNDEFINED;
	/** @remarks Is StatId._UNDEFINED if the weapon is not set. */
	public WeaponSet1MainHand : StatId = StatId._UNDEFINED;
	/** @remarks Is StatId._UNDEFINED if the weapon is not set. */
	public WeaponSet1OffHand  : StatId = StatId._UNDEFINED;
	/** @remarks Is StatId._UNDEFINED if the weapon is not set. */
	public WeaponSet2MainHand : StatId = StatId._UNDEFINED;
	/** @remarks Is StatId._UNDEFINED if the weapon is not set. */
	public WeaponSet2OffHand  : StatId = StatId._UNDEFINED;
	public Amulet             : StatId = StatId._UNDEFINED;

	public [ 0] = this.Helmet;
	public [ 1] = this.Shoulders;
	public [ 2] = this.Chest;
	public [ 3] = this.Gloves;
	public [ 4] = this.Leggings;
	public [ 5] = this.Boots;
	public [ 6] = this.BackItem;
	public [ 7] = this.Accessory1;
	public [ 8] = this.Accessory2;
	public [ 9] = this.Ring1;
	public [10] = this.Ring2;
	public [11] = this.WeaponSet1MainHand;
	public [12] = this.WeaponSet1OffHand ;
	public [13] = this.WeaponSet2MainHand;
	public [14] = this.WeaponSet2OffHand ;
	public [15] = this.Amulet;
}

export class AllEquipmentInfusions {
	public Helmet       : ItemId = ItemId._UNDEFINED;
	public Shoulders    : ItemId = ItemId._UNDEFINED;
	public Chest        : ItemId = ItemId._UNDEFINED;
	public Gloves       : ItemId = ItemId._UNDEFINED;
	public Leggings     : ItemId = ItemId._UNDEFINED;
	public Boots        : ItemId = ItemId._UNDEFINED;
	public BackItem_1   : ItemId = ItemId._UNDEFINED;
	public BackItem_2   : ItemId = ItemId._UNDEFINED;
	public Accessory1   : ItemId = ItemId._UNDEFINED;
	public Accessory2   : ItemId = ItemId._UNDEFINED;
	public Ring1_1      : ItemId = ItemId._UNDEFINED;
	public Ring1_2      : ItemId = ItemId._UNDEFINED;
	public Ring1_3      : ItemId = ItemId._UNDEFINED;
	public Ring2_1      : ItemId = ItemId._UNDEFINED;
	public Ring2_2      : ItemId = ItemId._UNDEFINED;
	public Ring2_3      : ItemId = ItemId._UNDEFINED;
	public WeaponSet1_1 : ItemId = ItemId._UNDEFINED;
	public WeaponSet1_2 : ItemId = ItemId._UNDEFINED;
	public WeaponSet2_1 : ItemId = ItemId._UNDEFINED;
	public WeaponSet2_2 : ItemId = ItemId._UNDEFINED;
	public Amulet       : ItemId = ItemId._UNDEFINED;

	public [ 0] = this.Helmet;
	public [ 1] = this.Shoulders;
	public [ 2] = this.Chest;
	public [ 3] = this.Gloves;
	public [ 4] = this.Leggings;
	public [ 5] = this.Boots;
	public [ 6] = this.BackItem_1;
	public [ 7] = this.BackItem_2;
	public [ 8] = this.Accessory1;
	public [ 9] = this.Accessory2;
	public [10] = this.Ring1_1;
	public [11] = this.Ring1_2;
	public [12] = this.Ring1_3;
	public [13] = this.Ring2_1;
	public [14] = this.Ring2_2;
	public [15] = this.Ring2_3;
	public [16] = this.WeaponSet1_1;
	public [17] = this.WeaponSet1_2;
	public [18] = this.WeaponSet2_1;
	public [19] = this.WeaponSet2_2;
	public [20] = this.Amulet;

	//NOTE(Rennorb): It isn't really optimal to use this performance wise, but its very convenient.
	public HasAny() : boolean
	{
		for(let i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			if(this[i] !== ItemId._UNDEFINED)
				return true;
		return false;
	}
}
