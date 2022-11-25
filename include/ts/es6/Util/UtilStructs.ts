import { Specialization, TraitLineChoice } from "../Structures";

import ItemId from "../Database/ItemIds";
import StatId from "../Database/StatIds";
import SkillId from "../Database/SkillIds";
import Static from "../Database/Static";

export class SpecializationChoices {
	public Choice1 : Specialization = new Specialization();
	public Choice2 : Specialization = new Specialization();
	public Choice3 : Specialization = new Specialization();

	public get [0]() { return this.Choice1; }
	public get [1]() { return this.Choice2; }
	public get [2]() { return this.Choice3; }

	public set [0](v) { this.Choice1 = v; }
	public set [1](v) { this.Choice2 = v; }
	public set [2](v) { this.Choice3 = v; }
}

export class TraitLineChoices {
	public Adept       : TraitLineChoice = TraitLineChoice.NONE;
	public Master      : TraitLineChoice = TraitLineChoice.NONE;
	public Grandmaster : TraitLineChoice = TraitLineChoice.NONE;

	public get [0]() { return this.Adept; }
	public get [1]() { return this.Master; }
	public get [2]() { return this.Grandmaster; }

	public set [0](v) { this.Adept = v; }
	public set [1](v) { this.Master = v; }
	public set [2](v) { this.Grandmaster = v; }
}

export class AllSkills {
	public Heal     : SkillId = SkillId._UNDEFINED;
	public Utility1 : SkillId = SkillId._UNDEFINED;
	public Utility2 : SkillId = SkillId._UNDEFINED;
	public Utility3 : SkillId = SkillId._UNDEFINED;
	public Elite    : SkillId = SkillId._UNDEFINED;

	public get [0]() { return this.Heal; }
	public get [1]() { return this.Utility1; }
	public get [2]() { return this.Utility2; }
	public get [3]() { return this.Utility3; }
	public get [4]() { return this.Elite; }

	public set [0](v) { this.Heal     = v; }
	public set [1](v) { this.Utility1 = v; }
	public set [2](v) { this.Utility2 = v; }
	public set [3](v) { this.Utility3 = v; }
	public set [4](v) { this.Elite    = v; }
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

	public get [ 0]() { return this.Helmet; }
	public get [ 1]() { return this.Shoulders; }
	public get [ 2]() { return this.Chest; }
	public get [ 3]() { return this.Gloves; }
	public get [ 4]() { return this.Leggings; }
	public get [ 5]() { return this.Boots; }
	public get [ 6]() { return this.BackItem; }
	public get [ 7]() { return this.Accessory1; }
	public get [ 8]() { return this.Accessory2; }
	public get [ 9]() { return this.Ring1; }
	public get [10]() { return this.Ring2; }
	public get [11]() { return this.WeaponSet1MainHand; }
	public get [12]() { return this.WeaponSet1OffHand ; }
	public get [13]() { return this.WeaponSet2MainHand; }
	public get [14]() { return this.WeaponSet2OffHand ; }
	public get [15]() { return this.Amulet; }

	public set [ 0](v) { this.Helmet             = v; }
	public set [ 1](v) { this.Shoulders          = v; }
	public set [ 2](v) { this.Chest              = v; }
	public set [ 3](v) { this.Gloves             = v; }
	public set [ 4](v) { this.Leggings           = v; }
	public set [ 5](v) { this.Boots              = v; }
	public set [ 6](v) { this.BackItem           = v; }
	public set [ 7](v) { this.Accessory1         = v; }
	public set [ 8](v) { this.Accessory2         = v; }
	public set [ 9](v) { this.Ring1              = v; }
	public set [10](v) { this.Ring2              = v; }
	public set [11](v) { this.WeaponSet1MainHand = v; }
	public set [12](v) { this.WeaponSet1OffHand  = v; }
	public set [13](v) { this.WeaponSet2MainHand = v; }
	public set [14](v) { this.WeaponSet2OffHand  = v; }
	public set [15](v) { this.Amulet             = v; }
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

	public get [ 0]() { return this.Helmet;       }
	public get [ 1]() { return this.Shoulders;    }
	public get [ 2]() { return this.Chest;        }
	public get [ 3]() { return this.Gloves;       }
	public get [ 4]() { return this.Leggings;     }
	public get [ 5]() { return this.Boots;        }
	public get [ 6]() { return this.BackItem_1;   }
	public get [ 7]() { return this.BackItem_2;   }
	public get [ 8]() { return this.Accessory1;   }
	public get [ 9]() { return this.Accessory2;   }
	public get [10]() { return this.Ring1_1;      }
	public get [11]() { return this.Ring1_2;      }
	public get [12]() { return this.Ring1_3;      }
	public get [13]() { return this.Ring2_1;      }
	public get [14]() { return this.Ring2_2;      }
	public get [15]() { return this.Ring2_3;      }
	public get [16]() { return this.WeaponSet1_1; }
	public get [17]() { return this.WeaponSet1_2; }
	public get [18]() { return this.WeaponSet2_1; }
	public get [19]() { return this.WeaponSet2_2; }
	public get [20]() { return this.Amulet;       }

	public set [ 0](v) { this.Helmet       = v; }
	public set [ 1](v) { this.Shoulders    = v; }
	public set [ 2](v) { this.Chest        = v; }
	public set [ 3](v) { this.Gloves       = v; }
	public set [ 4](v) { this.Leggings     = v; }
	public set [ 5](v) { this.Boots        = v; }
	public set [ 6](v) { this.BackItem_1   = v; }
	public set [ 7](v) { this.BackItem_2   = v; }
	public set [ 8](v) { this.Accessory1   = v; }
	public set [ 9](v) { this.Accessory2   = v; }
	public set [10](v) { this.Ring1_1      = v; }
	public set [11](v) { this.Ring1_2      = v; }
	public set [12](v) { this.Ring1_3      = v; }
	public set [13](v) { this.Ring2_1      = v; }
	public set [14](v) { this.Ring2_2      = v; }
	public set [15](v) { this.Ring2_3      = v; }
	public set [16](v) { this.WeaponSet1_1 = v; }
	public set [17](v) { this.WeaponSet1_2 = v; }
	public set [18](v) { this.WeaponSet2_1 = v; }
	public set [19](v) { this.WeaponSet2_2 = v; }
	public set [20](v) { this.Amulet       = v; }

	//NOTE(Rennorb): It isn't really optimal to use this performance wise, but its very convenient.
	public HasAny() : boolean
	{
		for(let i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			if(this[i] !== ItemId._UNDEFINED)
				return true;
		return false;
	}
}
