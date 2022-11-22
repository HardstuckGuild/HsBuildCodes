import ItemId from "./Database/ItemIds";
import LazyLoadMode from "./Database/LazyLoadMode";
import PerProfessionData from "./Database/PerProfessionData";
import SpecializationId from "./Database/SpecializationIds";
import Overrides from "./Database/Overrides";
import Static from "./Database/Static";
import StatId from "./Database/StatIds";
import { Arbitrary, BuildCode, IArbitrary, IProfessionSpecific, Kind, Legend, PetId, Profession, ProfessionSpecific, RangerData, RevenantData, Specialization, TraitLineChoice, WeaponSet, WeaponType } from "./Structures";
import { AllEquipmentInfusions, AllEquipmentStats, TraitLineChoices } from "./Util/UtilStructs";
import BinaryView from "./Util/BinaryView";
import { Assert } from "./Util/Static";

//TODO(Rennorb): rewrite these using ArrayBuffers
export class BitReader {
	public Data   : Uint8Array;
	public BitPos : number;

	public constructor(data : Uint8Array) {
		this.Data   = data;
		this.BitPos = 0;
	}

	//NOTE(Rennorb): its more efficient to not use this, but its really handy
	public EatIfExpected(expected : number, width : number) : boolean
	{
		const val = this.DecodeNext(width);
		if(val !== expected) this.BitPos -= width;
		return val === expected;
	}

	public DecodeNext_GetMinusMinIfAtLeast(min : number, width : number) : number
	{
		const id = this.DecodeNext(width);
		const actualValue = id - min;
		if(id >= min) return actualValue;
		return 0;
	}

	public DecodeNext(width : number) : number
	{
		//NOTE(Rennorb): this is way overcompluicated
		//TODO(Rennorb): @cleanup refactor this mess

		const sourceByteStart = this.BitPos >> 3;
		const sourceByteWidth = ((this.BitPos & 7) + width + 7) >> 3;
		//sadly required because  BinaryPrimitives.ReadInt32BigEndian always wants to decode 4 bytes
		const containsData = [0, 0, 0, 0];
		const additionalBytes = containsData.length - sourceByteWidth;
		for(let i = 0; i < sourceByteWidth; i++)
			containsData[additionalBytes + i] = this.Data[sourceByteStart + i];
		const bitShiftRight = 8 - (this.BitPos + width) & 7;
		const bitShiftLeft = ((this.BitPos & 7) - (8 - (width & 7))) & 7;
		for(let i = 3; i > additionalBytes; i--)
		{
			containsData[i] = containsData[i] >> bitShiftRight;
			containsData[i] |= (containsData[i - 1] << bitShiftLeft) & 0xFF;
		}
		const firstTargetByte = containsData.length - ((width + 7) >> 3);
		if(firstTargetByte !== additionalBytes) containsData[additionalBytes] = 0;
		else containsData[additionalBytes] = containsData[additionalBytes] >> bitShiftRight;
		containsData[firstTargetByte] &= ((1 << width) - 1) & 0xFF;

		this.BitPos += width;

		const data = new DataView(new Uint8Array(containsData).buffer);
		return data.getUint32(0);
	}

	public DebugPrint() : string
	{
		let s = "";
		for(let i = 0; i < this.Data.length; i++)
		{
			if(i > 0 && i % 8 === 0) s += "| ";

			if(i === this.BitPos / 8)
				s += "_["+this.Data[i]!.toString(2).padStart(8, '0')+']';
			else s += '_'+this.Data[i]!.toString(2).padStart(8, '0');
		}
		return s;
	}
}

export class BitWriter {
	public Data   : number[]  = [];
	public BitPos : number = 0;

	public Write(value : number, bitWidth : number) : void
	{
		const posInByte = this.BitPos & 7;
		const bytesTouched = (posInByte + bitWidth + 7) >> 3;

		const buffer = new ArrayBuffer(4);
		const view = new DataView(buffer);
		view.setUint32(0, value << (32 - bitWidth - posInByte));

		const dest = this.BitPos >> 3;
		for(let i = 0; i < bytesTouched; i++)
			this.Data[dest + i] = (this.Data[dest + i] !== undefined ? this.Data[dest + i] : 0) | view.getUint8(i);

		this.BitPos += bitWidth;
	}
	
