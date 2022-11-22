import { describe, expect, test } from "@jest/globals";
import BinaryLoader, { BitReader, BitWriter } from "../../include/ts/BinaryLoader";
import ItemId from "../../include/ts/Database/ItemIds";
import LazyLoadMode from "../../include/ts/Database/LazyLoadMode";
import PerProfessionData from "../../include/ts/Database/PerProfessionData";
import SkillId from "../../include/ts/Database/SkillIds";
import SpecializationId from "../../include/ts/Database/SpecializationIds";
import Static from "../../include/ts/Database/Static";
import StatId from "../../include/ts/Database/StatIds";
import { Arbitrary, BuildCode, Kind, Legend, PetId, Profession, ProfessionSpecific, RangerData, RevenantData, Specialization, TraitLineChoice, WeaponType } from "../../include/ts/Structures";
import { Assert, Base64Decode } from "../../include/ts/Util/Static";
import { TraitLineChoices } from "../../include/ts/Util/UtilStructs";
import TestUtilities from "./TestUtilities";

describe('FunctionTests', () => {
	test('DecodeByteValue', () => {
		const data = new Uint8Array([0b00110011]);
		const bitspan = new BitReader(data);
		expect(bitspan.DecodeNext(4)).toBe(3);
	});

	test('SuccessiveDecodeByteValue', () => {
		const data = new Uint8Array([0b0011_1100]);
		const bitspan = new BitReader(data);
		expect(bitspan.DecodeNext(4)).toBe( 3);
		expect(bitspan.DecodeNext(4)).toBe(12);
	});

	test('DecodeMultibyteValue', () => {
		const data = new Uint8Array([0b0000_0000, 0b0001_0000]);
		const bitspan = new BitReader(data);
		expect(bitspan.DecodeNext(12)).toBe(1);

		const data2 = new Uint8Array([0b1000_0000, 0b0000_0000]);
		const bitspan2 = new BitReader(data2);
		expect(bitspan2.DecodeNext(12)).toBe(0b1000_0000_0000);
	});

	test('DecodeMultibyteValue2', () => {
		const data = new Uint8Array([0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0010]);
		const bitspan = new BitReader(data);
		bitspan.BitPos = 7;
		expect(bitspan.DecodeNext(24)).toBe(1);
	});

	test('SuccessiveDecodeMultibyteValue', () => {
		const data = new Uint8Array([0b00110011, 0b00110001]);
		const bitspan = new BitReader(data);
		expect(bitspan.DecodeNext(12)).toBe(0b001100110011);
		expect(bitspan.DecodeNext(4)).toBe(1);
	});

	test('SuccessiveDecodeValueCrossByteBoundary', () => {
		const data = new Uint8Array([0b01010101, 0b10101010, 0b11110000]);
		const bitspan = new BitReader(data);
		expect(bitspan.DecodeNext(12)).toBe(0b010101011010);
		expect(bitspan.DecodeNext(6)).toBe(0b101011);
	});

	test('EatIfExpected', () => {
		const data = new Uint8Array([0b00000011, 0b00110001]);
		const bitspan = new BitReader(data);
		expect(bitspan.EatIfExpected(8, 5)).toBeFalsy();
		expect(bitspan.BitPos).toBe(0);
		expect(bitspan.EatIfExpected(0, 5)).toBeTruthy();
		expect(bitspan.BitPos).toBe(5);
	});

	test('WriteBits', () => {
		const bitStream = new BitWriter();
		bitStream.Write(3, 2);
		expect(bitStream.Data[0]).toBe(0b11000000);
	});

	test('WriteManyBits', () => {
		const bitStream = new BitWriter();
		bitStream.Write(3, 24);
		expect(bitStream.Data).toStrictEqual([0, 0, 0b00000011]);
	});

	test('SuccessiveWriteBits', () => {
		const bitStream = new BitWriter();
		bitStream.Write(3, 2);
		bitStream.Write(3, 2);
		expect(bitStream.Data[0]).toBe(0b11110000);
	});

	test('SuccessiveWriteBitsAcrossByteBoundry', () => {
		const bitStream = new BitWriter();
		bitStream.Write(3, 6);
		bitStream.Write(3, 4);
		expect(bitStream.Data[0]).toBe(0b00001100);
		expect(bitStream.Data[1]).toBe(0b11000000);
	});

	test('SuccessiveWriteManyBits', () => {
		const bitStream = new BitWriter();
		bitStream.Write(3, 20);
		bitStream.Write(3, 24);
		expect(bitStream.Data).toStrictEqual([0, 0, 0b00110000, 0, 0, 0b00110000]);
	});
});

