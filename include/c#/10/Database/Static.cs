using System.Diagnostics;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class Static
{
	public const int CURRENT_VERSION = 2;
	public const int OFFICIAL_CHAT_CODE_BYTE_LENGTH = 44;

	public const int ALL_EQUIPMENT_COUNT = 16;
	public const int ALL_INFUSION_COUNT = 21;

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

	public static Legend? ResolveLegend(in Specialization? eliteSpec, string? str) => (str) switch {
		"Legend1" => Legend.GLINT,
		"Legend2" => Legend.SHIRO,
		"Legend3" => Legend.JALIS,
		"Legend4" => Legend.MALLYX,
		"Legend5" => Legend.KALLA,
		"Legend6" => Legend.VENTARI,
		null when eliteSpec?.SpecializationId == SpecializationId.Vindicator => Legend.VINDICATOR,
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
}