	public DebugPrint() : string
	{
		let s = "";
		for(let i = 0; i < this.Data.length; i++)
		{
			if(i > 0 && i % 8 === 0) s += "| ";

			if(i === this.BitPos / 8)
				s += "_["+this.Data[i].toString(2).padStart(8, '0')+']';
			else s += '_'+this.Data[i].toString(2).padStart(8, '0');
		}
		return s;
	}
}

class BinaryLoader {
	//#region hardstuck codes

	public static LoadBuildCode(raw : Uint8Array) : BuildCode
	{
		Assert(raw.length > 10, "Too short.");
		const rawSpan = new BitReader(raw);

		const code = new BuildCode();
		code.Version = rawSpan.DecodeNext(8) - 'a'.charCodeAt(0);
		Assert(code.Version >= Static.FIRST_VERSIONED_VERSION && code.Version <= Static.CURRENT_VERSION, "Code version mismatch");
		switch (rawSpan.DecodeNext(2)) {
			case 0: code.Kind = Kind.PvP; break;
			case 1: code.Kind = Kind.WvW; break;
			case 2: code.Kind = Kind.PvE; break;
			default: code.Kind = Kind._UNDEFINED;
		};
		Assert(code.Kind !== Kind._UNDEFINED, "Code type not valid");
		code.Profession = (1 + rawSpan.DecodeNext(4)) as Profession;

		for(let i = 0; i < 3; i++) {
			const traitLine = rawSpan.DecodeNext(7);
			if(traitLine !== SpecializationId._UNDEFINED) {
				const choices = new TraitLineChoices();
				for(let j = 0; j < 3; j++)
					choices[j] = rawSpan.DecodeNext(2) as TraitLineChoice;
				code.Specializations[i] = new Specialization(traitLine, choices);
			}
		}
		if(!rawSpan.EatIfExpected(0, 5)) {
			code.WeaponSet1 = BinaryLoader.LoadWeaponSet(rawSpan);
			if(!rawSpan.EatIfExpected(0, 5))
				code.WeaponSet2 = BinaryLoader.LoadWeaponSet(rawSpan);
		}
		for(let i = 0; i < 5; i++)
			code.SlotSkills[i] = rawSpan.DecodeNext(24);

		code.Rune = rawSpan.DecodeNext(24);
		
		if(code.Kind !== Kind.PvP)
			code.EquipmentAttributes = BinaryLoader.LoadAllEquipmentStats(rawSpan, code);
		else
			code.EquipmentAttributes.Amulet = rawSpan.DecodeNext(16);

		if(code.Kind !== Kind.PvP) {
			if(!rawSpan.EatIfExpected(0, 24))
				code.Infusions = BinaryLoader.LoadAllEquipmentInfusions(rawSpan, code);

			code.Food    = rawSpan.DecodeNext(24);
			code.Utility = rawSpan.DecodeNext(24);
		}
		code.ProfessionSpecific = BinaryLoader.LoadProfessionSpecific(rawSpan, code.Profession);
		code.Arbitrary          = BinaryLoader.LoadArbitrary(rawSpan);

		return code;
	}

	private static LoadWeaponSet(rawSpan : BitReader) : WeaponSet
	{
		const set = new WeaponSet();
		set.MainHand = (WeaponType._FIRST + rawSpan.DecodeNext_GetMinusMinIfAtLeast(2, 5)) as WeaponType;
		set.Sigil1 = rawSpan.DecodeNext(24);
		if(set.MainHand !== WeaponType._UNDEFINED && !Static.IsTwoHanded(set.MainHand))
			set.OffHand = (WeaponType._FIRST + rawSpan.DecodeNext_GetMinusMinIfAtLeast(2, 5)) as WeaponType;
		set.Sigil2 = rawSpan.DecodeNext(24);
		return set;
	}

