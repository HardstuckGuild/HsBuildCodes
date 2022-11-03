namespace Hardstuck.GuildWars2.BuildCodes.V2.Util;

public struct SpecializationChoices {
	public Specialization? Choice1;
	public Specialization? Choice2;
	public Specialization? Choice3;

	public Specialization? this[int index] {
		get => (index) switch {
			0 => this.Choice1,
			1 => this.Choice2,
			2 => this.Choice3,
			_ => throw new ArgumentOutOfRangeException(nameof(index)),
		};
		set {
			switch(index) {
				case 0: this.Choice1 = value; break;
				case 1: this.Choice2 = value; break;
				case 2: this.Choice3 = value; break;
				default: throw new ArgumentOutOfRangeException(nameof(index));
			};
		}
	}
}

public struct TraitLineChoices {
	public TraitLineChoice Adept;
	public TraitLineChoice Master;
	public TraitLineChoice Grandmaster;

	public TraitLineChoice this[int index] {
		get => (index) switch {
			0 => this.Adept,
			1 => this.Master,
			2 => this.Grandmaster,
			_ => throw new ArgumentOutOfRangeException(nameof(index)),
		};
		set {
				switch(index) {
				case 0: this.Adept       = value; break;
				case 1: this.Master      = value; break;
				case 2: this.Grandmaster = value; break;
				default: throw new ArgumentOutOfRangeException(nameof(index));
			};
		}
	}
}

public struct AllWeapons {
	public WeaponSet Set1;
	public WeaponSet Set2;
}

public struct AllSkills {
	public SkillId? Heal;
	public SkillId? Utility1;
	public SkillId? Utility2;
	public SkillId? Utility3;
	public SkillId? Elite;

	public SkillId? this[int index] {
		get => (index) switch {
			0 => this.Heal,
			1 => this.Utility1,
			2 => this.Utility2,
			3 => this.Utility3,
			4 => this.Elite,
			_ => throw new ArgumentOutOfRangeException(nameof(index)),
		};
		set {
			switch(index) {
				case 0: this.Heal     = value; break;
				case 1: this.Utility1 = value; break;
				case 2: this.Utility2 = value; break;
				case 3: this.Utility3 = value; break;
				case 4: this.Elite    = value; break;
				default: throw new ArgumentOutOfRangeException(nameof(index));
			};
		}
	}
}

public struct AllEquipmentStats {
	public StatId  Helmet;
	public StatId  Shoulders;
	public StatId  Chest;
	public StatId  Gloves;
	public StatId  Leggings;
	public StatId  Boots;
	public StatId  BackItem;
	public StatId  Accessory1;
	public StatId  Accessory2;
	public StatId  Ring1;
	public StatId  Ring2;
	public StatId? WeaponSet1MainHand;
	public StatId? WeaponSet1OffHand;
	public StatId? WeaponSet2MainHand;
	public StatId? WeaponSet2OffHand;
	public StatId  Amulet;

	public StatId? this[int index] {
		get => (index) switch {
			 0 => this.Helmet,
			 1 => this.Shoulders,
			 2 => this.Chest,
			 3 => this.Gloves,
			 4 => this.Leggings,
			 5 => this.Boots,
			 6 => this.BackItem,
			 7 => this.Accessory1,
			 8 => this.Accessory2,
			 9 => this.Ring1,
			10 => this.Ring2,
			11 => this.WeaponSet1MainHand,
			12 => this.WeaponSet1OffHand ,
			13 => this.WeaponSet2MainHand,
			14 => this.WeaponSet2OffHand ,
			15 => this.Amulet,
			_ => throw new ArgumentOutOfRangeException(nameof(index)),
		};
		set {
			switch(index) {
				case  0: this.Helmet             = value ?? 0; break;
				case  1: this.Shoulders          = value ?? 0; break;
				case  2: this.Chest              = value ?? 0; break;
				case  3: this.Gloves             = value ?? 0; break;
				case  4: this.Leggings           = value ?? 0; break;
				case  5: this.Boots              = value ?? 0; break;
				case  6: this.BackItem           = value ?? 0; break;
				case  7: this.Accessory1         = value ?? 0; break;
				case  8: this.Accessory2         = value ?? 0; break;
				case  9: this.Ring1              = value ?? 0; break;
				case 10: this.Ring2              = value ?? 0; break;
				case 11: this.WeaponSet1MainHand = value; break;
				case 12: this.WeaponSet1OffHand  = value; break;
				case 13: this.WeaponSet2MainHand = value; break;
				case 14: this.WeaponSet2OffHand  = value; break;
				case 15: this.Amulet             = value ?? 0; break;
				default: throw new ArgumentOutOfRangeException(nameof(index));
			};
		}
	}
}

public struct AllEquipmentInfusions {
	public int? Helmet;
	public int? Shoulders;
	public int? Chest;
	public int? Gloves;
	public int? Leggings;
	public int? Boots;
	public int? BackItem_1;
	public int? BackItem_2;
	public int? Accessory1;
	public int? Accessory2;
	public int? Ring1_1;
	public int? Ring1_2;
	public int? Ring1_3;
	public int? Ring2_1;
	public int? Ring2_2;
	public int? Ring2_3;
	public int? WeaponSet1_1;
	public int? WeaponSet1_2;
	public int? WeaponSet2_1;
	public int? WeaponSet2_2;
	public int? Amulet;

	public int? this[int index]
	{
		get => (index) switch {
			 0 => this.Helmet,
			 1 => this.Shoulders,
			 2 => this.Chest,
			 3 => this.Gloves,
			 4 => this.Leggings,
			 5 => this.Boots,
			 6 => this.BackItem_1,
			 7 => this.BackItem_2,
			 8 => this.Accessory1,
			 9 => this.Accessory2,
			10 => this.Ring1_1,
			11 => this.Ring1_2,
			12 => this.Ring1_3,
			13 => this.Ring2_1,
			14 => this.Ring2_2,
			15 => this.Ring2_3,
			16 => this.WeaponSet1_1,
			17 => this.WeaponSet1_2,
			18 => this.WeaponSet2_1,
			19 => this.WeaponSet2_2,
			20 => this.Amulet,
			_ => throw new ArgumentOutOfRangeException(nameof(index)),
		};

		set {
			switch(index)
			{
				case  0: this.Helmet       = value; break;
				case  1: this.Shoulders    = value; break;
				case  2: this.Chest        = value; break;
				case  3: this.Gloves       = value; break;
				case  4: this.Leggings     = value; break;
				case  5: this.Boots        = value; break;
				case  6: this.BackItem_1   = value; break;
				case  7: this.BackItem_2   = value; break;
				case  8: this.Accessory1   = value; break;
				case  9: this.Accessory2   = value; break;
				case 10: this.Ring1_1      = value; break;
				case 11: this.Ring1_2      = value; break;
				case 12: this.Ring1_3      = value; break;
				case 13: this.Ring2_1      = value; break;
				case 14: this.Ring2_2      = value; break;
				case 15: this.Ring2_3      = value; break;
				case 16: this.WeaponSet1_1 = value; break;
				case 17: this.WeaponSet1_2 = value; break;
				case 18: this.WeaponSet2_1 = value; break;
				case 19: this.WeaponSet2_2 = value; break;
				case 20: this.Amulet       = value; break;
				default: throw new ArgumentOutOfRangeException(nameof(index));
			};
		}
	}
}