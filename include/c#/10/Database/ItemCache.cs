using Gw2Sharp.WebApi.Caching;
using System.Diagnostics;
using Gw2Sharp.WebApi.V2.Models;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class ItemCache {
	static readonly Gw2Sharp.Connection _connection = new(null, default, cacheMethod: new MemoryCacheMethod(30 * 60 * 1000));
	static readonly Gw2Sharp.Gw2Client  _client = new(_connection);

	public static WeaponType ResolveWeaponType(int itemId)
	{
		var itemData = _client.WebApi.V2.Items.GetAsync(itemId).Result;
		Debug.Assert(itemData.Type.Value == ItemType.Weapon, $"Item is not a weapon:\n{itemData}");
		var weaponData = (ItemWeapon)itemData;
		return weaponData.Details.Type.IsUnknown ? WeaponType._UNDEFINED : Enum.Parse<WeaponType>(weaponData.Details.Type.RawValue!);
	}

	public static int ResolveStatId(int itemId)
	{
		var itemData = _client.WebApi.V2.Items.GetAsync(itemId).Result;
		return (itemData) switch {
			ItemWeapon   weaponData =>  weaponData.Details.InfixUpgrade?.Id ?? 0,
			ItemArmor     armorData =>   armorData.Details.InfixUpgrade?.Id ?? 0,
			ItemTrinket trinketData => trinketData.Details.InfixUpgrade?.Id ?? 0,
			ItemBack       backData =>    backData.Details.InfixUpgrade?.Id ?? 0,
			_ => throw new InvalidOperationException($"the provided itemId des not correspond to an item with stats: {itemData}"),
		};
	}
}
