import BinaryLoader from "./BinaryLoader";
import ItemId from "./Database/ItemIds";
import SpecializationId from "./Database/SpecializationIds";
import { ALL_EQUIPMENT_COUNT, ALL_INFUSION_COUNT, CURRENT_VERSION, ExistsAndIsTwoHanded, HasAttributeSlot, HasInfusionSlot, IsTwoHanded } from "./Database/Static";
import StatId from "./Database/StatIds";
import { Arbitrary, BuildCode, IArbitrary, IProfessionSpecific, Kind, Legend, PetId, Profession, ProfessionSpecific, RangerData, RevenantData, Specialization, TraitLineChoice, WeaponSet, WeaponType } from "./Structures";
import StringView from "./Util/StringView"
import { AllEquipmentInfusions, AllEquipmentStats, SpecializationChoices, TraitLineChoices } from "./Util/UtilStructs";
import { Assert, Base64Decode, Base64Encode } from "./Util/Static";

class TextLoader {
	public static CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-";
	public static INVERSE_CHARSET = [
		/*0x*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		/*1x*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		/*2x*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 62, -1, 63, -1, -1,
		/*3x*/ 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, -1, -1, -1, -1, -1, -1,
		/*4x*/ -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
		/*5x*/ 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, -1,
		/*6x*/ -1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
		/*7x*/ 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, -1, -1, -1, -1, -1,
	];

	public static DecodeAndAdvance(view : StringView, maxWidth : number = 1) : number
	{
		if(maxWidth === 1) return TextLoader.INVERSE_CHARSET[view.NextByte()];

		let value = 0;
		let width = 0;
		do {
			const c = view.NextChar();
			const mulShift = 6 * width; // shift by 6, 12, 18 = multiply by 64 64^2 64^3
			width++;
			if(c === '~') break;
			value += TextLoader.INVERSE_CHARSET[c.charCodeAt(0)] << mulShift;
		} while(width < maxWidth);

		return value;
	}

	/** Eats the token from view if it is the right one, otherwise does nothing. */
	public static EatToken(view : StringView, token : string) : boolean
	{
		if(view.Data[view.Pos] === token) {
			view.Pos++;
			return true; 
		}
		return false;
	}

	///#region hardstuck codes

	public static LoadBuildCode(text : string) : BuildCode {
		Assert(text.length > 10, "Code too short");

		if(text[0] === text[0].toLowerCase()) {
			new TextEncoder().encode()
			const binary = Base64Decode(text.slice(1).replace('-', '/'));
			const buffer = new Uint8Array(binary.length + 1);
			buffer[0] = text.charCodeAt(0);
			buffer.set(binary, 1);
			return BinaryLoader.LoadBuildCode(buffer);
		}

		const view = new StringView(text);
		const code = new BuildCode();
		code.Version    = TextLoader.DecodeAndAdvance(view);
		Assert(code.Version === CURRENT_VERSION, "Code version mismatch");
		code.Kind       = TextLoader.DecodeAndAdvance(view) as Kind;
		Assert(code.Kind !== Kind._UNDEFINED, "Code type not valid");
		code.Profession = (1 + TextLoader.DecodeAndAdvance(view)) as Profession;

		for(let i = 0; i < 3; i++) {
			if(!TextLoader.EatToken(view, '_')) {
				const id = TextLoader.DecodeAndAdvance(view, 2);
				const mixed = TextLoader.DecodeAndAdvance(view);
				const choices = new TraitLineChoices();
				for(let j = 0; j < 3; j++)
					choices[j] = (((mixed >> (4 - j * 2)) & 0b00000011)) as TraitLineChoice;
				code.Specializations[i] = new Specialization(id, choices);
			}
		}
		if(!TextLoader.EatToken(view, '~')) {
			code.WeaponSet1 = TextLoader.LoadWeaponSet(view);
			if(!TextLoader.EatToken(view, '~'))
				code.WeaponSet2 = TextLoader.LoadWeaponSet(view);
		}

		for(let i = 0; i < 5; i++)
			if(!TextLoader.EatToken(view, '_'))
				code.SlotSkills[i] = TextLoader.DecodeAndAdvance(view, 3);
		
		if(!TextLoader.EatToken(view, '_'))
			code.Rune = TextLoader.DecodeAndAdvance(view, 3);
		
		if(code.Kind !== Kind.PvP)
			code.EquipmentAttributes = TextLoader.LoadAllEquipmentStats(view, code);
		else
			code.EquipmentAttributes.Amulet = TextLoader.DecodeAndAdvance(view, 2);

		if(code.Kind !== Kind.PvP) {
			if(!TextLoader.EatToken(view, '~'))
				code.Infusions = TextLoader.LoadAllEquipmentInfusions(view, code);
			if(!TextLoader.EatToken(view, '_'))
				code.Food = TextLoader.DecodeAndAdvance(view, 3);
			if(!TextLoader.EatToken(view, '_'))
				code.Utility = TextLoader.DecodeAndAdvance(view, 3);
		}

		code.ProfessionSpecific = TextLoader.LoadProfessionSpecific(view, code.Profession);
		code.Arbitrary          = TextLoader.LoadArbitrary(view);
		return code;
	}

