using System.Diagnostics;
using Hardstuck.GuildWars2.MumbleLink;
using static Hardstuck.GuildWars2.BuildCodes.V2.Static;
using Permission = Hardstuck.GuildWars2.BuildCodes.V2.OfficialAPI.Permission;
using EquipmentItemSlot = Hardstuck.GuildWars2.BuildCodes.V2.OfficialAPI.EquipmentItemSlot;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class APILoader {

	/// <summary> Produces a list of token scopes that are required for the various methods but missing. </summary>
	public static async Task<IEnumerable<string>> ValidateScopes(string authToken)
	{
		var tokenInfo = await API.RequestJson<OfficialAPI.TokenInfo>("/tokeninfo", authToken);
		Debug.Assert(tokenInfo != null);

		var required = new[] { Permission.Account, Permission.Characters, Permission.Builds };
		return required.Except(tokenInfo.Permissions);
	}

	static readonly MumbleReader _mumble = new(false);

	/// <summary> This method assumes the scopes account, character and build are available, but does not explicitely test for them. </summary>
	/// <exception cref="Gw2Sharp.WebApi.Exceptions.NotFoundException">If the character can't be found. Usually happens if the api key doenst actaully correspond to the logged in account.</exception>
	/// <exception cref="Gw2Sharp.WebApi.Exceptions.MissingScopesException"></exception>
	/// <exception cref="Gw2Sharp.WebApi.Exceptions.InvalidAccessTokenException">If the token is not valid.</exception>
	/// <returns> Null if the characer can't be determined. </returns>
	public static async ValueTask<BuildCode?> LoadBuildCodeFromCurrentCharacter(string authToken, bool aquatic = false)
	{
		_mumble.Update();

		if(string.IsNullOrEmpty(_mumble.Data.Identity?.Name)) return null;

		var gamemode = Kind.PvE;
		switch(_mumble.Data.Context.MapType)
		{
			case MapType.PvP:
			case MapType.GvG:
			case MapType.Tournament:
			case MapType.UserTournament:
				gamemode = Kind.PvP;
				break;

			case MapType.WvW_EBG:
			case MapType.WvW_BBL:
			case MapType.WvW_GBL:
			case MapType.WvW_RBL:
			case MapType.WvW_OS:
			case MapType.WvW_EotM:
			case MapType.WvW_Lounge:
				gamemode = Kind.WvW;
				break;
		}
		return await LoadBuildCode(authToken, _mumble.Data.Identity.Name, gamemode, aquatic);
	}

	/// <summary> This method assumes the scopes account, character and build are available, but does not explicitely test for them. </summary>
	/// <exception cref="Gw2Sharp.WebApi.Exceptions.NotFoundException">If the character can't be found.</exception>
	/// <exception cref="Gw2Sharp.WebApi.Exceptions.MissingScopesException"></exception>
	/// <exception cref="Gw2Sharp.WebApi.Exceptions.InvalidAccessTokenException">If the token is not valid.</exception>
	public static async Task<BuildCode> LoadBuildCode(string authToken, string characterName, Kind targetGameMode, bool aquatic = false) {
		var code = new BuildCode();
		code.Version = CURRENT_VERSION;
		code.Kind    = targetGameMode;

		var playerData = await API.RequestJson<OfficialAPI.Character>($"/characters/{characterName}", authToken);
		
		code.Profession = Enum.Parse<Profession>(playerData.Profession);

		var activeBuild = playerData.BuildTabs.First(t => t.IsActive).Build;
		for(var i = 0; i < 3; i++) {
			var spec = activeBuild.Specializations[i];
			if(!spec.Id.HasValue) continue;

			code.Specializations[i] = new() {
				SpecializationId = (SpecializationId)spec.Id.Value,
				Choices          = {
					Adept       = await APICache.ResolvePosition(spec.Traits![0]),
					Master      = await APICache.ResolvePosition(spec.Traits![1]),
					Grandmaster = await APICache.ResolvePosition(spec.Traits![2]),
				},
			};
		}

		var activeEquipment = playerData.EquipmentTabs.First(t => t.IsActive);
		if(targetGameMode != Kind.PvP)
		{
			ItemId? runeId = null;

			async void SetArmorData(int equipSlot, OfficialAPI.EquipmentItem item)
			{
				code.EquipmentAttributes[equipSlot] = await ResolveStatId(item);
				code.Infusions          [equipSlot] = (ItemId?)item.Infusions?[0] ?? ItemId._UNDEFINED;
				if(item.Upgrades != null) {
					if(runeId == null) runeId = (ItemId)item.Upgrades[0];
					else if(runeId != (ItemId)item.Upgrades[0]) runeId = ItemId._UNDEFINED;
				}
			}

			foreach(var item in activeEquipment.Equipment!) {
				switch(item.Slot)
				{
					case EquipmentItemSlot.Helm       : if( aquatic) break; SetArmorData(0, item); break;
					case EquipmentItemSlot.HelmAquatic: if(!aquatic) break; SetArmorData(0, item); break;
					case EquipmentItemSlot.Shoulders  :                     SetArmorData(1, item); break;
					case EquipmentItemSlot.Coat       :                     SetArmorData(2, item); break;
					case EquipmentItemSlot.Gloves     :                     SetArmorData(3, item); break;
					case EquipmentItemSlot.Leggings   :                     SetArmorData(4, item); break;
					case EquipmentItemSlot.Boots      :                     SetArmorData(5, item); break;
						
					case EquipmentItemSlot.Backpack:
						code.EquipmentAttributes.BackItem = await ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.BackItem_1 = (ItemId)item.Infusions[0];
							if(item.Infusions.Count > 1)
								code.Infusions.BackItem_1 = (ItemId)item.Infusions[1];
						}
						break;

					case EquipmentItemSlot.Accessory1:
						code.EquipmentAttributes.Accessory1 = await ResolveStatId(item);
						code.Infusions          .Accessory1 = (ItemId?)item.Infusions?[0] ?? ItemId._UNDEFINED;
						break;

					case EquipmentItemSlot.Accessory2:
						code.EquipmentAttributes.Accessory2 = await ResolveStatId(item);
						code.Infusions          .Accessory2 = (ItemId?)item.Infusions?[0] ?? ItemId._UNDEFINED;
						break;

					case EquipmentItemSlot.Ring1:
						code.EquipmentAttributes.Ring1 = await ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.Ring1_1 = (ItemId)item.Infusions[0];
							if(item.Infusions.Count > 1) {
								code.Infusions.Ring1_2 = (ItemId)item.Infusions[1];
								if(item.Infusions.Count > 2)
									code.Infusions.Ring1_3 = (ItemId)item.Infusions[2];
							}
						}
						break;
						
					case EquipmentItemSlot.Ring2:
						code.EquipmentAttributes.Ring2 = await ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.Ring2_1 = (ItemId)item.Infusions[0];
							if(item.Infusions.Count > 1) {
								code.Infusions.Ring2_2 = (ItemId)item.Infusions[1];
								if(item.Infusions.Count > 2)
									code.Infusions.Ring2_3 = (ItemId)item.Infusions[2];
							}
						}
						break;
						
					case EquipmentItemSlot.WeaponA1:
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet1MainHand = await ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.WeaponSet1_1 = (ItemId)item.Infusions[0];
							if(item.Infusions.Count > 1)
								code.Infusions.WeaponSet1_2 = (ItemId)item.Infusions[1];
						}
						code.WeaponSet1.MainHand = await APICache.ResolveWeaponType(item.Id);
						if(item.Upgrades != null) {
							code.WeaponSet1.Sigil1 = (ItemId)item.Upgrades[0];
							if(item.Upgrades.Count > 1)
								code.WeaponSet1.Sigil2 = (ItemId)item.Upgrades[1];
						}
						break;

					case EquipmentItemSlot.WeaponAquaticA:
						if(!aquatic) break;
						code.EquipmentAttributes.WeaponSet1MainHand = await ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.WeaponSet1_1 = (ItemId)item.Infusions[0];
							if(item.Infusions.Count > 1)
								code.Infusions.WeaponSet1_2 = (ItemId)item.Infusions[1];
						}
						code.WeaponSet1.MainHand = await APICache.ResolveWeaponType(item.Id);
						if(item.Upgrades != null) {
							code.WeaponSet1.Sigil1 = (ItemId)item.Upgrades[0];
							if(item.Upgrades.Count > 1)
								code.WeaponSet1.Sigil2 = (ItemId)item.Upgrades[1];
						}
						break;

					case EquipmentItemSlot.WeaponA2:
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet1OffHand = await ResolveStatId(item);
						code.Infusions.WeaponSet1_2 = (ItemId?)item.Infusions?[0] ?? ItemId._UNDEFINED; //NOTE(Rennorb): this assues that buidls with twohanded main weapons dont contain an 'empty' weapon with no upgrades
						code.WeaponSet1.OffHand = await APICache.ResolveWeaponType(item.Id);
						code.WeaponSet1.Sigil2 = (ItemId?)item.Upgrades?[0] ?? ItemId._UNDEFINED; //NOTE(Rennorb): this assues that buidls with twohanded main weapons dont contain an 'empty' weapon with no upgrades
						break;

					case EquipmentItemSlot.WeaponB1:
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet2MainHand = await ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.WeaponSet2_1 = (ItemId)item.Infusions[0];
							if(item.Infusions.Count > 1)
								code.Infusions.WeaponSet2_2 = (ItemId)item.Infusions[1];
						}
						code.WeaponSet2.MainHand = await APICache.ResolveWeaponType(item.Id);
						if(item.Upgrades != null) {
							code.WeaponSet2.Sigil1 = (ItemId)item.Upgrades[0];
							if(IsTwoHanded(code.WeaponSet2.MainHand) && item.Upgrades.Count > 1)
								code.WeaponSet2.Sigil2 = (ItemId)item.Upgrades[1];
						}
						break;

					case EquipmentItemSlot.WeaponAquaticB:
						if(!aquatic) break;
						code.EquipmentAttributes.WeaponSet2MainHand = await ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.WeaponSet2_1 = (ItemId)item.Infusions[0];
							if(item.Infusions.Count > 1)
								code.Infusions.WeaponSet2_2 = (ItemId)item.Infusions[1];
						}
						code.WeaponSet2.MainHand = await APICache.ResolveWeaponType(item.Id);
						if(item.Upgrades != null) {
							code.WeaponSet2.Sigil1 = (ItemId)item.Upgrades[0];
							if(item.Upgrades.Count > 1)
								code.WeaponSet2.Sigil2 = (ItemId)item.Upgrades[1];
						}
						break;

					case EquipmentItemSlot.WeaponB2:
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet2OffHand = await ResolveStatId(item);
						code.Infusions.WeaponSet2_2 = (ItemId?)item.Infusions?[0] ?? ItemId._UNDEFINED;
						code.WeaponSet2.OffHand = await APICache.ResolveWeaponType(item.Id);
						code.WeaponSet2.Sigil2 = (ItemId?)item.Upgrades?[0] ?? ItemId._UNDEFINED;
						break;

					case EquipmentItemSlot.Amulet:
						if(aquatic) break;
						code.EquipmentAttributes.Amulet = await ResolveStatId(item);
						code.Infusions          .Amulet = (ItemId?)item.Infusions?[0] ?? ItemId._UNDEFINED;
						break;
				}
			}

			if(runeId != 0) code.Rune = runeId ?? ItemId._UNDEFINED;
		}
		else // WvW, PvE
		{
			var pvpEquip = activeEquipment.EquipmentPvp!;

			code.EquipmentAttributes.Helmet = (StatId)pvpEquip.Amulet.GetValueOrDefault();
			code.Rune = (ItemId?)pvpEquip.Rune ?? ItemId._UNDEFINED;
			code.WeaponSet1.Sigil1 = (ItemId?)pvpEquip.Sigils[0] ?? ItemId._UNDEFINED;
			code.WeaponSet1.Sigil2 = (ItemId?)pvpEquip.Sigils[1] ?? ItemId._UNDEFINED;
			code.WeaponSet2.Sigil1 = (ItemId?)pvpEquip.Sigils[2] ?? ItemId._UNDEFINED;
			code.WeaponSet2.Sigil2 = (ItemId?)pvpEquip.Sigils[3] ?? ItemId._UNDEFINED;
		}

		// swap weapon set so the first set always has the waepons if there are any.
		if(!code.WeaponSet1.HasAny && code.WeaponSet2.HasAny)
		{
			code.WeaponSet1 = code.WeaponSet2;
		}

		var apiSkills = aquatic ? activeBuild.AquaticSkills : activeBuild.Skills;
		code.SlotSkills.Heal     = (SkillId?)apiSkills.Heal ?? SkillId._UNDEFINED;
		code.SlotSkills.Utility1 = (SkillId?)apiSkills.Utilities[0] ?? SkillId._UNDEFINED;
		code.SlotSkills.Utility2 = (SkillId?)apiSkills.Utilities[1] ?? SkillId._UNDEFINED;
		code.SlotSkills.Utility3 = (SkillId?)apiSkills.Utilities[2] ?? SkillId._UNDEFINED;
		code.SlotSkills.Elite    = (SkillId?)apiSkills.Elite ?? SkillId._UNDEFINED;

		switch(code.Profession)
		{
			case Profession.Ranger:
				var rangerData = new RangerData();

				var petBlock = aquatic ? activeBuild.Pets!.Aquatic : activeBuild.Pets!.Terrestrial;
				rangerData.Pet1 = (PetId?)petBlock[0] ?? PetId._UNDEFINED;
				rangerData.Pet2 = (PetId?)petBlock[1] ?? PetId._UNDEFINED;

				code.ProfessionSpecific = rangerData;
				break;

			case Profession.Revenant:
				var revenantData = new RevenantData();

				var legends = aquatic ? activeBuild.AquaticLegends! : activeBuild.Legends!;
				var legend1 = ResolveLegend(in code.Specializations.Choice3, legends[0]);
				var legend2 = ResolveLegend(in code.Specializations.Choice3, legends[1]);
				if(legend1.HasValue) // One legend is always set.
				{
					revenantData.Legend1 = legend1.Value;
					revenantData.Legend2 = legend2 ?? Legend._UNDEFINED;

					//NOTE(Rennorb): doesnt seem to be available via the api
					// activeBuild.Skills = 
				}
				else // Flip so the legend 1 has the data.
				{
					revenantData.Legend1 = legend2!.Value;
					revenantData.Legend2 = legend1 ?? Legend._UNDEFINED;

					revenantData.AltUtilitySkill1 = code.SlotSkills.Utility1;
					revenantData.AltUtilitySkill2 = code.SlotSkills.Utility2;
					revenantData.AltUtilitySkill3 = code.SlotSkills.Utility3;

					// inactive skills dont seem to be available
					code.SlotSkills.Utility1 = SkillId._UNDEFINED;
					code.SlotSkills.Utility2 = SkillId._UNDEFINED;
					code.SlotSkills.Utility3 = SkillId._UNDEFINED;
				}

				code.ProfessionSpecific = revenantData;
				break;
		}

		Overrides.PostfixApiBuild(code);

		return code;
	}

	internal static async ValueTask<StatId> ResolveStatId(OfficialAPI.EquipmentItem item)
		=> item.Stats != null ? (StatId)item.Stats.Id : await APICache.ResolveStatId(item.Id);
}