	private static LoadAllEquipmentStats(rawSpan : BitReader, weaponRef : BuildCode) : AllEquipmentStats
	{
		const allData = new AllEquipmentStats();

		let repeatCount = 0;
		let data = StatId._UNDEFINED;
		for(let i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			if(repeatCount === 0) {
				data = rawSpan.DecodeNext(16);

				if(i === Static.ALL_EQUIPMENT_COUNT - 1) repeatCount = 1;
				else repeatCount = rawSpan.DecodeNext(4) + 1;
			}

			switch(i) {
				case 11:
					if(!weaponRef.WeaponSet1.HasAny()) { i += 3; continue; }
					else if(weaponRef.WeaponSet1.MainHand === WeaponType._UNDEFINED) { continue; }
					else break;
				case 12:
					if(weaponRef.WeaponSet1.OffHand === WeaponType._UNDEFINED) continue;
					else break;
				case 13:
					if(!weaponRef.WeaponSet2.HasAny()) { i++; continue; }
					else if(weaponRef.WeaponSet2.MainHand === WeaponType._UNDEFINED) continue;
					else break;
				case 14:
					if(weaponRef.WeaponSet2.OffHand === WeaponType._UNDEFINED) continue;
					else break;
			}

			allData[i] = data;
			repeatCount--;
		}
		return allData;
	}

	private static LoadAllEquipmentInfusions(rawSpan : BitReader, weaponRef : BuildCode) : AllEquipmentInfusions
	{
		const allData = new AllEquipmentInfusions();

		let repeatCount = 0;
		let data = ItemId._UNDEFINED;
		for(let i = 0; i < Static.ALL_INFUSION_COUNT; i++) {
			if(repeatCount === 0) {
				data = rawSpan.DecodeNext_GetMinusMinIfAtLeast(1, 24);

				if(i === Static.ALL_EQUIPMENT_COUNT - 1) repeatCount = 1;
				else repeatCount = rawSpan.DecodeNext(5) + 1;
			}

			switch(i) {
				case 16:
					if(!weaponRef.WeaponSet1.HasAny()) { i += 3; continue; }
					else if(weaponRef.WeaponSet1.MainHand === WeaponType._UNDEFINED) { continue; }
					else break;
				case 17:
					if(weaponRef.WeaponSet1.OffHand === WeaponType._UNDEFINED) continue;
					else break;
				case 18:
					if(!weaponRef.WeaponSet2.HasAny()) { i++; continue; }
					else if(weaponRef.WeaponSet2.MainHand === WeaponType._UNDEFINED) continue;
					else break;
				case 19:
					if(weaponRef.WeaponSet2.OffHand === WeaponType._UNDEFINED) continue;
					else break;
			}

			allData[i] = data;
			repeatCount--;
		}
		return allData;
	}

	private static LoadProfessionSpecific(rawSpan : BitReader, profession : Profession) : IProfessionSpecific
	{
		switch(profession)
		{
			case Profession.Ranger: {
				const data = new RangerData();
				if(!rawSpan.EatIfExpected(0, 7)) {
					data.Pet1 = rawSpan.DecodeNext_GetMinusMinIfAtLeast(2, 7);
					data.Pet2 = rawSpan.DecodeNext_GetMinusMinIfAtLeast(2, 7);
				}
				return data;
			}

			case Profession.Revenant: {
				const data = new RevenantData();
				data.Legend1 = rawSpan.DecodeNext(4);
				if(!rawSpan.EatIfExpected(0, 4)){
					data.Legend2 = rawSpan.DecodeNext(4);
					data.AltUtilitySkill1 = rawSpan.DecodeNext_GetMinusMinIfAtLeast(1, 24);
					data.AltUtilitySkill2 = rawSpan.DecodeNext_GetMinusMinIfAtLeast(1, 24);
					data.AltUtilitySkill3 = rawSpan.DecodeNext_GetMinusMinIfAtLeast(1, 24);
				}
				return data;
			}

			default: return ProfessionSpecific.NONE.GetInstance();
		}
	}

	private static LoadArbitrary(rawSpan : BitReader) : IArbitrary
	{
		//implement extensions here in the future
		return Arbitrary.NONE.GetInstance();
	}