	private static LoadWeaponSet(text : StringView) : WeaponSet
	{
		const set = new WeaponSet();
		if(!TextLoader.EatToken(text, '_')) set.MainHand = (WeaponType._FIRST + TextLoader.DecodeAndAdvance(text)) as WeaponType;
		if(set.MainHand)
			if(!TextLoader.EatToken(text, '_')) set.Sigil1 = TextLoader.DecodeAndAdvance(text, 3);

		if(!ExistsAndIsTwoHanded(set.MainHand))
			if(!TextLoader.EatToken(text, '_')) set.OffHand = (WeaponType._FIRST + TextLoader.DecodeAndAdvance(text)) as WeaponType;
		if(set.OffHand || ExistsAndIsTwoHanded(set.MainHand))
			if(!TextLoader.EatToken(text, '_')) set.Sigil2 = TextLoader.DecodeAndAdvance(text, 3);
		return set;
	}

	private static LoadAllEquipmentStats(text : StringView, weaponRef : BuildCode) : AllEquipmentStats
	{
		const allData = new AllEquipmentStats();

		let repeatCount = 0;
		let data = StatId._UNDEFINED;
		for(let i = 0; i < ALL_EQUIPMENT_COUNT; i++) {
			if(!HasAttributeSlot(weaponRef, i)) continue;

			if(repeatCount === 0) {
				data = TextLoader.DecodeAndAdvance(text, 2);

				if(i === ALL_EQUIPMENT_COUNT - 1) repeatCount = 1;
				else repeatCount = TextLoader.DecodeAndAdvance(text);
			}

			allData[i] = data;
			repeatCount--;
		}
		return allData;
	}

	private static LoadAllEquipmentInfusions(text : StringView, weaponRef : BuildCode) : AllEquipmentInfusions
	{
		const allData = new AllEquipmentInfusions();

		let repeatCount = 0;
		let data = ItemId._UNDEFINED;
		for(let i = 0; i < ALL_INFUSION_COUNT; i++)
		{
			if(!HasInfusionSlot(weaponRef, i)) continue;

			if(repeatCount === 0) {
				data = TextLoader.EatToken(text, '_') ? ItemId._UNDEFINED : TextLoader.DecodeAndAdvance(text, 3);

				if(i === ALL_INFUSION_COUNT - 1) repeatCount = 1;
				else repeatCount = TextLoader.DecodeAndAdvance(text);
			}

			allData[i] = data;
			repeatCount--;
		}
		return allData;
	}

	private static LoadProfessionSpecific(text : StringView, profession : Profession) : IProfessionSpecific
	{
		switch(profession)
		{
			case Profession.Ranger: {
				const data = new RangerData();
				if(!TextLoader.EatToken(text, '~')) {
					if(!TextLoader.EatToken(text, '_'))
						data.Pet1 = TextLoader.DecodeAndAdvance(text, 2);
					if(!TextLoader.EatToken(text, '_'))
						data.Pet1 = TextLoader.DecodeAndAdvance(text, 2);
				}
				return data;
			}

			case Profession.Revenant: {
				const data = new RevenantData();
				data.Legend1 = TextLoader.DecodeAndAdvance(text) + Legend._FIRST;
				if(!TextLoader.EatToken(text, '_')) {
					data.Legend2 = TextLoader.DecodeAndAdvance(text) + Legend._FIRST;
					if(!TextLoader.EatToken(text, '_'))
						data.AltUtilitySkill1 = TextLoader.DecodeAndAdvance(text, 3);
					if(!TextLoader.EatToken(text, '_'))
						data.AltUtilitySkill2 = TextLoader.DecodeAndAdvance(text, 3);
					if(!TextLoader.EatToken(text, '_'))
						data.AltUtilitySkill3 = TextLoader.DecodeAndAdvance(text, 3);
				}
				return data;
			}

			default: return ProfessionSpecific.NONE.Instance;
		}
	}

