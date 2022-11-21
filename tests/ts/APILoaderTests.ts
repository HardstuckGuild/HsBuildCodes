import { describe, expect, test } from "@jest/globals";
import APILoader from "../../include/ts/APILoader";
import ItemId from "../../include/ts/Database/ItemIds";
import SkillId from "../../include/ts/Database/SkillIds";
import SpecializationId from "../../include/ts/Database/SpecializationIds";
import Static from "../../include/ts/Database/Static";
import StatId from "../../include/ts/Database/StatIds";
import { Kind, Profession, TraitLineChoice, WeaponType } from "../../include/ts/Structures";
import { TraitLineChoices } from "../../include/ts/Util/UtilStructs";

const VALID_KEY = "92CE5A6C-E594-9D4D-B92B-5621ACFE047D436C02BD-0810-47D9-B9D4-2620EB7DD598";
const MISSING_PERMS_KEY = "AD041D99-AEEF-2E45-8732-0057285EFE370740BF1D-6427-4191-8C4F-84DD1C97F05F";

describe('FunctionTests', () => {
	test('ShouldThrowNotAToken', () => {
		expect(() => {
			const code = APILoader.LoadBuildCode("xxx", "sss", Kind.PvE);
		}).toThrow();
	});
	
	test('ShouldThrowInvalidScopes', () => {
		expect(() => {
			const code = APILoader.LoadBuildCode(MISSING_PERMS_KEY, "sss", Kind.PvE);
		}).toThrow();
	});

	test('ShouldFindMissinScopes', () => {
		const missingScopes = APILoader.ValidateScopes(MISSING_PERMS_KEY);
		expect(missingScopes).toBe(["characters", "builds"]);
	});

	test('ShouldThrowNoSuchCharacter', () => {
		expect(() => {
			const code = APILoader.LoadBuildCode(VALID_KEY, "does not exist", Kind.PvE);
		}).toThrow();
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
		expect(code.Specializations[0].Choices).toBe(reference1);

		expect(code.Specializations[1].SpecializationId).toBe(SpecializationId.Trickery);
		const reference2 = new TraitLineChoices();
		reference2.Adept       = TraitLineChoice.BOTTOM;
		reference2.Master      = TraitLineChoice.TOP;
		reference2.Grandmaster = TraitLineChoice.TOP;
		expect(code.Specializations[1].Choices).toBe(reference2);

		expect(code.Specializations[2].SpecializationId).toBe(SpecializationId.Specter);
		const reference3 = new TraitLineChoices();
		reference3.Adept       = TraitLineChoice.BOTTOM;
		reference3.Master      = TraitLineChoice.BOTTOM;
		reference3.Grandmaster = TraitLineChoice.TOP;
		expect(code.Specializations[2].Choices).toBe(reference3);
		
		expect(code.WeaponSet1.MainHand).toBe(WeaponType.Scepter);
		expect(code.WeaponSet1.OffHand).toBe(WeaponType.Dagger );
		expect(code.WeaponSet2.MainHand).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet2.OffHand).toBe(WeaponType.Pistol );

		expect(code.WeaponSet1.Sigil1).toBe(ItemId.Superior_Sigil_of_Deamons2);
		expect(code.WeaponSet1.Sigil2).toBe(ItemId.Superior_Sigil_of_Concentration2);
		expect(code.WeaponSet2.Sigil1).toBe(ItemId._UNDEFINED);
		expect(code.WeaponSet2.Sigil2).toBe(ItemId.Superior_Sigil_of_Paralysation2);

		const celestialStatsKEKW = [ StatId.Celestial1, StatId.Celestial2, StatId.Celestial3, StatId.Celestial4 ];
		for(let i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++)
			if(i !== 13) // empty second main hand
				expect(celestialStatsKEKW).toContain(code.EquipmentAttributes[i]);

		expect(code.SlotSkills.Heal).toBe(SkillId.Well_of_Gloom  );
		expect(code.SlotSkills.Utility1).toBe(SkillId.Well_of_Silence);
		expect(code.SlotSkills.Utility2).toBe(SkillId.Well_of_Bounty );
		expect(code.SlotSkills.Utility3).toBe(SkillId.Well_of_Sorrow );
		expect(code.SlotSkills.Elite).toBe(SkillId.Shadowfall     );

		expect(code.Rune).toBe(ItemId.Superior_Rune_of_the_Traveler2);
	});
});