	public static WriteCode(code : BuildCode) : Uint8Array
	{
		const rawBits = new BitWriter();
		rawBits.Data[0] = 'a'.charCodeAt(0) + code.Version;
		rawBits.BitPos += 8;
		
		switch (code.Kind) {
			case Kind.PvP: rawBits.Write(0, 2); break;
			case Kind.WvW: rawBits.Write(1, 2); break;
			case Kind.PvE: rawBits.Write(2, 2); break;
			default: throw new Error("invalid value in code.Kind");
		}
		
		rawBits.Write(code.Profession - 1, 4);

		for(let i = 0; i < 3; i++)
		{
			if(code.Specializations[i].SpecializationId === SpecializationId._UNDEFINED) rawBits.Write(0, 7);
			else
			{
				rawBits.Write(code.Specializations[i].SpecializationId, 7);
				for(let j = 0; j < 3; j++)
					rawBits.Write(code.Specializations[i].Choices[j].value, 2);
			}
		}

		if(!code.WeaponSet1.HasAny() && !code.WeaponSet2.HasAny()) rawBits.Write(0, 5);
		else
		{
			rawBits.Write(1 + code.WeaponSet1.MainHand, 5);
			rawBits.Write(code.WeaponSet1.Sigil1, 24);
			rawBits.Write(1 + code.WeaponSet1.OffHand, 5);
			rawBits.Write(code.WeaponSet1.Sigil2, 24);

			if(!code.WeaponSet2.HasAny()) rawBits.Write(0, 5);
			else
			{
				rawBits.Write(1 + code.WeaponSet2.MainHand, 5);
				rawBits.Write(code.WeaponSet2.Sigil1, 24);
				rawBits.Write(1 + code.WeaponSet2.OffHand, 5);
				rawBits.Write(code.WeaponSet2.Sigil2, 24);
			}
		}

		for(let i = 0; i < 5; i++)
			rawBits.Write(code.SlotSkills[i], 24);

		rawBits.Write(code.Rune, 24);

		if(code.Kind === Kind.PvP) rawBits.Write(code.EquipmentAttributes.Amulet, 16);
		else
		{
			{
				let lastStat : StatId|null = null;
				let repeatCount = 0;
				for(let i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++)
				{
					switch(i)
					{
						case 11:
							if(!code.WeaponSet1.HasAny()) { i += 3; continue; }
							else if(code.WeaponSet1.MainHand === WeaponType._UNDEFINED) { continue; }
							else break;
						case 12:
							if(code.WeaponSet1.OffHand === WeaponType._UNDEFINED) continue;
							else break;
						case 13:
							if(!code.WeaponSet2.HasAny()) { i++; continue; }
							else if(code.WeaponSet2.MainHand === WeaponType._UNDEFINED) continue;
							else break;
						case 14:
							if(code.WeaponSet2.OffHand === WeaponType._UNDEFINED) continue;
							else break;
					}

					if(code.EquipmentAttributes[i] !== lastStat)
					{
						if(lastStat !== null)
						{
							rawBits.Write(lastStat, 16);
							rawBits.Write(repeatCount - 1, 4);
						}

						lastStat = code.EquipmentAttributes[i];
						repeatCount = 1;
					}
					else
					{
						repeatCount++;
					}
				}

				rawBits.Write(lastStat as StatId, 16);
				if(repeatCount > 1)
					rawBits.Write(repeatCount - 1, 4);
			}

			if(!code.Infusions.HasAny()) rawBits.Write(0, 24);
			else
			{
				let lastInfusion = ItemId._UNDEFINED;
				let repeatCount = 0;
				for(let i = 0; i < Static.ALL_INFUSION_COUNT; i++)
				{
					switch(i)
					{
						case 16:
							if(!code.WeaponSet1.HasAny()) { i += 3; continue; }
							else if(code.WeaponSet1.MainHand === WeaponType._UNDEFINED) { continue; }
							else break;
						case 17:
							if(code.WeaponSet1.OffHand === WeaponType._UNDEFINED) continue;
							else break;
						case 18:
							if(!code.WeaponSet2.HasAny()) { i++; continue; }
							else if(code.WeaponSet2.MainHand === WeaponType._UNDEFINED) continue;
							else break;
						case 19:
							if(code.WeaponSet2.OffHand === WeaponType._UNDEFINED) continue;
							else break;
					}

					if(code.Infusions[i] !== lastInfusion)
					{
						if(lastInfusion !== ItemId._UNDEFINED)
						{
							rawBits.Write(lastInfusion + 1, 24);
							rawBits.Write(repeatCount - 1, 5);
						}

						lastInfusion = code.Infusions[i];
						repeatCount = 1;
					}
					else
					{
						repeatCount++;
					}
				}

				rawBits.Write(lastInfusion + 1, 24);
				if(repeatCount > 1)
					rawBits.Write(repeatCount - 1, 5);
			}

			rawBits.Write(code.Food, 24);
			rawBits.Write(code.Utility, 24);
		}

		switch(code.Profession)
		{
			case Profession.Ranger:
				const rangerData = code.ProfessionSpecific as RangerData;
				if(rangerData.Pet1 === PetId._UNDEFINED && rangerData.Pet2 === PetId._UNDEFINED) rawBits.Write(0, 7);
				else
				{
					rawBits.Write(1 + rangerData.Pet1, 7);
					rawBits.Write(1 + rangerData.Pet2, 7);
				}
				break;

			case Profession.Revenant:
				const revenantData = code.ProfessionSpecific as RevenantData;
				rawBits.Write(revenantData.Legend1, 4);
				if(revenantData.Legend2 === Legend._UNDEFINED) rawBits.Write(0, 4);
				else
				{
					rawBits.Write(revenantData.Legend2, 4);
					rawBits.Write(revenantData.AltUtilitySkill1, 24);
					rawBits.Write(revenantData.AltUtilitySkill2, 24);
					rawBits.Write(revenantData.AltUtilitySkill3, 24);
				}
				break;
		}

		return new Uint8Array(rawBits.Data);
	}

