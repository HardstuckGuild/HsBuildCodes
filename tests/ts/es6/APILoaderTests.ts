import { describe, expect, test } from "@jest/globals";
import APILoader from "../../../include/ts/es6/APILoader";
import ItemId from "../../../include/ts/es6/Database/ItemIds";
import SkillId from "../../../include/ts/es6/Database/SkillIds";
import SpecializationId from "../../../include/ts/es6/Database/SpecializationIds";
import { ALL_EQUIPMENT_COUNT, ResolveAltRevSkills } from "../../../include/ts/es6/Database/Static";
import StatId from "../../../include/ts/es6/Database/StatIds";
import { Kind, Profession, RevenantData, TraitLineChoice, WeaponType } from "../../../include/ts/es6/Structures";
import { TraitLineChoices } from "../../../include/ts/es6/Util/UtilStructs";

const VALID_KEY = "92CE5A6C-E594-9D4D-B92B-5621ACFE047D436C02BD-0810-47D9-B9D4-2620EB7DD598";
const MISSING_PERMS_KEY = "AD041D99-AEEF-2E45-8732-0057285EFE370740BF1D-6427-4191-8C4F-84DD1C97F05F";

describe('FunctionTests', () => {
	test('ShouldThrowNotAToken', async () => {
		await expect(APILoader.LoadBuildCode("xxx", "sss", Kind.PvE)).rejects.toBeInstanceOf(Error);
	});
	
	test('ShouldThrowInvalidScopes', async () => {
		await expect(APILoader.LoadBuildCode(MISSING_PERMS_KEY, "sss", Kind.PvE)).rejects.toBeInstanceOf(Error);
	});

	test('ShouldFindMissinScopes', async () => {
		const missingScopes = await APILoader.ValidateScopes(MISSING_PERMS_KEY);
		expect(missingScopes).toStrictEqual(["characters", "builds"]);
	});

	test('ShouldThrowNoSuchCharacter', async () => {
		await expect(APILoader.LoadBuildCode(VALID_KEY, "does not exist", Kind.PvE)).rejects.toBeInstanceOf(Error);
	});
});

describe('BasicCodesTests', () => {
	test('LoadBuild', async () => {
		const code = await APILoader.LoadBuildCode(VALID_KEY, "Hardstuck Thief", Kind.PvE);
		expect(code.Profession).toBe(Profession.Thief);

		expect(code.Specializations[0].SpecializationId).toBe(SpecializationId.Deadly_Arts);
		const reference1 = new TraitLineChoices();
		reference1.Adept       = TraitLineChoice.BOTTOM;
		reference1.Master      = TraitLineChoice.MIDDLE;
		reference1.Grandmaster = TraitLineChoice.TOP;
		expect(code.Specializations[0].Choices).toStrictEqual(reference1);

		expect(code.Specializations[1].SpecializationId).toBe(SpecializationId.Trickery);
		const reference2 = new TraitLineChoices();
		reference2.Adept       = TraitLineChoice.BOTTOM;
		reference2.Master      = TraitLineChoice.TOP;
		reference2.Grandmaster = TraitLineChoice.TOP;
		expect(code.Specializations[1].Choices).toStrictEqual(reference2);

		expect(code.Specializations[2].SpecializationId).toBe(SpecializationId.Specter);
		const reference3 = new TraitLineChoices();
		reference3.Adept       = TraitLineChoice.BOTTOM;
		reference3.Master      = TraitLineChoice.BOTTOM;
		reference3.Grandmaster = TraitLineChoice.TOP;
		expect(code.Specializations[2].Choices).toStrictEqual(reference3);
		
		expect(code.WeaponSet1.MainHand).toBe(WeaponType.Scepter);
		expect(code.WeaponSet1.OffHand).toBe(WeaponType.Dagger );
		expect(code.WeaponSet2.MainHand).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet2.OffHand).toBe(WeaponType.Pistol );

		expect(code.WeaponSet1.Sigil1).toBe(ItemId.Legendary_Sigil_of_Demons);
		expect(code.WeaponSet1.Sigil2).toBe(ItemId.Legendary_Sigil_of_Concentration);
		expect(code.WeaponSet2.Sigil1).toBe(ItemId._UNDEFINED);
		expect(code.WeaponSet2.Sigil2).toBe(ItemId.Legendary_Sigil_of_Paralyzation);

		const celestialStatsKEKW = [ StatId.Celestial1, StatId.Celestial2, StatId.Celestial3, StatId.Celestial4 ];
		for(let i = 0; i < ALL_EQUIPMENT_COUNT; i++)
			if(i !== 13) // empty second main hand
				expect(celestialStatsKEKW).toContain(code.EquipmentAttributes[i]);

		expect(code.SlotSkills.Heal).toBe(SkillId.Well_of_Gloom  );
		expect(code.SlotSkills.Utility1).toBe(SkillId.Well_of_Silence);
		expect(code.SlotSkills.Utility2).toBe(SkillId.Well_of_Bounty );
		expect(code.SlotSkills.Utility3).toBe(SkillId.Well_of_Sorrow );
		expect(code.SlotSkills.Elite).toBe(SkillId.Shadowfall     );

		expect(code.Rune).toBe(ItemId.Legendary_Rune_of_the_Traveler);
	});

	/* regression: revenant skills would always show the alliance stance*/
	test('teapot1', async () => {
		var code = await APILoader.LoadBuildCode(VALID_KEY, "Hardstuck Revenant", Kind.PvE);

		expect(code.SlotSkills.Heal    ).toBe(SkillId.Facet_of_Light   );
		expect(code.SlotSkills.Utility1).toBe(SkillId.Facet_of_Darkness);
		expect(code.SlotSkills.Utility2).toBe(SkillId.Facet_of_Elements);
		expect(code.SlotSkills.Utility3).toBe(SkillId.Facet_of_Strength);
		expect(code.SlotSkills.Elite   ).toBe(SkillId.Facet_of_Chaos   );

		var altSkills = ResolveAltRevSkills(code.ProfessionSpecific as RevenantData);
		expect(altSkills.Heal    ).toBe(SkillId.Project_Tranquility);
		expect(altSkills.Utility1).toBe(SkillId.Protective_Solace1 );
		expect(altSkills.Utility2).toBe(SkillId.Natural_Harmony1   );
		expect(altSkills.Utility3).toBe(SkillId.Purifying_Essence1 );
		expect(altSkills.Elite   ).toBe(SkillId.Energy_Expulsion1  );
	});
});
