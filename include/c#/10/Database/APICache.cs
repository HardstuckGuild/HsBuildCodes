using Gw2Sharp.WebApi.Caching;
using System.Diagnostics;
using Gw2Sharp.WebApi.V2.Models;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class APICache {
	static readonly Gw2Sharp.Connection _connection = new(null, default, cacheMethod: new MemoryCacheMethod(30 * 60 * 1000));
	static readonly Gw2Sharp.Gw2Client  _client = new(_connection);

	public static async Task<WeaponType> ResolveWeaponType(int itemId)
	{
		var itemData = await _client.WebApi.V2.Items.GetAsync(itemId);
		Debug.Assert(itemData.Type.Value == ItemType.Weapon, $"Item is not a weapon:\n{itemData}");

		var weaponData = (ItemWeapon)itemData;
		return weaponData.Details.Type.IsUnknown ? WeaponType._UNDEFINED : Enum.Parse<WeaponType>(weaponData.Details.Type.RawValue!);
	}

	/// <returns><see cref="StatId._UNDEFINED"/> if the item does not have stats</returns>
	/// <exception cref="InvalidOperationException">sad</exception>
	public static async Task<StatId> ResolveStatId(int itemId)
	{
		var itemData = await _client.WebApi.V2.Items.GetAsync(itemId);
		return (StatId)((itemData) switch {
			ItemWeapon   weaponData =>  weaponData.Details.InfixUpgrade?.Id ?? 0,
			ItemArmor     armorData =>   armorData.Details.InfixUpgrade?.Id ?? 0,
			ItemTrinket trinketData => trinketData.Details.InfixUpgrade?.Id ?? 0,
			ItemBack       backData =>    backData.Details.InfixUpgrade?.Id ?? 0,
			_ => 0,
		});
	}

	public static async ValueTask<TraitLineChoice> ResolvePosition(int? traitId)
	{
		if(!traitId.HasValue) return TraitLineChoice.NONE;

		var traitData = await _client.WebApi.V2.Traits.GetAsync(traitId.Value);
		return (TraitLineChoice)(traitData.Order + 1);
	}

	public static async Task<Skill> ResolveSkillInfo(SkillId skillId)
	{
		// palce to do overrides
		return await _client.WebApi.V2.Skills.GetAsync((int)skillId);
	}

	public static async ValueTask<SkillId> ResolveWeaponSkill(BuildCode code, WeaponSet effectiveWeapons, int skillIndex)
	{
		ProfessionWeapon weapon;
		if(skillIndex < 3)
		{
			if(effectiveWeapons.MainHand == WeaponType._UNDEFINED) return SkillId._UNDEFINED;

			//NOTE(Rennorb): this isnt outside of the if to allow early bail if the guard condition isnt met.
			var professionData = await _client.WebApi.V2.Professions.GetAsync(Enum.GetName(code.Profession)!);
			weapon = professionData.Weapons[Enum.GetName(effectiveWeapons.MainHand)!];
		}
		else
		{
			if(effectiveWeapons.OffHand == WeaponType._UNDEFINED && !Static.IsTwoHanded(effectiveWeapons.MainHand))
				return SkillId._UNDEFINED;

			//NOTE(Rennorb): this isnt outside of the if to allow early bail if the guard condition isnt met.
			var professionData = await _client.WebApi.V2.Professions.GetAsync(Enum.GetName(code.Profession)!);
			if(effectiveWeapons.OffHand != WeaponType._UNDEFINED)
				weapon = professionData.Weapons[Enum.GetName(effectiveWeapons.OffHand)!];
			else
				weapon = professionData.Weapons[Enum.GetName(effectiveWeapons.MainHand)!];

		}

		return (SkillId)weapon.Skills.First(w => w.Slot.Value == SkillSlot.Weapon1 + skillIndex).Id;
	}
}
