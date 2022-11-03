using System.Diagnostics;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class Static
{
	public static readonly int CURRENT_VERSION = 2;
	public static readonly int OFFICIAL_CHAT_CODE_BYTE_LENGTH = 44;


	public static readonly int ALL_EQUIPMENT_COUNT = 16;
	public static readonly int ALL_INFUSION_COUNT = 21;

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
}