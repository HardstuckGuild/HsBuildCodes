import { describe, expect, test } from "@jest/globals";
import ItemId from "../../../include/ts/es6/Database/ItemIds";
import LazyLoadMode from "../../../include/ts/es6/Database/LazyLoadMode";
import PerProfessionData from "../../../include/ts/es6/Database/PerProfessionData";
import SkillId from "../../../include/ts/es6/Database/SkillIds";
import SpecializationId from "../../../include/ts/es6/Database/SpecializationIds";
import Static from "../../../include/ts/es6/Database/Static";
import StatId from "../../../include/ts/es6/Database/StatIds";
import { Arbitrary, BuildCode, Kind, Legend, PetId, Profession, ProfessionSpecific, RangerData, RevenantData, Specialization, TraitLineChoice, WeaponType } from "../../../include/ts/es6/Structures";
import TextLoader from "../../../include/ts/es6/TextLoader";
import StringView from "../../../include/ts/es6/Util/StringView";
import { TraitLineChoices } from "../../../include/ts/es6/Util/UtilStructs";
import TestUtilities from "./TestUtilities";

describe('FunctionTests', () => {
	test('DecodeValueFixed', () => {
		for(let i = 0; i < 64; i++)
			expect(TextLoader.INVERSE_CHARSET[TextLoader.CHARSET[i].charCodeAt(0)]).toBe(i);
	});

	test('SuccessiveDecodeAndEatValueFixed', () => {
		const text = new StringView("Aa-");
		expect(TextLoader.DecodeAndAdvance(text)).toBe( 0);
		expect(TextLoader.DecodeAndAdvance(text)).toBe(26);
		expect(TextLoader.DecodeAndAdvance(text)).toBe(63);
		expect(text.LengthRemaining()).toBe(0);
	});

	test('SuccessiveDecodeAndEatValueValirable', () => {
		const text = new StringView("Aa-");
		expect(TextLoader.DecodeAndAdvance(text, 1)).toBe( 0);
		expect(TextLoader.DecodeAndAdvance(text, 1)).toBe(26);
		expect(TextLoader.DecodeAndAdvance(text, 1)).toBe(63);
		expect(text.LengthRemaining()).toBe(0);
	});

	test('DecodeAndEatValueEarlyTerm', () => {
		const text = new StringView("A~");
		expect(TextLoader.DecodeAndAdvance(text, 3)).toBe(0);
		expect(text.LengthRemaining()).toBe(0);
	});
});

