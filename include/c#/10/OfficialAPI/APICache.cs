using System.Diagnostics;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class APICache {
	/// <summary> The cache implementation to use for all operations. </summary>
	/// <remarks> If uninitialized before other functions are used this will automatically initialize to a reasonable default memory cache implementation. </remarks>
	public static ICache? CacheImpl;

	public static Task<T> Get<T>(string path, string schemaVersion = API.LATEST_SCHEMA)
	{
		if(CacheImpl == null) Interlocked.CompareExchange(ref CacheImpl, new DefaultCacheImpl(), null);
		return CacheImpl.Get<T>(path, schemaVersion);
	}

	public static async Task<WeaponType> ResolveWeaponType(int itemId)
	{
		var itemData = await Get<OfficialAPI.Item>($"/items/{itemId}");
		Debug.Assert(itemData.Type == OfficialAPI.WeaponType.Weapon, $"Item is not a weapon:\n{itemData.Id}");

		if(!Enum.TryParse(Overrides.FixWeaponTypeName(itemData.Details.Type), out WeaponType type)) type = WeaponType._UNDEFINED;
		return type;
	}

	/// <returns><see cref="StatId._UNDEFINED"/> if the item does not have stats</returns>
	/// <exception cref="InvalidOperationException">sad</exception>
	public static async Task<StatId> ResolveStatId(int itemId)
	{
		var itemData = await Get<OfficialAPI.Item>($"/items/{itemId}");
		return (StatId)(itemData.Details.InfixUpgrade?.Id ?? 0);
	}

	public static async ValueTask<TraitLineChoice> ResolvePosition(int? traitId)
	{
		if(!traitId.HasValue) return TraitLineChoice.NONE;

		var traitData = await Get<OfficialAPI.Trait>($"/traits/{traitId.Value}");
		return (TraitLineChoice)(traitData.Order + 1);
	}

	public static async ValueTask<SkillId> ResolveWeaponSkill(BuildCode code, WeaponSet effectiveWeapons, int skillIndex)
	{
		OfficialAPI.Weapon weapon;
		if(skillIndex < 3)
		{
			if(effectiveWeapons.MainHand == WeaponType._UNDEFINED) return SkillId._UNDEFINED;

			//NOTE(Rennorb): this isnt outside of the if to allow early bail if the guard condition isnt met.
			var professionData = await Get<OfficialAPI.Profession>($"/professions/{Enum.GetName(code.Profession)}");
			weapon = professionData.Weapons[Enum.GetName(effectiveWeapons.MainHand)!];
		}
		else
		{
			if(effectiveWeapons.OffHand == WeaponType._UNDEFINED
				&& (effectiveWeapons.MainHand == WeaponType._UNDEFINED || !Static.IsTwoHanded(effectiveWeapons.MainHand)))
				return SkillId._UNDEFINED;

			//NOTE(Rennorb): this isnt outside of the if to allow early bail if the guard condition isnt met.
			var professionData = await Get<OfficialAPI.Profession>($"/professions/{Enum.GetName(code.Profession)}")!;
			if(effectiveWeapons.OffHand != WeaponType._UNDEFINED)
				weapon = professionData.Weapons[Enum.GetName(effectiveWeapons.OffHand)!];
			else
				weapon = professionData.Weapons[Enum.GetName(effectiveWeapons.MainHand)!];

		}

		var skill = (code.Profession) switch {
			Profession.Thief        => weapon.Skills.Find(skill => skill.Slot == $"Weapon_{skillIndex + 1}" && (Static.IsTwoHanded(effectiveWeapons.MainHand) || skillIndex != 2 || skill.Offhand == effectiveWeapons.OffHand)),
			Profession.Elementalist => weapon.Skills.LastOrDefault(skill => skill.Slot == $"Weapon_{skillIndex + 1}" && skill.Attunement == "Fire"),
			_                       => weapon.Skills.Find(skill => skill.Slot == $"Weapon_{skillIndex + 1}"),
		};
		return (SkillId)(skill?.Id ?? 0);
	}

	/// <returns> <see cref="TraitId._UNDEFINED" />  If spec is empty </returns>
	public static async Task<TraitId> ResolveTrait(Specialization spec, TraitSlot traitSlot)
	{
		if(spec.SpecializationId == SpecializationId._UNDEFINED) return TraitId._UNDEFINED;
		var traitPos = spec.Choices[(int)traitSlot];
		if(traitPos == TraitLineChoice.NONE) return TraitId._UNDEFINED;

		var allSpecializationData = await Get<OfficialAPI.Specialization[]>("/specializations?ids=all");

		foreach(var specialization in allSpecializationData)
		{
			if(specialization.Id != (int)spec.SpecializationId) continue;

			return (TraitId)specialization.MajorTraits[(int)traitSlot * 3 + (int)traitPos - 1];
		}

		return TraitId._UNDEFINED;
	}
}
