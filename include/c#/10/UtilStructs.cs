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

public static class AllEquipmentData {
	public static readonly int ALL_EQUIPMENT_COUNT = 16;
}
public struct AllEquipmentData<T> {
	public T  Helmet;
	public T  Shoulders;
	public T  Chest;
	public T  Gloves;
	public T  Leggings;
	public T  Boots;
	public T  BackItem;
	public T  Accessory1;
	public T  Accessory2;
	public T  Ring1;
	public T  Ring2;
	public T? WeaponLandSet1MainHand;
	public T? WeaponLandSet1OffHand;
	public T? WeaponLandSet2MainHand;
	public T? WeaponLandSet2OffHand;
	public T  Amulet;

	public T this[int index] {
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
			11 => this.WeaponLandSet1MainHand!,
			12 => this.WeaponLandSet1OffHand !,
			13 => this.WeaponLandSet2MainHand!,
			14 => this.WeaponLandSet2OffHand !,
			15 => this.Amulet,
			_ => throw new ArgumentOutOfRangeException(nameof(index)),
		};
		set {
			switch(index) {
				case  0: this.Helmet                 = value; break;
				case  1: this.Shoulders              = value; break;
				case  2: this.Chest                  = value; break;
				case  3: this.Gloves                 = value; break;
				case  4: this.Leggings               = value; break;
				case  5: this.Boots                  = value; break;
				case  6: this.BackItem               = value; break;
				case  7: this.Accessory1             = value; break;
				case  8: this.Accessory2             = value; break;
				case  9: this.Ring1                  = value; break;
				case 10: this.Ring2                  = value; break;
				case 11: this.WeaponLandSet1MainHand = value; break;
				case 12: this.WeaponLandSet1OffHand  = value; break;
				case 13: this.WeaponLandSet2MainHand = value; break;
				case 14: this.WeaponLandSet2OffHand  = value; break;
				case 15: this.Amulet                 = value; break;
				default: throw new ArgumentOutOfRangeException(nameof(index));
			};
		}
	}
}