	//#endregion

	//#region official codes

	/**
	 * @param raw binary data
	 * @remarks Requires PerProfessionData to be loaded or PerProfessionData.LazyLoadMode to be set to something other than LazyLoadMode.NONE.
	 */
	public static async LoadOfficialBuildCode(raw : Uint8Array, aquatic : boolean = false) : Promise<BuildCode>
	{
		const rawView = new BinaryView(raw.buffer);
		const codeType = rawView.NextByte();
		Assert(codeType === 0x0D);

		const code = new BuildCode();
		code.Version    = Static.CURRENT_VERSION;
		code.Kind       = Kind.PvE;
		code.Profession = rawView.NextByte() as Profession;

		if(PerProfessionData.LazyLoadMode >= LazyLoadMode.OFFLINE_ONLY) await PerProfessionData.Reload(code.Profession/*, PerProfessionData.LazyLoadMode < LazyLoadMode.FULL*/);
		const professionData = PerProfessionData.ByProfession(code.Profession);

		for(let i = 0; i < 3; i++) {
			const spec = rawView.NextByte();
			const mix = rawView.NextByte();
			if(spec === SpecializationId._UNDEFINED) continue;

			const choices = new TraitLineChoices();
			for(let j = 0; j < 3; j++) {
				choices[j] = ((mix >> (j * 2)) & 0b00000011) as TraitLineChoice;
			}
			code.Specializations[i] = new Specialization(spec, choices);
		}

		const offset = aquatic ? 2 : 0;
		const specRaw = rawView.Slice(5 * 4);
		rawView.Pos += offset;

		switch(code.Profession)
		{
			case Profession.Ranger:
				specRaw.Pos += offset;

				const rangerData = new RangerData();
				if(specRaw.ByteAt(0) !== 0) rangerData.Pet1 = specRaw.ByteAt(0);
				if(specRaw.ByteAt(1) !== 0) rangerData.Pet2 = specRaw.ByteAt(1);

				code.ProfessionSpecific = rangerData;
				break;

			case Profession.Revenant:
				specRaw.Pos += offset;

				const revenantData = new RevenantData();
				
				if(specRaw.ByteAt(0) !== 0)
				{
					revenantData.Legend1 = specRaw.ByteAt(0) - Legend._FIRST;

					for(let i = 0; i < 5; i++) {
						const palletteId = rawView.NextUShortLE();
						rawView.Pos += 2;
						code.SlotSkills[i] = Overrides.RevPalletteToSkill(revenantData.Legend1, palletteId);
					}
				}
				else
				{
					//NOTE(Rennorb): no legend available, here we can only guess the right skils.
					BinaryLoader.ReadSlotSkillsNormally(code, professionData, rawView);
				}

				if(specRaw.ByteAt(1) !== 0)
				{
					revenantData.Legend2 = specRaw.ByteAt(1) - Legend._FIRST;
					const revSkillOffset = aquatic ? 6: 2;
					specRaw.Pos += revSkillOffset;

					const altSkills = [
						Overrides.RevPalletteToSkill(revenantData.Legend2, specRaw.NextUShortLE()),
						Overrides.RevPalletteToSkill(revenantData.Legend2, specRaw.NextUShortLE()),
						Overrides.RevPalletteToSkill(revenantData.Legend2, specRaw.NextUShortLE()),
					];

					if(specRaw.ByteAt(-6) !== 0)
					{
						if(altSkills[0] !== 0) revenantData.AltUtilitySkill1 = altSkills[0];
						if(altSkills[1] !== 0) revenantData.AltUtilitySkill2 = altSkills[1];
						if(altSkills[2] !== 0) revenantData.AltUtilitySkill3 = altSkills[2];
					}
					else //flip skills so the first legend is always set
					{
						revenantData.Legend1 = revenantData.Legend2;
						revenantData.Legend2 = Legend._UNDEFINED;

						revenantData.AltUtilitySkill1 = code.SlotSkills.Utility1;
						revenantData.AltUtilitySkill2 = code.SlotSkills.Utility2;
						revenantData.AltUtilitySkill3 = code.SlotSkills.Utility3;

						for(let i = 0; i < 3; i++)
							if(altSkills[i] !== 0)
								code.SlotSkills[1 + i] = altSkills[i];
					}
				}

				code.ProfessionSpecific = revenantData;
				return code;
		}

		BinaryLoader.ReadSlotSkillsNormally(code, professionData, rawView);

		return code;
	}

