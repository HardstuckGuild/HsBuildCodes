using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Runtime.InteropServices;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public class BuildCode {
	public int                    Version;
	public Kind                   Kind;
	public Profession             Profession;
	public SpecializationChoices  Specializations;
	public WeaponSet              WeaponSet1;
	public WeaponSet              WeaponSet2;
	public AllSkills              SlotSkills;
	public int?                   Rune;
	/// <summary> Note: for simplicity, pvp codes only have their amulet id set on the amulet </summary> //TODO @nocommit
	public AllEquipmentStats      EquipmentAttributes;
	public AllEquipmentInfusions  Infusions;
	public int?                   Food;
	public int?                   Utility;
	public IProfessionSpecific    ProfessionSpecific = IProfessionSpecific.NONE.Instance;
	public IArbitrary             Arbitrary          = IArbitrary         .NONE.Instance;
}

public enum Kind : ushort {
	_UNDEFINED = default,
	PvP       = 26 + 'p' - 'a',
	WvW       = 26 + 'w' - 'a',
	PvE       = 26 + 'o' - 'a',
}

//NOTE(Rennorb): names match official API
public enum Profession {
	_UNDEFINED = default,
	Guardian = 1, Warrior, Engineer, Ranger, Thief, Elementalist, Mesmer, Necromancer, Revenant,
	_FIRST = Guardian,
}

public struct Specialization {
	public SpecializationId SpecializationId;
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

	public bool HasAny => MainHand.HasValue || OffHand.HasValue;
}

public struct UnderwaterWeapon {
	public WeaponType Weapon;

	public int? Sigil1;
	public int? Sigil2;
}

//NOTE(Rennorb): names match official API
public enum WeaponType {
	_UNDEFINED = default,
	Axe = 1, Dagger, Mace, Pistol, Sword, Scepter, Focus, Shield, Torch, Warhorn, ShortBow, 
	Greatsword, Hammer, Longbow, Rifle, Staff, HarpoonGun, Spear, Trident,
	_FIRST = Axe,
}

public interface IProfessionSpecific {
	public class NONE : IProfessionSpecific {
		public static readonly NONE Instance = new();
	}
}

public class RangerData : IProfessionSpecific {
	public int? Pet1;
	public int? Pet2;
}

public class RevenantData : IProfessionSpecific {
	public Legend Legend1;
	public Legend? Legend2;

	public SkillId? AltUtilitySkill1;
	public SkillId? AltUtilitySkill2;
	public SkillId? AltUtilitySkill3;
}

public enum Legend {
	_UNDEFINED = 0,
	/// <summary> Assasin </summary>
	SHIRO = 1,
	/// <summary> Dragon </summary>
	GLINT,
	/// <summary> Deamon </summary>
	MALLYX,
	/// <summary> Dwarf </summary>
	JALIS,
	/// <summary> Centaur </summary>
	VENTARI,
	/// <summary> Renegate </summary>
	KALLA,
	/// <summary> Alliance </summary>
	VINDICATOR,
	_FIRST = SHIRO,
}

public interface IArbitrary {
	public class NONE : IArbitrary {
		public static readonly NONE Instance = new();
	}
}


/// <summary> Unused. be weary of endianess </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 44)]
unsafe struct OfficialBuildCode {
	public       byte   Type;
	public       byte   Profession;
	public fixed ushort Specializations[3];
	public fixed ushort SkillPalletteIds[10];
	public ProfessionSpecificSegment ProfessionSpecific;

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct ProfessionSpecificSegment {
		[FieldOffset(0)]
		public RangerSegment   Ranger;
		[FieldOffset(0)]
		public RevenantSegment Revenant;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct RangerSegment {
			public byte LandPet1;
			public byte LandPet2;
			public byte WaterPet1;
			public byte WaterPet2;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct RevenantSegment {
			public byte LandLegend1;
			public byte LandLegend2;
			public byte WaterLegend1;
			public byte WaterLegend2;

			public fixed ushort LandInactiveUtilitySkills[3];
			public fixed ushort WaterInactiveUtilitySkills[3];
		}
	}
}