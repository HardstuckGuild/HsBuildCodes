import { BuildCode, Legend, Profession, RevenantData, Specialization } from "../Structures";
import SkillId from "./SkillIds";
import PerProfessionData from "./PerProfessionData";
import SpecializationId from "./SpecializationIds";

class Overrides {
	/** @remarks Requires PerProfessionData for Revs to be loaded first. */
	public static RevPalletteToSkill(legend : Legend, palletteId : number) : SkillId
	{
		switch(palletteId) {
			case 4572: switch(legend) {
					case Legend.SHIRO  : return SkillId.Enchanted_Daggers;
					case Legend.VENTARI: return SkillId.Project_Tranquility;
					case Legend.MALLYX : return SkillId.Empowering_Misery;
					case Legend.GLINT  : return SkillId.Facet_of_Light;
					case Legend.JALIS  : return SkillId.Soothing_Stone1;
					case Legend.KALLA  : return SkillId.Breakrazors_Bastion;
				} break;

			case 4564: switch(legend) {
					case Legend.SHIRO  : return SkillId.Impossible_Odds;
					case Legend.VENTARI: return SkillId.Purifying_Essence1;
					case Legend.MALLYX : return SkillId.Call_to_Anguish1;
					case Legend.GLINT  : return SkillId.Facet_of_Strength;
					case Legend.JALIS  : return SkillId.Vengeful_Hammers;
					case Legend.KALLA  : return SkillId.Darkrazors_Daring;
				} break;

			case 4614: switch(legend) {
					case Legend.SHIRO  : return SkillId.Riposting_Shadows;
					case Legend.VENTARI: return SkillId.Protective_Solace1;
					case Legend.MALLYX : return SkillId.Pain_Absorption;
					case Legend.GLINT  : return SkillId.Facet_of_Darkness;
					case Legend.JALIS  : return SkillId.Inspiring_Reinforcement1;
					case Legend.KALLA  : return SkillId.Razorclaws_Rage;
				} break;

			case 4651: switch(legend) {
					case Legend.SHIRO  : return SkillId.Phase_Traversal;
					case Legend.VENTARI: return SkillId.Natural_Harmony1;
					case Legend.MALLYX : return SkillId.Banish_Enchantment;
					case Legend.GLINT  : return SkillId.Facet_of_Elements;
					case Legend.JALIS  : return SkillId.Forced_Engagement;
					case Legend.KALLA  : return SkillId.Icerazors_Ire;
				} break;

			case 4554: switch(legend) {
					case Legend.SHIRO  : return SkillId.Jade_Winds1;
					case Legend.VENTARI: return SkillId.Energy_Expulsion1;
					case Legend.MALLYX : return SkillId.Embrace_the_Darkness;
					case Legend.GLINT  : return SkillId.Facet_of_Chaos;
					case Legend.JALIS  : return SkillId.Rite_of_the_Great_Dwarf;
					case Legend.KALLA  : return SkillId.Soulcleaves_Summit;
				} break;
			}

			return PerProfessionData.Revenant.PalletteToSkill[palletteId];
	}

	public static LoadAdditionalPerProfessionData(profession : Profession, data : PerProfessionData) : void
	{
		if(profession === Profession.Revenant) {
			const defaults = {
				[SkillId.Enchanted_Daggers  ]: 4572,
				[SkillId.Project_Tranquility]: 4572,
				[SkillId.Empowering_Misery  ]: 4572,
				[SkillId.Facet_of_Light     ]: 4572,
				[SkillId.Soothing_Stone1    ]: 4572,
				[SkillId.Breakrazors_Bastion]: 4572,

				[SkillId.Impossible_Odds   ]: 4564,
				[SkillId.Purifying_Essence1]: 4564,
				[SkillId.Purifying_Essence2]: 4564,
				[SkillId.Call_to_Anguish1  ]: 4564,
				[SkillId.Call_to_Anguish2  ]: 4564,
				[SkillId.Facet_of_Strength ]: 4564,
				[SkillId.Vengeful_Hammers  ]: 4564,
				[SkillId.Darkrazors_Daring ]: 4564,

				[SkillId.Riposting_Shadows       ]: 4614,
				[SkillId.Protective_Solace1      ]: 4614,
				[SkillId.Protective_Solace2      ]: 4614,
				[SkillId.Pain_Absorption         ]: 4614,
				[SkillId.Facet_of_Darkness       ]: 4614,
				[SkillId.Inspiring_Reinforcement1]: 4614,
				[SkillId.Inspiring_Reinforcement2]: 4614,
				[SkillId.Razorclaws_Rage         ]: 4614,

				[SkillId.Phase_Traversal   ]: 4651,
				[SkillId.Natural_Harmony1  ]: 4651,
				[SkillId.Natural_Harmony2  ]: 4651,
				[SkillId.Banish_Enchantment]: 4651,
				[SkillId.Facet_of_Elements ]: 4651,
				[SkillId.Forced_Engagement ]: 4651,
				[SkillId.Icerazors_Ire     ]: 4651,

				[SkillId.Jade_Winds1            ]: 4554,
				[SkillId.Jade_Winds2            ]: 4554,
				[SkillId.Energy_Expulsion1      ]: 4554,
				[SkillId.Energy_Expulsion2      ]: 4554,
				[SkillId.Embrace_the_Darkness   ]: 4554,
				[SkillId.Facet_of_Chaos         ]: 4554,
				[SkillId.Rite_of_the_Great_Dwarf]: 4554,
				[SkillId.Soulcleaves_Summit     ]: 4554,
			};

			data.SkillToPallette = Object.assign(defaults, data.SkillToPallette);
		}
	}

