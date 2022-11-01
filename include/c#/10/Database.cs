using System.Diagnostics;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class Database
{
	public static readonly int CURRENT_VERSION = 2;
	public static bool IsTwoHanded(WeaponType weaponType)
	{
		switch(weaponType)
		{
			case WeaponType.AXE:
			case WeaponType.DAGGER:
			case WeaponType.MACE:
			case WeaponType.PISTOL:
			case WeaponType.SWORD:
			case WeaponType.SCEPTER:
			case WeaponType.FOCUS:
			case WeaponType.SHIELD:
			case WeaponType.TORCH:
			case WeaponType.WARHORN:
			case WeaponType.SHORTBOW:
				return false;

			case WeaponType.GREATSWORD:
			case WeaponType.HAMMER:
			case WeaponType.LONGBOW:
			case WeaponType.RIFLE:
			case WeaponType.STAFF:
			case WeaponType.HARPOON_GUN:
			case WeaponType.SPEAR:
			case WeaponType.TRIDENT:
				return true;

			default: 
				Debug.Assert(false, $"invalid weapon {weaponType}");
				return false;
		}
	}
}