function BitStringToBytes(data : string) : Uint8Array {
	let list : Array<number> = [];
	let counter = 0;
	let current = 0;
	for(const c of data.split('')) {
		switch(c)
		{
			case '0':
				current = current << 1;
				counter++;
				break;

			case '1':
				current = current << 1;
				current |= 1;
				counter++;
				break;

			default: if(c >= 'a' && c <= 'z' || (c >= 'A' && c <= 'Z')) {
					if(counter !== 0) Assert(false, "only on byte boundries");
					list.push(c.charCodeAt(0));
				}
				break;

		}

		if(counter === 8)
		{
			list.push(current);
			current = 0;
			counter = 0;
		}
	}

	if(counter > 0) {
		list.push((current << (8 - counter)) & 0xFF);
	}

	return new Uint8Array(list);
}

const TrueFalseProvider = [true, false];

describe('BasicCodeTests', () => {
	test('HelperWorking', () => {
		const rawCode0 = BitStringToBytes("b01010101");
		expect(rawCode0).toStrictEqual(new Uint8Array(['b'.charCodeAt(0), 0b01010101]));

		const rawCode1 = BitStringToBytes("b010001");
		expect(rawCode1).toStrictEqual(new Uint8Array(['b'.charCodeAt(0), 0b01000100]));

		const rawCode2 = BitStringToBytes("b0100_01");
		expect(rawCode2).toStrictEqual(new Uint8Array(['b'.charCodeAt(0), 0b01000100]));
	});

	test('ShouldThrowVersion', () => {
		const rawCode = new Uint8Array(['B'.charCodeAt(0), ...(new Array(79).fill(0x2))]);

		expect(() => {
			const code = BinaryLoader.LoadBuildCode(rawCode);
		}).toThrow();
	});

	test('ShouldThrowTooShort', () => {
		const rawCode = new Uint8Array(['d'.charCodeAt(0), 0x2]);
		expect(() => {
			const code = BinaryLoader.LoadBuildCode(rawCode);
		}).toThrow();
	});

	test.each(TrueFalseProvider)('MinimalPvPWithSkills', async (lazyload : boolean) => {
		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Guardian/*, true*/);

		const rawCode = BitStringToBytes(TestUtilities.CodesV2Binary["minimal-pvp-with-skills"]);
		const code = BinaryLoader.LoadBuildCode(rawCode);
		expect(code.Version).toBe(3);
		expect(code.Kind).toBe(Kind.PvP);
		expect(code.Profession).toBe(Profession.Guardian);
		for(let i = 0; i < 3; i++)
			expect(code.Specializations[i].SpecializationId).toBe(SpecializationId._UNDEFINED);
		expect(code.WeaponSet1.HasAny()).toBeFalsy();
		expect(code.WeaponSet2.HasAny()).toBeFalsy();
		for(let i = 0; i < 5; i++)
			expect(code.SlotSkills[i]).toBe(i + 1);
		expect(code.Rune).toBe(ItemId._UNDEFINED);
		for(let i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			if(i >= 11 && i <= 14) expect(code.EquipmentAttributes[i]).toBe(StatId._UNDEFINED);
			else if(i === Static.ALL_EQUIPMENT_COUNT - 1)  expect(code.EquipmentAttributes[i]).toBe(1);
			else expect(code.EquipmentAttributes[i]).toBe(StatId._UNDEFINED);
		}
		for(let i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			expect(code.Infusions[i]).toBe(ItemId._UNDEFINED);
		expect(code.Food).toBe(ItemId._UNDEFINED);
		expect(code.Utility).toBe(ItemId._UNDEFINED);
		expect(code.ProfessionSpecific).toBe(ProfessionSpecific.NONE.GetInstance());
		expect(code.Arbitrary)         .toBe(Arbitrary         .NONE.GetInstance());
	});

	test.each(TrueFalseProvider)('MinimalPvE', async (lazyload : boolean) => {
		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Guardian/*, true*/);

		const rawCode = BitStringToBytes(TestUtilities.CodesV2Binary["minimal-pve"]);
		const code = BinaryLoader.LoadBuildCode(rawCode);
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

	/** @test @dataProvider TrueFalseProvider  */
	test.each(TrueFalseProvider)('MinimalRanger', async (lazyload : boolean) => {
		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Ranger/*, true*/);

		const rawCode = BitStringToBytes(TestUtilities.CodesV2Binary["minimal-ranger"]);
		const code = BinaryLoader.LoadBuildCode(rawCode);
		expect(code.Profession).toBe(Profession.Ranger);
		expect(code.ProfessionSpecific).toBeInstanceOf(RangerData);
		const data = code.ProfessionSpecific as RangerData;
		expect(data.Pet1).toBe(PetId._UNDEFINED);
		expect(data.Pet2).toBe(PetId._UNDEFINED);
	});

	test.each(TrueFalseProvider)('MinimalRevenant', async (lazyload : boolean) => {
		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Revenant/*, true*/);

		const rawCode = BitStringToBytes(TestUtilities.CodesV2Binary["minimal-revenant"]);
		const code = BinaryLoader.LoadBuildCode(rawCode);
		expect(code.Profession).toBe(Profession.Revenant);
		expect(code.ProfessionSpecific).toBeInstanceOf(RevenantData);
		const data = code.ProfessionSpecific as RevenantData;
		expect(data.Legend1).toBe(Legend.SHIRO      );
		expect(data.Legend2).toBe(Legend._UNDEFINED );
		expect(data.AltUtilitySkill1).toBe(SkillId._UNDEFINED);
		expect(data.AltUtilitySkill2).toBe(SkillId._UNDEFINED);
		expect(data.AltUtilitySkill3).toBe(SkillId._UNDEFINED);
	});

	test.each(TrueFalseProvider)('LoopWriteMinimalRevenant', async (lazyload : boolean) => {
		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Revenant/*, true*/);

		const rawCode = BitStringToBytes(TestUtilities.CodesV2Binary["minimal-revenant"]);
		const code = BinaryLoader.LoadBuildCode(rawCode);

		const result = BinaryLoader.WriteCode(code);

		expect(result).toStrictEqual(rawCode);
	});
});