	private static LoadArbitrary(text : StringView) : IArbitrary
	{
		//implement extensions here in the future
		return Arbitrary.NONE.Instance;
	}

	public static Encode(value : number, width : number) : string
	{
		let destination = '';
		let pos = 0;
		do
		{
			destination += TextLoader.CHARSET[value & 0b00111111];
			value = value >> 6;
			pos++;
		} while(value > 0);
		if(pos < width) destination += '~';
		return destination;
	}

	public static EncodeOrUnderscoreOnZero(value : number, encodeWidth : number) : string
	{
		return (value === 0) ? '_' : TextLoader.Encode(value, encodeWidth); 
	}

	public static WriteBuildCode(code : BuildCode) : string
	{
		let destination = '';

		destination += TextLoader.CHARSET[code.Version];
		destination += TextLoader.CHARSET[code.Kind];
		destination += TextLoader.CHARSET[code.Profession - 1];
		for(let i = 0; i < 3; i++) {
			const spec = code.Specializations[i];
			if(spec.SpecializationId === SpecializationId._UNDEFINED) destination += '_';
			else {
				destination += TextLoader.Encode(spec.SpecializationId, 2);
				destination += TextLoader.CHARSET[
					(spec.Choices[0] << 4) | (spec.Choices[1] << 2) | spec.Choices[2]
				];
			}
		}
		
		if(!code.WeaponSet1.HasAny()) destination += '~';
		else
		{
			destination += TextLoader.EncodeWeaponSet(code.WeaponSet1);

			if(!code.WeaponSet2.HasAny()) destination += '~';
			else
			{
				destination += TextLoader.EncodeWeaponSet(code.WeaponSet2);
			}
		}

		for(let i = 0; i < 5; i++)
			destination += TextLoader.EncodeOrUnderscoreOnZero(code.SlotSkills[i], 3);

			destination += TextLoader.EncodeOrUnderscoreOnZero(code.Rune, 3);

		if(code.Kind !== Kind.PvP) destination += TextLoader.EncodeStats(code);
		else destination += TextLoader.Encode(code.EquipmentAttributes.Amulet, 2);

		if(code.Kind !== Kind.PvP)
		{
			if(!code.Infusions.HasAny()) destination += '~';
			else destination += TextLoader.EncodeInfusions(code);

			destination += TextLoader.EncodeOrUnderscoreOnZero(code.Food, 3);
			destination += TextLoader.EncodeOrUnderscoreOnZero(code.Utility, 3);
		}

		destination += TextLoader.EncodeProfessionArbitrary(code.ProfessionSpecific);
		destination += TextLoader.EncodeArbitrary(code.Arbitrary);

		return destination;
	}

	private static EncodeWeaponSet(set : WeaponSet) : string
	{
		let destination = '';
		if(set.MainHand === WeaponType._UNDEFINED) destination += '_';
		else {
			destination += TextLoader.CHARSET[set.MainHand - WeaponType._FIRST];
			if(set.Sigil1 === ItemId._UNDEFINED) destination += '_';
			else destination += TextLoader.Encode(set.Sigil1, 3);
		}

		if(!ExistsAndIsTwoHanded(set.MainHand))
		{
			if(set.OffHand === WeaponType._UNDEFINED) destination += '_';
			else destination += TextLoader.CHARSET[set.OffHand - WeaponType._FIRST];
		}

		if(set.OffHand !== WeaponType._UNDEFINED || ExistsAndIsTwoHanded(set.MainHand))
		{
			if(set.Sigil2 === ItemId._UNDEFINED) destination += '_';
			else destination += TextLoader.Encode(set.Sigil2, 3);
		}

		return destination;
	}

