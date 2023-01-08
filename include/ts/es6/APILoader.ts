import ItemId from "./Database/ItemIds";
import Overrides from "./Database/Overrides";
import SkillId from "./Database/SkillIds";
import { CURRENT_VERSION, IsTwoHanded, ResolveLegend } from "./Database/Static";
import StatId from "./Database/StatIds";
import API from "./OfficialAPI/API";
import APICache from "./OfficialAPI/APICache";
import { BuildCode, Kind, Legend, Profession, RangerData, RevenantData, Specialization, WeaponSet } from "./Structures";
import { TraitLineChoices } from "./Util/UtilStructs";

class APILoader {
	/** Produces a list of token scopes that are missing. */
	public static async ValidateScopes(token : string) : Promise<string[]>
	{
		const tokenInfo = await API.RequestJson("/tokeninfo", token);
		const required = [ "account", "characters", "builds"  ];
		return required.filter(req => !tokenInfo.permissions.includes(req));
	}

	//NOTE(Rennorb): Removed Load from current character because php is not run on clients

	/** 
	 * @remarks This method assumes the scopes account, character and build are available, but does not explicitly test for them.
	 * @throws Error If the character can't be found.
	 * @throws Error If scopes are missing.
	 * @throws Error If the token is not valid.
	 */
	public static async LoadBuildCode(authToken : string, characterName : string, targetGameMode : Kind, aquatic : boolean = false) : Promise<BuildCode>
	{
		const code = new BuildCode();
		code.Version = CURRENT_VERSION;
		code.Kind    = targetGameMode;

		const playerData = await API.RequestJson("/characters/"+characterName, authToken);
		
		code.Profession = Profession[playerData.profession as keyof typeof Profession];

		const activeBuild = playerData.build_tabs.find(tab => tab.is_active).build;
		for(let i = 0; i < 3; i++) {
			const spec = activeBuild.specializations[i];
			if(spec.id === null) continue;

			const choices = new TraitLineChoices();
			choices.Adept       = await APICache.ResolvePosition(spec.traits[0]);
			choices.Master      = await APICache.ResolvePosition(spec.traits[1]);
			choices.Grandmaster = await APICache.ResolvePosition(spec.traits[2]);
			code.Specializations[i] = new Specialization(spec.id, choices);
		}

		//const activeEquipment = playerData.equipment_tabs.find(tab => tab.is_active);
		const activeEquipment = playerData; // #15
		if(targetGameMode !== Kind.PvP)
		{
			let runeId : ItemId|null = null;

			const SetArmorData = function(equipSlot : number, item) : void  {
				code.EquipmentAttributes[equipSlot] = APILoader.ResolveStatId(item);
				code.Infusions          [equipSlot] = item.infusions ? item.infusions[0] : ItemId._UNDEFINED;
				if(item.upgrades) {
					if(runeId === null) runeId = item.upgrades[0];
					else if(runeId !== item.upgrades[0]) runeId = ItemId._UNDEFINED;
				}
			};

			for(const item of activeEquipment.equipment) {
				//NOTE(Rennorb): #15 can be removed once the api bug is fixed as the data in templates already only shows the equipped stuff
				if(item.Location === "Armory" || item.Location === "LegendaryArmory")
					continue;

				switch(item.slot)
				{
					case "Helm"       : if( aquatic) break; SetArmorData(0, item); break;
					case "HelmAquatic": if(!aquatic) break; SetArmorData(0, item); break;
					case "Shoulders"  :                     SetArmorData(1, item); break;
					case "Coat"       :                     SetArmorData(2, item); break;
					case "Gloves"     :                     SetArmorData(3, item); break;
					case "Leggings"   :                     SetArmorData(4, item); break;
					case "Boots"      :                     SetArmorData(5, item); break;
						
					case "Backpack":
						code.EquipmentAttributes.BackItem = APILoader.ResolveStatId(item);
						if(item.infusions) {
							code.Infusions.BackItem_1 = item.infusions[0];
							if(item.infusions.length > 1)
								code.Infusions.BackItem_2 = item.infusions[1];
						}
						break;

					case "Accessory1":
						code.EquipmentAttributes.Accessory1 = APILoader.ResolveStatId(item);
						code.Infusions          .Accessory1 = item.infusions ? item.infusions[0] : ItemId._UNDEFINED;
						break;

					case "Accessory2":
						code.EquipmentAttributes.Accessory2 = APILoader.ResolveStatId(item);
						code.Infusions          .Accessory2 = item.infusions ? item.infusions[0] : ItemId._UNDEFINED;
						break;

					case "Ring1":
						code.EquipmentAttributes.Ring1 = APILoader.ResolveStatId(item);
						if(item.infusions) {
							code.Infusions.Ring1_1 = item.infusions[0];
							if(item.infusions.length > 1) {
								code.Infusions.Ring1_2 = item.infusions[1];
								if(item.infusions.length > 2)
									code.Infusions.Ring1_3 = item.infusions[2];
							}
						}
						break;
						
					case "Ring2":
						code.EquipmentAttributes.Ring2 = APILoader.ResolveStatId(item);
						if(item.infusions) {
							code.Infusions.Ring2_1 = item.infusions[0];
							if(item.infusions.length > 1) {
								code.Infusions.Ring2_2 = item.infusions[1];
								if(item.infusions.length > 2)
									code.Infusions.Ring2_3 = item.infusions[2];
							}
						}
						break;
						
					case "WeaponA1":
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet1MainHand = APILoader.ResolveStatId(item);
						if(item.infusions) {
							code.Infusions.WeaponSet1_1 = item.infusions[0];
							if(item.infusions.length > 1)
								code.Infusions.WeaponSet1_2 = item.infusions[1];
						}
						code.WeaponSet1.MainHand = await APICache.ResolveWeaponType(item.id);
						if(item.upgrades) {
							code.WeaponSet1.Sigil1 = item.upgrades[0];
							if(item.upgrades.length > 1)
								code.WeaponSet1.Sigil2 = item.upgrades[1];
						}
						break;

					case "WeaponAquaticA":
						if(!aquatic) break;
						code.EquipmentAttributes.WeaponSet1MainHand = APILoader.ResolveStatId(item);
						if(item.infusions) {
							code.Infusions.WeaponSet1_1 = item.infusions[0];
							if(item.infusions.length > 1)
								code.Infusions.WeaponSet1_2 = item.infusions[1];
						}
						code.WeaponSet1.MainHand = await APICache.ResolveWeaponType(item.id);
						if(item.upgrades) {
							code.WeaponSet1.Sigil1 = item.upgrades[0];
							if(item.upgrades.length > 1)
								code.WeaponSet1.Sigil2 = item.upgrades[1];
						}
						break;

					case "WeaponA2":
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet1OffHand = APILoader.ResolveStatId(item);
						code.Infusions.WeaponSet1_2 = item.infusions ? item.infusions[0] : ItemId._UNDEFINED; //NOTE(Rennorb): this assues that buidls with twohanded main weapons dont contain an 'empty' weapon with no upgrades
						code.WeaponSet1.OffHand = await APICache.ResolveWeaponType(item.id);
						code.WeaponSet1.Sigil2 =  item.upgrades ? item.upgrades[0] : ItemId._UNDEFINED; //NOTE(Rennorb): this assues that buidls with twohanded main weapons dont contain an 'empty' weapon with no upgrades
						break;

					case "WeaponB1":
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet2MainHand = APILoader.ResolveStatId(item);
						if(item.infusions) {
							code.Infusions.WeaponSet2_1 = item.infusions[0];
							if(item.infusions.length > 1)
								code.Infusions.WeaponSet2_2 = item.infusions[1];
						}
						code.WeaponSet2.MainHand = await APICache.ResolveWeaponType(item.id);
						if(item.upgrades) {
							code.WeaponSet2.Sigil1 = item.upgrades[0];
							if(IsTwoHanded(code.WeaponSet2.MainHand) && item.upgrades.length > 1)
								code.WeaponSet2.Sigil2 = item.upgrades[1];
						}
						break;

					case "WeaponAquaticB":
						if(!aquatic) break;
						code.EquipmentAttributes.WeaponSet2MainHand = APILoader.ResolveStatId(item);
						if(item.infusions) {
							code.Infusions.WeaponSet2_1 = item.infusions[0];
							if(item.infusions.length > 1)
								code.Infusions.WeaponSet2_2 = item.infusions[1];
						}
						code.WeaponSet2.MainHand = await APICache.ResolveWeaponType(item.id);
						if(item.upgrades) {
							code.WeaponSet2.Sigil1 = item.upgrades[0];
							if(item.upgrades.length > 1)
								code.WeaponSet2.Sigil2 = item.upgrades[1];
						}
						break;

					case "WeaponB2":
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet2OffHand = APILoader.ResolveStatId(item);
						code.Infusions.WeaponSet2_2 = item.infusions ? item.infusions[0] : ItemId._UNDEFINED;
						code.WeaponSet2.OffHand = await APICache.ResolveWeaponType(item.id);
						code.WeaponSet2.Sigil2 = item.upgrades ? item.upgrades[0] : ItemId._UNDEFINED;
						break;

					case "Amulet":
						if(aquatic) break;
						code.EquipmentAttributes.Amulet = APILoader.ResolveStatId(item);
						code.Infusions          .Amulet = item.infusions ? item.infusions[0] : ItemId._UNDEFINED;
						break;
				}
			}

			if(runeId !== 0) code.Rune = runeId ?? ItemId._UNDEFINED;
		}
		else // WvW, PvE
		{
			//const pvpEquip = activeEquipment.equipment_pvp;
			const pvpEquip = playerData.equipment_tabs.find(tab => tab.is_active).equipment_pvp; // #15

			for(const item of activeEquipment.equipment) {
				//NOTE(Rennorb): #15 can be removed once the api bug is fixed as the data in templates already only shows the equipped stuff
				if(item.Location === "Armory" || item.Location === "LegendaryArmory")
					continue;

				switch(item.slot) {
					case "WeaponA1"      : if(!aquatic) code.WeaponSet1.MainHand = await APICache.ResolveWeaponType(item.id); break;
					case "WeaponAquaticA": if( aquatic) code.WeaponSet1.MainHand = await APICache.ResolveWeaponType(item.id); break;
					case "WeaponA2"      : if(!aquatic) code.WeaponSet1.OffHand  = await APICache.ResolveWeaponType(item.id); break;
					case "WeaponB1"      : if(!aquatic) code.WeaponSet2.MainHand = await APICache.ResolveWeaponType(item.id); break;
					case "WeaponAquaticB": if( aquatic) code.WeaponSet2.MainHand = await APICache.ResolveWeaponType(item.id); break;
					case "WeaponB2"      : if(!aquatic) code.WeaponSet2.OffHand  = await APICache.ResolveWeaponType(item.id); break;
				}
			}

			code.EquipmentAttributes.Amulet = pvpEquip.amulet ?? 0;
			code.Rune = pvpEquip.rune ?? 0;
			code.WeaponSet1.Sigil1 = pvpEquip.sigils[0] ?? 0;
			code.WeaponSet1.Sigil2 = pvpEquip.sigils[1] ?? 0;
			code.WeaponSet2.Sigil1 = pvpEquip.sigils[2] ?? 0;
			code.WeaponSet2.Sigil2 = pvpEquip.sigils[3] ?? 0;
		}

		// swap weapon set so the first set always has the waepons if there are any.
		if(!code.WeaponSet1.HasAny() && code.WeaponSet2.HasAny())
		{
			code.WeaponSet1 = code.WeaponSet2;
			code.WeaponSet2 = new WeaponSet();
		}

		const apiSkills = aquatic ? activeBuild.aquatic_skills : activeBuild.skills;
		code.SlotSkills.Heal     = apiSkills.heal         ?? 0;
		code.SlotSkills.Utility1 = apiSkills.utilities[0] ?? 0;
		code.SlotSkills.Utility2 = apiSkills.utilities[1] ?? 0;
		code.SlotSkills.Utility3 = apiSkills.utilities[2] ?? 0;
		code.SlotSkills.Elite    = apiSkills.elite        ?? 0;

		switch(code.Profession)
		{
			case Profession.Ranger:
				const rangerData = new RangerData();

				const petBlock = aquatic ? activeBuild.pets.aquatic : activeBuild.pets.terrestrial;
				rangerData.Pet1 = petBlock[0] ?? 0;
				rangerData.Pet2 = petBlock[1] ?? 0;

				code.ProfessionSpecific = rangerData;
				break;

			case Profession.Revenant:
				const revenantData = new RevenantData();

				const legends = aquatic ? activeBuild.aquatic_legends : activeBuild.legends;
				const legend1 = ResolveLegend(code.Specializations.Choice3, legends[0]);
				const legend2 = ResolveLegend(code.Specializations.Choice3, legends[1]);
				if(legend1 !== null)
				{
					revenantData.Legend1 = legend1;
					revenantData.Legend2 = legend2 ?? Legend._UNDEFINED;

					if(legend2 !== null)
					{
						//NOTE(Rennorb): inactive skills dont seem to be available so we just use the one that are selected on the other legend
						revenantData.AltUtilitySkill1 = code.SlotSkills.Utility1;
						revenantData.AltUtilitySkill2 = code.SlotSkills.Utility2;
						revenantData.AltUtilitySkill3 = code.SlotSkills.Utility3;
					}
				}
				else // Flip so the legend 1 has the data.
				{
					revenantData.Legend1 = legend2!; // One legend is always set.
					revenantData.Legend2 = Legend._UNDEFINED;
				}

				code.ProfessionSpecific = revenantData;
				break;
		}

		Overrides.PostfixApiBuild(code);

		return code;
	}

	private static ResolveStatId(item) : StatId
	{ return item.stats !== null ? item.stats.id : APICache.ResolveStatId(item.id); }
}
export default APILoader;