	private static ReadSlotSkillsNormally(code : BuildCode, skillData : PerProfessionData, raw : BinaryView) : void
	{
		for(let i = 0; i < 5; i++) {
			const palletteId = raw.NextUShortLE();
			if(palletteId !== 0) code.SlotSkills[i] = skillData.PalletteToSkill[palletteId];
			raw.Pos += 2;
		}
	}

	/** @remarks Requires PerProfessionData to be loaded or PerProfessionData.LazyLoadMode to be set to something other than LazyLoadMode.NONE. */
	public static async WriteOfficialBuildCode(code : BuildCode, aquatic : boolean = false) : Promise<Uint8Array>
	{
		const destination = new Uint8Array(44);
		let pos = 0;
		function WriteByte(byte : number) {
			destination[pos++] = byte;
		}
		
		if(PerProfessionData.LazyLoadMode >= LazyLoadMode.OFFLINE_ONLY) await PerProfessionData.Reload(code.Profession/*, PerProfessionData.LazyLoadMode < LazyLoadMode.FULL*/);
		const professionData = PerProfessionData.ByProfession(code.Profession);

		WriteByte(0x0d); //code type
		WriteByte(code.Profession);
		for(let i = 0; i < 3; i++) {
			if(code.Specializations[i].SpecializationId === SpecializationId._UNDEFINED) continue;

			const spec = code.Specializations[i];
			WriteByte(spec.SpecializationId);
			WriteByte((spec.Choices[0] | (spec.Choices[1] << 2) | (spec.Choices[2] << 4)));
		}

		{
			if(aquatic) pos += 2;
			var view = new DataView(destination.buffer, pos);
			for(let i = 0; i < 5; i++) {
				const palletteIndex = professionData.SkillToPallette[code.SlotSkills[i]];
				view.setUint16(i * 4, palletteIndex, true);
			}
			if(!aquatic) pos += 2;
		}

		switch(code.Profession)
		{
			case Profession.Ranger:
				const rangerData = code.ProfessionSpecific as RangerData;
				if(aquatic) pos += 2;
				WriteByte(1 + rangerData.Pet1);
				WriteByte(1 + rangerData.Pet2);
				break;

			case Profession.Revenant:
				const revenantData = code.ProfessionSpecific as RevenantData;

				if(aquatic) pos += 2;
				WriteByte(revenantData.Legend1);
				WriteByte(revenantData.Legend2);

				if(aquatic) pos += 6;
				else pos += 2;

				const altSkill1PalletteId = professionData.SkillToPallette[revenantData.AltUtilitySkill1];
				const altSkill2PalletteId = professionData.SkillToPallette[revenantData.AltUtilitySkill2];
				const altSkill3PalletteId = professionData.SkillToPallette[revenantData.AltUtilitySkill3];

				{
					var view = new DataView(destination.buffer, pos);
					view.setUint16(0, altSkill1PalletteId, true);
					view.setUint16(2, altSkill2PalletteId, true);
					view.setUint16(4, altSkill3PalletteId, true);
				}

				if(!aquatic) pos += 6;
				break;
		}

		return destination;
	}

	//#endregion
}

export default BinaryLoader;