	private static EncodeStats(weaponRef : BuildCode) : string
	{
		let destination = '';
		let lastStat : number|null = null;
		let repeatCount = 0;
		for(let i = 0; i < ALL_EQUIPMENT_COUNT; i++)
		{
			if(!HasAttributeSlot(weaponRef, i)) continue;

			if(weaponRef.EquipmentAttributes[i] !== lastStat)
			{
				if(lastStat !== null)
				{
					destination += TextLoader.Encode(lastStat, 2);
					destination += TextLoader.CHARSET[repeatCount];
				}

				lastStat = weaponRef.EquipmentAttributes[i];
				repeatCount = 1;
			}
			else
			{
				repeatCount++;
			}
		}

		destination += TextLoader.Encode(lastStat as number, 2);
		if(repeatCount > 1)
			destination += TextLoader.CHARSET[repeatCount];

		return destination;
	}

	private static EncodeInfusions(weaponRef : BuildCode) : string
	{
		let destination = '';
		let lastInfusion : ItemId|null = null;
		let repeatCount = 0;
		for(let i = 0; i < ALL_INFUSION_COUNT; i++)
		{
			if(!HasInfusionSlot(weaponRef, i)) continue;

			if(weaponRef.Infusions[i] !== lastInfusion)
			{
				if(lastInfusion !== null)
				{
					destination += TextLoader.EncodeOrUnderscoreOnZero(lastInfusion, 3);
					destination += TextLoader.CHARSET[repeatCount];
				}

				lastInfusion = weaponRef.Infusions[i];
				repeatCount = 1;
			}
			else
			{
				repeatCount++;
			}
		}

		destination += TextLoader.EncodeOrUnderscoreOnZero(lastInfusion || ItemId._UNDEFINED, 2);
		if(repeatCount > 1)
			destination += TextLoader.CHARSET[repeatCount];

		return destination;
	}

	private static EncodeProfessionArbitrary(professionSpecific : IProfessionSpecific) : string
	{
		let destination = '';
		if(professionSpecific instanceof RangerData)
		{
			if(professionSpecific.Pet1 === PetId._UNDEFINED && professionSpecific.Pet2 === PetId._UNDEFINED) destination += '~';
			else
			{
				destination += TextLoader.EncodeOrUnderscoreOnZero(professionSpecific.Pet1, 2);
				destination += TextLoader.EncodeOrUnderscoreOnZero(professionSpecific.Pet2, 2);
			}
		} else if(professionSpecific instanceof RevenantData) {
			destination += TextLoader.CHARSET[professionSpecific.Legend1 - Legend._FIRST];
			if(professionSpecific.Legend2 === Legend._UNDEFINED) destination += '_';
			else
			{
				destination += TextLoader.CHARSET[professionSpecific.Legend2 - Legend._FIRST];
				destination += TextLoader.EncodeOrUnderscoreOnZero(professionSpecific.AltUtilitySkill1, 3);
				destination += TextLoader.EncodeOrUnderscoreOnZero(professionSpecific.AltUtilitySkill2, 3);
				destination += TextLoader.EncodeOrUnderscoreOnZero(professionSpecific.AltUtilitySkill3, 3);
			}
		}

		return destination;
	}

	private static EncodeArbitrary(arbitraryData : IArbitrary) : string
	{
		//space for expansions
		return '';
	}

	//#endregion

	//#region official codes

	/**
	 * @param chatLink base64 encoded raw link (without [&...]) or full link (with [&...])
	 * @remarks Requires PerProfessionData to be loaded or PerProfessionData.LazyLoadMode to be set to something other than LazyLoadMode.NONE.
	 */
	public static LoadOfficialBuildCode(chatLink : string , aquatic : boolean = false) : Promise<BuildCode>
	{
		const base64 = chatLink[0] === '[' ? chatLink.slice(2, -1) : chatLink;
		return BinaryLoader.LoadOfficialBuildCode(Base64Decode(base64), aquatic);
	}

	/** @remarks Requires PerProfessionData to be loaded or PerProfessionData.LazyLoadMode to be set to something other than LazyLoadMode.NONE. */
	public static async WriteOfficialBuildCode(code : BuildCode, aquatic : boolean = false) : Promise<string>
	{
		const buffer = await BinaryLoader.WriteOfficialBuildCode(code, aquatic);
		return "[&"+Base64Encode(buffer)+']';
	}

	//#endregion
}

export default TextLoader;