	public static FixRevApiSkill(legend : Legend, skillFromApi : SkillId) : SkillId
	{
		switch(skillFromApi) {
			case SkillId.Selfish_Spirit: switch(legend) {
					case Legend.SHIRO  : return SkillId.Enchanted_Daggers;
					case Legend.VENTARI: return SkillId.Project_Tranquility;
					case Legend.MALLYX : return SkillId.Empowering_Misery;
					case Legend.GLINT  : return SkillId.Facet_of_Light;
					case Legend.JALIS  : return SkillId.Soothing_Stone1;
					case Legend.KALLA  : return SkillId.Breakrazors_Bastion;
				} break;

			case SkillId.Reavers_Rage: switch(legend) {
					case Legend.SHIRO  : return SkillId.Impossible_Odds;
					case Legend.VENTARI: return SkillId.Purifying_Essence1;
					case Legend.MALLYX : return SkillId.Call_to_Anguish1;
					case Legend.GLINT  : return SkillId.Facet_of_Strength;
					case Legend.JALIS  : return SkillId.Vengeful_Hammers;
					case Legend.KALLA  : return SkillId.Darkrazors_Daring;
				} break;

			case SkillId.Nomads_Advance: switch(legend) {
					case Legend.SHIRO  : return SkillId.Riposting_Shadows;
					case Legend.VENTARI: return SkillId.Protective_Solace1;
					case Legend.MALLYX : return SkillId.Pain_Absorption;
					case Legend.GLINT  : return SkillId.Facet_of_Darkness;
					case Legend.JALIS  : return SkillId.Inspiring_Reinforcement1;
					case Legend.KALLA  : return SkillId.Razorclaws_Rage;
				} break;

			case SkillId.Scavenger_Burst: switch(legend) {
					case Legend.SHIRO  : return SkillId.Phase_Traversal;
					case Legend.VENTARI: return SkillId.Natural_Harmony1;
					case Legend.MALLYX : return SkillId.Banish_Enchantment;
					case Legend.GLINT  : return SkillId.Facet_of_Elements;
					case Legend.JALIS  : return SkillId.Forced_Engagement;
					case Legend.KALLA  : return SkillId.Icerazors_Ire;
				} break;

			case SkillId.Spear_of_Archemorus: switch(legend) {
					case Legend.SHIRO  : return SkillId.Jade_Winds1;
					case Legend.VENTARI: return SkillId.Energy_Expulsion1;
					case Legend.MALLYX : return SkillId.Embrace_the_Darkness;
					case Legend.GLINT  : return SkillId.Facet_of_Chaos;
					case Legend.JALIS  : return SkillId.Rite_of_the_Great_Dwarf;
					case Legend.KALLA  : return SkillId.Soulcleaves_Summit;
				} break;
		}
		
		return skillFromApi;
	}

	//NOTE(Rennorb): meme values returned from the characters api
	public static ResolveLegend(eliteSpec : Specialization, str : string|null) : Legend|null
	{ 
		switch (str) {
			case "Fire" : return Legend.GLINT;
			case "Water": return Legend.SHIRO;
			case "Air"  : return Legend.JALIS;
			case "Earth": return Legend.MALLYX;
			case "Deathshroud": return Legend.VENTARI;
			case null:
				if(eliteSpec.SpecializationId === SpecializationId.Vindicator) return Legend.VINDICATOR;
				if(eliteSpec.SpecializationId === SpecializationId.Renegade) return Legend.KALLA;
			default: return null;
		};
	}

	public static PostfixApiBuild(code : BuildCode) : void
	{
		//NOTE(Rennorb): Apparrently, mortar kit is utterly broken with the api.
		// Just guess that if the elite is empty that its actually mortar kit.
		if(code.Profession === Profession.Engineer && code.SlotSkills.Elite === SkillId._UNDEFINED)
		{
			code.SlotSkills.Elite = SkillId.Elite_Mortar_Kit;
		}

		//NOTE(Rennorb): Skill ids from the api for rev are always legendary alliance.
		// Swap them according to the legend
		if(code.Profession == Profession.Revenant)
		{
			const revData = code.ProfessionSpecific as RevenantData;
			for(let i = 0; i < 5; i++)
				code.SlotSkills[i] = Overrides.FixRevApiSkill(revData.Legend1, code.SlotSkills[i]);
			
			if(revData.Legend2 != Legend._UNDEFINED)
			{
				revData.AltUtilitySkill1 = Overrides.FixRevApiSkill(revData.Legend2, revData.AltUtilitySkill1);
				revData.AltUtilitySkill2 = Overrides.FixRevApiSkill(revData.Legend2, revData.AltUtilitySkill2);
				revData.AltUtilitySkill3 = Overrides.FixRevApiSkill(revData.Legend2, revData.AltUtilitySkill3);
			}
		}
	}

	public static FixWeaponTypeName(apiName : string) : string {
		switch (apiName)  {
			//other spellings because of issue #21
			case "LongBow" : return "Longbow";
			case "ShortBow": return "Shortbow";
			default: return apiName;
		};
	}
}
export default Overrides;