describe('BasicCodesTests', () => {
	test('ShouldThrowVersion', () => {
		expect(() => {
			const code = TextLoader.LoadBuildCode(TestUtilities.CodesInvalid["wrong-version"]);
		}).toThrow();
	});

	test('ShouldThrowTooShort', () => {
		expect(() => {
			const code = TextLoader.LoadBuildCode(TestUtilities.CodesInvalid["too-short"]);
		}).toThrow();
	});

	test('ShouldThrowInvalidCharacters', () => {
		expect(() => {
			const code = TextLoader.LoadBuildCode(TestUtilities.CodesInvalid["invalid-chars"]);
		}).toThrow();
	});

	test('MinimalPvP', () => {
		const code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["minimal-pvp"]);
		expect(code.Version).toBe(3);
		expect(code.Kind).toBe(Kind.PvP);
		expect(code.Profession).toBe(Profession.Guardian);
		for(let i = 0; i < 3; i++)
			expect(code.Specializations[i].SpecializationId).toBe(SpecializationId._UNDEFINED);
		expect(code.WeaponSet1.HasAny()).toBeFalsy();
		expect(code.WeaponSet2.HasAny()).toBeFalsy();
		for(let i = 0; i < 5; i++)
			expect(code.SlotSkills[i]).toBe(SkillId._UNDEFINED);
		expect(code.Rune).toBe(ItemId._UNDEFINED);
		for(let i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			if(i >= 11 && i <= 14) expect(code.EquipmentAttributes[i]).toBe(StatId._UNDEFINED);

			else if(i === Static.ALL_EQUIPMENT_COUNT - 1) expect(code.EquipmentAttributes[i]).toBe(1); 
			else expect(code.EquipmentAttributes[i]).toBe(StatId._UNDEFINED);
		}
		for(let i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			expect(code.Infusions[i]).toBe(ItemId._UNDEFINED);
		expect(code.Food).toBe(ItemId._UNDEFINED);
		expect(code.Utility).toBe(ItemId._UNDEFINED);
		expect(code.ProfessionSpecific).toBe(ProfessionSpecific.NONE.GetInstance());
		expect(code.Arbitrary)         .toBe(Arbitrary         .NONE.GetInstance());
	});

	test('MinimalPvE', () => {
		const code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["minimal-pve"]);
		expect(code.Version).toBe(3                   );
		expect(code.Kind).toBe(Kind.PvE           );
		expect(code.Profession).toBe(Profession.Guardian);
		for(let i = 0; i < 3; i++)
			expect(code.Specializations[i].SpecializationId).toBe(SpecializationId._UNDEFINED);
		expect(code.WeaponSet1.HasAny()).toBeFalsy();
		expect(code.WeaponSet2.HasAny()).toBeFalsy();
		for(let i = 0; i < 5; i++)
			expect(code.SlotSkills[i]).toBe(SkillId._UNDEFINED);
		expect(code.Rune).toBe(ItemId._UNDEFINED);
		for(let i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			if(11 <= i && i <= 14) expect(code.EquipmentAttributes[i]).toBe(StatId._UNDEFINED);
			else expect(code.EquipmentAttributes[i]).toBe(1);
		}
		for(let i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			expect(code.Infusions[i]).toBe(ItemId._UNDEFINED);
		expect(code.Food).toBe(ItemId._UNDEFINED);
		expect(code.Utility).toBe(ItemId._UNDEFINED);
		expect(code.ProfessionSpecific).toBe(ProfessionSpecific.NONE.GetInstance());
		expect(code.Arbitrary)         .toBe(Arbitrary         .NONE.GetInstance());
	});

	test('MinimalRanger', () => {
		const code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["minimal-ranger"]);
		expect(code.ProfessionSpecific).toBeInstanceOf(RangerData);
		const data = code.ProfessionSpecific as RangerData;
		expect(data.Pet1).toBe(PetId._UNDEFINED);
		expect(data.Pet2).toBe(PetId._UNDEFINED);
	});

	test('MinimalRevenant', () => {
		const code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["minimal-revenant"]);
		expect(code.ProfessionSpecific).toBeInstanceOf(RevenantData);
		const data = code.ProfessionSpecific as RevenantData;
		expect(data.Legend1).toBe(Legend.SHIRO);
		expect(data.Legend2).toBe(Legend._UNDEFINED);
		expect(data.AltUtilitySkill1).toBe(SkillId._UNDEFINED);
		expect(data.AltUtilitySkill2).toBe(SkillId._UNDEFINED);
		expect(data.AltUtilitySkill3).toBe(SkillId._UNDEFINED);
	});

	test('CycleBasicCode', () => {
		const text1 = TestUtilities.CodesV2["minimal-revenant"];
		const code = TextLoader.LoadBuildCode(text1);
		const text2 = TextLoader.WriteBuildCode(code);
		expect(text2).toBe(text1);
	});

	test('MidNecro', () => {
		const code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["mid-necro"]);
		expect(code.Profession).toBe(Profession.Necromancer);

		expect(code.WeaponSet1.MainHand).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet1.OffHand ).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet2.MainHand).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet2.OffHand ).toBe(WeaponType._UNDEFINED);

		expect(code.Rune).toBe(ItemId._UNDEFINED);
		for(let i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			expect(code.EquipmentAttributes[i]).toBe(StatId._UNDEFINED);
		}

		expect(code.Specializations[0].SpecializationId).toBe(SpecializationId.Spite);
		const reference1 = new TraitLineChoices();
		reference1.Adept       = TraitLineChoice.TOP;
		reference1.Master      = TraitLineChoice.MIDDLE;
		reference1.Grandmaster = TraitLineChoice.MIDDLE;
		expect(code.Specializations[0].Choices).toStrictEqual(reference1);

		expect(code.Specializations[1].SpecializationId).toBe(SpecializationId.Soul_Reaping);
		const reference2 = new TraitLineChoices();
		reference2.Adept       = TraitLineChoice.TOP;
		reference2.Master      = TraitLineChoice.TOP;
		reference2.Grandmaster = TraitLineChoice.MIDDLE;
		expect(code.Specializations[1].Choices).toStrictEqual(reference2);

		expect(code.Specializations[2].SpecializationId).toBe(SpecializationId.Reaper);
		const reference3 = new TraitLineChoices();
		reference3.Adept       = TraitLineChoice.MIDDLE;
		reference3.Master      = TraitLineChoice.TOP;
		reference3.Grandmaster = TraitLineChoice.BOTTOM;
		expect(code.Specializations[2].Choices).toStrictEqual(reference3);

		expect(code.SlotSkills[0]).toBe(SkillId.Your_Soul_Is_Mine);
		expect(code.SlotSkills[1]).toBe(SkillId.Well_of_Suffering1);
		expect(code.SlotSkills[2]).toBe(SkillId.Well_of_Darkness1);
		expect(code.SlotSkills[3]).toBe(SkillId.Signet_of_Spite);
		expect(code.SlotSkills[4]).toBe(SkillId.Summon_Flesh_Golem);
	});

	test('FullNecro', () => {
		const code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["full-necro"]);
		expect(code.Profession).toBe(Profession.Necromancer);

		expect(code.WeaponSet1.MainHand).toBe(WeaponType.Axe    );
		expect(code.WeaponSet1.Sigil1).toBe(ItemId.Superior_Sigil_of_Paralysation2);
		expect(code.WeaponSet1.OffHand).toBe(WeaponType.Dagger );
		expect(code.WeaponSet1.Sigil2).toBe(ItemId.Superior_Sigil_of_Paralysation2);
		expect(code.WeaponSet2.MainHand).toBe(WeaponType.Scepter);
		expect(code.WeaponSet2.Sigil1).toBe(ItemId.Superior_Sigil_of_Paralysation2);
		expect(code.WeaponSet2.OffHand).toBe(WeaponType.Focus  );
		expect(code.WeaponSet2.Sigil2).toBe(ItemId.Superior_Sigil_of_Paralysation2);

		expect(code.Rune).toBe(ItemId.Superior_Rune_of_the_Scholar);
		const berserkers = [StatId.Berserkers1, StatId.Berserkers2, StatId.Berserkers3, StatId.Berserkers4, StatId.Berserkers5];
		for(let i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			expect(berserkers).toContain(code.EquipmentAttributes[i]);
		}

		for(let i = 0; i < Static.ALL_INFUSION_COUNT; i++) {
			expect(code.Infusions[i]).toBe(ItemId.Mighty_5_Agony_Infusion);
		}

		expect(code.Specializations[0].SpecializationId).toBe(SpecializationId.Spite);
		const reference1 = new TraitLineChoices();
		reference1.Adept       = TraitLineChoice.TOP;
		reference1.Master      = TraitLineChoice.MIDDLE;
		reference1.Grandmaster = TraitLineChoice.MIDDLE;
		expect(code.Specializations[0].Choices).toStrictEqual(reference1);

		expect(code.Specializations[1].SpecializationId).toBe(SpecializationId.Soul_Reaping);
		const reference2 = new TraitLineChoices();
		reference2.Adept       = TraitLineChoice.TOP;
		reference2.Master      = TraitLineChoice.TOP;
		reference2.Grandmaster = TraitLineChoice.MIDDLE;
		expect(code.Specializations[1].Choices).toStrictEqual(reference2);

		expect(code.Specializations[2].SpecializationId).toBe(SpecializationId.Reaper);
		const reference3 = new TraitLineChoices();
		reference3.Adept       = TraitLineChoice.MIDDLE;
		reference3.Master      = TraitLineChoice.TOP;
		reference3.Grandmaster = TraitLineChoice.BOTTOM;
		expect(code.Specializations[2].Choices).toStrictEqual(reference3);

		expect(code.SlotSkills[0]).toBe(SkillId.Your_Soul_Is_Mine);
		expect(code.SlotSkills[1]).toBe(SkillId.Well_of_Suffering1);
		expect(code.SlotSkills[2]).toBe(SkillId.Well_of_Darkness1);
		expect(code.SlotSkills[3]).toBe(SkillId.Signet_of_Spite);
		expect(code.SlotSkills[4]).toBe(SkillId.Summon_Flesh_Golem);

		expect(code.Food).toBe(ItemId.Bowl_of_Sweet_and_spicy_Butternut_Squash_Soup);
		expect(code.Utility).toBe(ItemId.Tin_of_Fruitcake);
	});

	test('ParseAll', () => {
		for(const [name, code] of Object.entries(TestUtilities.CodesV2)) {
			try {
				const code_ = TextLoader.LoadBuildCode(code);
			} catch(ex : any) {
				console.error(`${name} (${code}) failed`, ex);
				throw new Error(`${name} (${code}) failed`);
			}
		}

		expect(true).toBeTruthy();
	});

	test('FullNecroCompressed', () => {
		const code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["full-necro-binary"]);
		expect(code.Profession).toBe(Profession.Necromancer);

		expect(code.WeaponSet1.MainHand).toBe(WeaponType.Axe    );
		expect(code.WeaponSet1.Sigil1).toBe(ItemId.Superior_Sigil_of_Paralysation2);
		expect(code.WeaponSet1.OffHand).toBe(WeaponType.Dagger );
		expect(code.WeaponSet1.Sigil2).toBe(ItemId.Superior_Sigil_of_Paralysation2);
		expect(code.WeaponSet2.MainHand).toBe(WeaponType.Scepter);
		expect(code.WeaponSet2.Sigil1).toBe(ItemId.Superior_Sigil_of_Paralysation2);
		expect(code.WeaponSet2.OffHand).toBe(WeaponType.Focus  );
		expect(code.WeaponSet2.Sigil2).toBe(ItemId.Superior_Sigil_of_Paralysation2);

		expect(code.Rune).toBe(ItemId.Superior_Rune_of_the_Scholar);
		const berserkers = [StatId.Berserkers1, StatId.Berserkers2, StatId.Berserkers3, StatId.Berserkers4, StatId.Berserkers5];
		for(let i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			expect(berserkers).toContain(code.EquipmentAttributes[i]);
		}

		for(let i = 0; i < Static.ALL_INFUSION_COUNT; i++) {
			expect(code.Infusions[i]).toBe(ItemId.Mighty_5_Agony_Infusion);
		}

		expect(code.Specializations[0].SpecializationId).toBe(SpecializationId.Spite);
		const reference1 = new TraitLineChoices();
		reference1.Adept       = TraitLineChoice.TOP;
		reference1.Master      = TraitLineChoice.MIDDLE;
		reference1.Grandmaster = TraitLineChoice.MIDDLE;
		expect(code.Specializations[0].Choices).toStrictEqual(reference1);

		expect(code.Specializations[1].SpecializationId).toBe(SpecializationId.Soul_Reaping);
		const reference2 = new TraitLineChoices();
		reference2.Adept       = TraitLineChoice.TOP;
		reference2.Master      = TraitLineChoice.TOP;
		reference2.Grandmaster = TraitLineChoice.MIDDLE;
		expect(code.Specializations[1].Choices).toStrictEqual(reference2);

		expect(code.Specializations[2].SpecializationId).toBe(SpecializationId.Reaper);
		const reference3 = new TraitLineChoices();
		reference3.Adept       = TraitLineChoice.MIDDLE;
		reference3.Master      = TraitLineChoice.TOP;
		reference3.Grandmaster = TraitLineChoice.BOTTOM;
		expect(code.Specializations[2].Choices).toStrictEqual(reference3);

		expect(code.SlotSkills[0]).toBe(SkillId.Your_Soul_Is_Mine);
		expect(code.SlotSkills[1]).toBe(SkillId.Well_of_Suffering1);
		expect(code.SlotSkills[2]).toBe(SkillId.Well_of_Darkness1);
		expect(code.SlotSkills[3]).toBe(SkillId.Signet_of_Spite);
		expect(code.SlotSkills[4]).toBe(SkillId.Summon_Flesh_Golem);

		expect(code.Food).toBe(ItemId.Bowl_of_Sweet_and_spicy_Butternut_Squash_Soup);
		expect(code.Utility).toBe(ItemId.Tin_of_Fruitcake);
	});
});