describe.each(TrueFalseProvider)('OfficialChatLinks', (lazyload : boolean) => {
	test('LoadOfficialLink', async () => {
		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Necromancer/*, true*/);

		const fullLink = TestUtilities.CodesIngame["full-necro"];
		const base64   = fullLink.slice(2, -1);
		const code = await BinaryLoader.LoadOfficialBuildCode(Base64Decode(base64));
		expect(code.Kind).not.toBe(Kind._UNDEFINED);
		expect(code.Profession).toBe(Profession.Necromancer);

		expect(code.WeaponSet1.MainHand).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet1.OffHand ).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet2.MainHand).toBe(WeaponType._UNDEFINED);
		expect(code.WeaponSet2.OffHand ).toBe(WeaponType._UNDEFINED);

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

		const buffer = await BinaryLoader.WriteOfficialBuildCode(code);

		const reference = TestUtilities.CodesIngame["full-necro2"];
		const referenceBase64 = reference.slice(2, -1);
		const referenceBytes = Base64Decode(referenceBase64);

		expect(buffer).toStrictEqual(referenceBytes);
	});

	// our very special boy spec
	test('LoadOfficiaRevlLink', async () => {
		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Revenant/*, true*/);

		const fullLink = TestUtilities.CodesIngame["partial-revenant"];
		const base64   = fullLink.slice(2, -1);
		const code = await BinaryLoader.LoadOfficialBuildCode(Base64Decode(base64));
		expect(code.Profession).toBe(Profession.Revenant);
		expect(code.SlotSkills[0]).toBe(SkillId.Empowering_Misery);
		expect(code.SlotSkills[1]).toBe(SkillId._UNDEFINED);
		expect(code.SlotSkills[2]).toBe(SkillId.Banish_Enchantment);
		expect(code.SlotSkills[3]).toBe(SkillId.Call_to_Anguish1);
		expect(code.SlotSkills[4]).toBe(SkillId._UNDEFINED);
	});
});
