using System.Diagnostics;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class Static
{
	public const int FIRST_VERSIONED_VERSION = 3;
	public const int CURRENT_VERSION = 3;
	public const int OFFICIAL_CHAT_CODE_BYTE_LENGTH = 44;

	public const int ALL_EQUIPMENT_COUNT = 16;
	public const int ALL_INFUSION_COUNT = 21;

	public static int DetermineCodeVersion(ReadOnlySpan<char> code)
	{
		if(code[0] > TextLoader.INVERSE_CHARSET.Length) return -1;

		if(code.StartsWith("v0_")) return 1;

		var potentialVersion = TextLoader.INVERSE_CHARSET[code[0]];
		// version may be lower or uppercase
		if(potentialVersion >= 26) potentialVersion -= 26;

		// NOTE(Rennorb): v1 codes start with the type indicator, which is never greater than 2. 
		// since this is also the first versioned version we can conclude tha values above the current version are invalid
		if(potentialVersion > CURRENT_VERSION) return -1;

		if(potentialVersion < FIRST_VERSIONED_VERSION) return 1;

		return potentialVersion;
	}

	public static bool IsTwoHanded(WeaponType weaponType)
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
				Debug.Assert(false, $"invalid weapon {weaponType}");
				return false;
		}
	}

	public static bool IsAquatic(WeaponType weaponType)
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
				Debug.Assert(false, $"invalid weapon {weaponType}");
				return false;
		}
	}

	public static Legend? ResolveLegend(in Specialization eliteSpec, string? str) => (str) switch {
		"Legend1" => Legend.GLINT,
		"Legend2" => Legend.SHIRO,
		"Legend3" => Legend.JALIS,
		"Legend4" => Legend.MALLYX,
		"Legend5" => Legend.KALLA,
		"Legend6" => Legend.VENTARI,
		null when eliteSpec.SpecializationId == SpecializationId.Vindicator => Legend.VINDICATOR,
		_ => null,
	};

	public static WeaponSet ResolveEffectiveWeapons(BuildCode code, WeaponSetNumber setNumber)
	{
		WeaponSet mainSet, offSet;
		if(setNumber == WeaponSetNumber.Set1)
		{
			mainSet = code.WeaponSet1;
			offSet  = code.WeaponSet2;
		}
		else
		{
			mainSet = code.WeaponSet2;
			offSet  = code.WeaponSet1;
		}

		var result = new WeaponSet();

		if(mainSet.MainHand != WeaponType._UNDEFINED)
		{
			result.MainHand = mainSet.MainHand;
			result.Sigil1   = mainSet.Sigil1;
			if(IsTwoHanded(mainSet.MainHand)) {
				result.Sigil2 = mainSet.Sigil2;
				return result;
			}
		}
		else if(offSet.MainHand != WeaponType._UNDEFINED)
		{
			if(IsTwoHanded(offSet.MainHand))
			{
				if(mainSet.OffHand != WeaponType._UNDEFINED)
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

		if(mainSet.OffHand != WeaponType._UNDEFINED)
		{
			result.OffHand = mainSet.OffHand;
			result.Sigil2  = mainSet.Sigil2;
		}
		else if(offSet.OffHand != WeaponType._UNDEFINED)
		{
			result.OffHand = offSet.OffHand;
			result.Sigil2  = offSet.Sigil2;
		}

		return result;
	}

		/** @remarks Also translates _UNDEFINED */
	public static ItemId ResolveDummyItemForWeaponType(WeaponType weaponType, StatId statId)
	{
		return (weaponType) switch {
			WeaponType.Axe        => ItemId.Mist_Lords_Axe          ,
			WeaponType.Dagger     => ItemId.Mist_Lords_Dagger       ,
			WeaponType.Mace       => ItemId.Mist_Lords_Mace         ,
			WeaponType.Pistol     => ItemId.Mist_Lords_Pistol       ,
			WeaponType.Scepter    => ItemId.Mist_Lords_Scepter      ,
			WeaponType.Sword      => ItemId.Mist_Lords_Sword        ,
			WeaponType.Focus      => ItemId.Mist_Lords_Focus        ,
			WeaponType.Shield     => ItemId.Mist_Lords_Shield       ,
			WeaponType.Torch      => ItemId.Mist_Lords_Torch        ,
			WeaponType.Warhorn    => ItemId.Mist_Lords_Warhorn      ,
			WeaponType.Greatsword => ItemId.Mist_Lords_Greatsword   ,
			WeaponType.Hammer     => ItemId.Mist_Lords_Hammer       ,
			WeaponType.Longbow    => ItemId.Mist_Lords_Longbow      ,
			WeaponType.Rifle      => ItemId.Mist_Lords_Rifle        ,
			WeaponType.ShortBow   => ItemId.Mist_Lords_Short_Bow    ,
			WeaponType.Staff      => ItemId.Mist_Lords_Staff        ,
			WeaponType.HarpoonGun => ItemId.Harpoon_Gun_of_the_Scion,
			WeaponType.Spear      => ItemId.Impaler_of_the_Scion    ,
			WeaponType.Trident    => ItemId.Trident_of_the_Scion    ,
			_ => ItemId._UNDEFINED,
		};
	}

	/// <remarks> Does not translate weapon items. Use <see cref="Static.ResolveDummyItemForWeaponType(WeaponType, StatId)" /> for that. </remarks>
	public static ItemId ResolveDummyItemForEquipment(int equipmentIndex, WeightClass weightClass, StatId statId)
	{
		return (equipmentIndex) switch {
			 0 => (weightClass) switch {
				WeightClass.Light  => ItemId.Illustrious_Masque,
				WeightClass.Medium => ItemId.Illustrious_Visage,
				WeightClass.Heavy  => ItemId.Illustrious_Visor,
				_ => ItemId._UNDEFINED,
			 },
			 1 => (weightClass) switch {
				WeightClass.Light  => ItemId.Illustrious_Epaulets,
				WeightClass.Medium => ItemId.Illustrious_Shoulderguard,
				WeightClass.Heavy  => ItemId.Illustrious_Pauldrons,
				_ => ItemId._UNDEFINED,
			 },
			 2 => (weightClass) switch {
				WeightClass.Light  => ItemId.Illustrious_Doublet,
				WeightClass.Medium => ItemId.Illustrious_Guise,
				WeightClass.Heavy  => ItemId.Illustrious_Breastplate,
				_ => ItemId._UNDEFINED,
			 },
			 3 => (weightClass) switch {
				WeightClass.Light  => ItemId.Illustrious_Wristguards,
				WeightClass.Medium => ItemId.Illustrious_Grips,
				WeightClass.Heavy  => ItemId.Illustrious_Warfists,
				_ => ItemId._UNDEFINED,
			 },
			 4 => (weightClass) switch {
				WeightClass.Light  => ItemId.Illustrious_Breeches,
				WeightClass.Medium => ItemId.Illustrious_Leggings,
				WeightClass.Heavy  => ItemId.Illustrious_Tassets,
				_ => ItemId._UNDEFINED,
			 },
			 5 => (weightClass) switch {
				WeightClass.Light  => ItemId.Illustrious_Footwear,
				WeightClass.Medium => ItemId.Illustrious_Striders,
				WeightClass.Heavy  => ItemId.Illustrious_Greaves,
				_ => ItemId._UNDEFINED,
			 },
			 6 => ItemId.Quiver_of_a_Thousand_Arrows,
			 7 => ItemId.Black_Ice_Earing           ,
			 8 => ItemId.Asgeirs_Talisman           ,
			 9 => ItemId.Black_Ice_Band             ,
			10 => ItemId.Mistborn_Band              ,
			15 => ItemId.Asgeirs_Amulet             ,
			_ => ItemId._UNDEFINED,
		};
	}

	public static WeightClass ResolveWeightClass(Profession profession) 
	{
		return (profession) switch {
			Profession.Guardian     => WeightClass.Heavy,
			Profession.Warrior      => WeightClass.Heavy,
			Profession.Engineer     => WeightClass.Medium,
			Profession.Ranger       => WeightClass.Medium,
			Profession.Thief        => WeightClass.Medium,
			Profession.Elementalist => WeightClass.Light,
			Profession.Mesmer       => WeightClass.Light,
			Profession.Necromancer  => WeightClass.Light,
			Profession.Revenant     => WeightClass.Heavy,
			_ => WeightClass._UNDEFINED,
		};
	}
}