describe.each([true])('OfficialChatLinks', (lazyload) => {
	/** @test @dataProvider TrueFalseProvider */
	test('LoadOfficialLink', async () => {
		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Necromancer/*, true*/);

		const code = await TextLoader.LoadOfficialBuildCode(TestUtilities.CodesIngame["full-necro"]);
		expect(code.Profession).toBe(Profession.Necromancer);

		expect(code.WeaponSet1.MainHand).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet1.OffHand).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet2.MainHand).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet2.OffHand).toBe(WeaponType._UNDEFINED);

		expect(code.Specializations[0].SpecializationId).toBe(SpecializationId.Spite);
		const reference1 = new TraitLineChoices();
		reference1.Adept       = TraitLineChoice.TOP;
		reference1.Master      = TraitLineChoice.MIDDLE;
		reference1.Grandmaster = TraitLineChoice.MIDDLE;
		expect(code.Specializations[0].Choices).toStrictEqual(reference1);

		expect(code.Specializations[1].SpecializationId).toBe(SpecializationId.Soul_Reaping);
		const reference2 = new TraitLineChoices();
		reference2.Adept       = TraitLineChoice.TOP;
		reference2.Master      = TraitLineChoice.TOP;
		reference2.Grandmaster = TraitLineChoice.MIDDLE;
		expect(code.Specializations[1].Choices).toStrictEqual(reference2);

		expect(code.Specializations[2].SpecializationId).toBe(SpecializationId.Reaper);
		const reference3 = new TraitLineChoices();
		reference3.Adept       = TraitLineChoice.MIDDLE;
		reference3.Master      = TraitLineChoice.TOP;
		reference3.Grandmaster = TraitLineChoice.BOTTOM;
		expect(code.Specializations[2].Choices).toStrictEqual(reference3);

		expect(code.SlotSkills[0]).toBe(SkillId.Your_Soul_Is_Mine);
		expect(code.SlotSkills[1]).toBe(SkillId.Well_of_Suffering1);
		expect(code.SlotSkills[2]).toBe(SkillId.Well_of_Darkness1);
		expect(code.SlotSkills[3]).toBe(SkillId.Signet_of_Spite);
		expect(code.SlotSkills[4]).toBe(SkillId.Summon_Flesh_Golem);
	});

	test('WriteOfficialLink', async () => {
		const code = new BuildCode();
		code.Profession = Profession.Necromancer;
		const choices1 = new TraitLineChoices();
		choices1.Adept       = TraitLineChoice.TOP;
		choices1.Master      = TraitLineChoice.MIDDLE;
		choices1.Grandmaster = TraitLineChoice.MIDDLE;
		code.Specializations.Choice1 = new Specialization(SpecializationId.Spite, choices1);
		const choices2 = new TraitLineChoices();
		choices2.Adept       = TraitLineChoice.TOP;
		choices2.Master      = TraitLineChoice.TOP;
		choices2.Grandmaster = TraitLineChoice.MIDDLE;
		code.Specializations.Choice2 = new Specialization(SpecializationId.Soul_Reaping, choices2);
		const choices3 = new TraitLineChoices();
		choices3.Adept       = TraitLineChoice.MIDDLE;
		choices3.Master      = TraitLineChoice.TOP;
		choices3.Grandmaster = TraitLineChoice.BOTTOM;
		code.Specializations.Choice3 = new Specialization(SpecializationId.Reaper, choices3);
		
		code.SlotSkills.Heal     = SkillId.Your_Soul_Is_Mine;
		code.SlotSkills.Utility1 = SkillId.Well_of_Suffering1;
		code.SlotSkills.Utility2 = SkillId.Well_of_Darkness1;
		code.SlotSkills.Utility3 = SkillId.Signet_of_Spite;
		code.SlotSkills.Elite    = SkillId.Summon_Flesh_Golem;

		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Necromancer/*, true*/);

		const reference = TestUtilities.CodesIngame["full-necro2"];
		const result = await TextLoader.WriteOfficialBuildCode(code);
		expect(result).toBe(reference);
	});
});
