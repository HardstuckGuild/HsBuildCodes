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

	/** @remarks Requires PerProfessionData for Revs to be loaded first. */
	public static RevSkillToPallette(skillId : SkillId) : number
	{
		switch(skillId) {
			case SkillId.Enchanted_Daggers:
			case SkillId.Project_Tranquility:
			case SkillId.Empowering_Misery:
			case SkillId.Facet_of_Light:
			case SkillId.Soothing_Stone1:
			case SkillId.Soothing_Stone2:
			case SkillId.Breakrazors_Bastion:
				return 4572;

			case SkillId.Impossible_Odds:
			case SkillId.Purifying_Essence1:
			case SkillId.Purifying_Essence2:
			case SkillId.Call_to_Anguish1:
			case SkillId.Call_to_Anguish2:
			case SkillId.Facet_of_Strength:
			case SkillId.Vengeful_Hammers:
			case SkillId.Darkrazors_Daring:
				return 4564;

			case SkillId.Riposting_Shadows:
			case SkillId.Protective_Solace1:
			case SkillId.Protective_Solace2:
			case SkillId.Pain_Absorption:
			case SkillId.Facet_of_Darkness:
			case SkillId.Inspiring_Reinforcement1:
			case SkillId.Inspiring_Reinforcement2:
			case SkillId.Razorclaws_Rage:
				return 4614;

			case SkillId.Phase_Traversal:
			case SkillId.Natural_Harmony1:
			case SkillId.Natural_Harmony2:
			case SkillId.Banish_Enchantment:
			case SkillId.Facet_of_Elements:
			case SkillId.Forced_Engagement:
			case SkillId.Icerazors_Ire:
				return 4651;

			case SkillId.Jade_Winds1:
			case SkillId.Jade_Winds2:
			case SkillId.Energy_Expulsion1:
			case SkillId.Energy_Expulsion2:
			case SkillId.Embrace_the_Darkness:
			case SkillId.Facet_of_Chaos:
			case SkillId.Rite_of_the_Great_Dwarf:
			case SkillId.Soulcleaves_Summit:
				return 4554;
		}

		return PerProfessionData.Revenant.SkillToPallette[skillId];
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
}
export default Overrides;
