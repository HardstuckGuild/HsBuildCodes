using Hardstuck.GuildWars2.BuildCodes.V2.Util;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public class BuildCode {
	public int                    Version;
	public Kind                   Kind;
	public Profession             Profession;
	public SpecializationChoices  Specializations;
	public AllWeapons             Weapons;
	public AllSkills              SlotSkills;
	public int?                   Rune;
	public AllEquipmentData<int>  EquipmentAttributes;
	public AllEquipmentData<int?> Infusions;
	public int?                   Food;
	public int?                   Utility;
	public ArbitraryData          ArbitraryData;
}

public enum Kind : ushort {
	UNDEFINED = 0,
	PvP       = 26 + 'p' - 'a',
	WvW       = 26 + 'w' - 'a',
	PvE       = 26 + 'o' - 'a',
}

public enum Profession : ushort {
	UNDEFINED = 0,
	GUARDIAN = 1, WARRIOR, ENGINEER, RANGER, THIEF, ELEMENTALIST, MESMER, NECROMANCER, REVENANT
}

public struct Specialization {
	public int              SpecializationIndex;
	public TraitLineChoices Choices;
}

public enum TraitLineChoice {
	NONE   = 0,
	TOP    = 1,
	MIDDLE = 2,
	BOTTOM = 3,
}

public struct WeaponSet {
	public WeaponType? MainHand;
	public WeaponType? OffHand;

	public int? Sigil1;
	public int? Sigil2;

	public bool IsSet => MainHand.HasValue || OffHand.HasValue;
}

public struct UnderwaterWeapon {
	public WeaponType Weapon;

	public int? Sigil1;
	public int? Sigil2;
}

public enum WeaponType {
	UNDEFINED = 0,
	AXE = 1, DAGGER, MACE, PISTOL, SWORD, SCEPTER, FOCUS, SHIELD, TORCH, WARHORN, SHORTBOW, 
	GREATSWORD, HAMMER, LONGBOW, RIFLE, STAFF, HARPOON_GUN, SPEAR, TRIDENT,
}

public struct ArbitraryData {
	public IProfessionArbitrary ProfessionSpecific;
	public IArbitrary           Arbitrary;
}

public interface IProfessionArbitrary {
	public class NONE : IProfessionArbitrary {
		public static readonly NONE Instance = new();
	}
}

public class RangerData : IProfessionArbitrary {
	public int? PetLand1;
	public int? PetLand2;
	public int? PetWater1;
	public int? PetWater2;
}

public class RevenantData : IProfessionArbitrary {
	public int? Legend1;
	public int? Legend2;
}

public interface IArbitrary {
	public class NONE : IArbitrary {
		public static readonly NONE Instance = new();
	}
}
