using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Diagnostics;

using static Hardstuck.GuildWars2.BuildCodes.V2.Static;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class TextLoader {
	public static readonly string CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-";
	public static readonly int[] INVERSE_CHARSET = new[] {
		/*0x*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		/*1x*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		/*2x*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 62, -1, 63, -1, -1,
		/*3x*/ 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, -1, -1, -1, -1, -1, -1,
		/*4x*/ -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
		/*5x*/ 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, -1,
		/*6x*/ -1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
		/*7x*/ 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, -1, -1, -1, -1, -1,
	};

	public static int DecodeAndAdvance(ref ReadOnlySpan<char> text, int maxWidth)
	{
		int value = 0;
		var width = 0;
		do {
			var c = text[width];
			var mulShift = 6 * width; // shift by 6, 12, 18 = multiply by 64 64^2 64^3
			width++;
			if(c == '~') break;
			value += INVERSE_CHARSET[c] << mulShift;
		} while(width < maxWidth);

		text = text[width..];

		return value;
	}

	public static int DecodeAndAdvance(ref ReadOnlySpan<char> text) 
		=> INVERSE_CHARSET[Util.Static.SliceAndAdvance(ref text)];

	/// <summary> eats the <paramref name="token"/> from <paramref name="text"/> if it is the right one. otherwise does nothing</summary>
	public static bool EatToken(ref ReadOnlySpan<char> text, char token)
	{
		if(text[0] == token) {
			text = text[1..];
			return true; 
		}
		return false;
	}

	#region hardstuck codes

	public static BuildCode LoadBuildCode(ReadOnlySpan<char> text) {

		if(char.IsLower(text[0])) {
			Span<char> base64 = stackalloc char[text.Length - 1];
			text[1..].CopyTo(base64);
			for(int i = 0; i < base64.Length; i++) if(base64[i] == '-') base64[i] = '/';

			Span<byte> buffer = stackalloc byte[base64.Length]; // raw will always be shorter than the base64 version
			buffer[0] = (byte)text[0];
			Convert.TryFromBase64Chars(base64, buffer[1..], out var len);
			return BinaryLoader.LoadBuildCode(buffer[..(len + 1)]);
		}

		var code = new BuildCode();
		code.Version    = DecodeAndAdvance(ref text);
		if(code.Version < FIRST_VERSIONED_VERSION || code.Version > CURRENT_VERSION) 
			throw new Util.VersionException("Code version mismatch");
		code.Kind       = (Kind)DecodeAndAdvance(ref text);
		Debug.Assert(code.Kind != Kind._UNDEFINED, "Code type not valid");
		code.Profession = (Profession)1 + DecodeAndAdvance(ref text);

		for(var i = 0; i < 3; i++) {
			if(!EatToken(ref text, '_')) {
				var id = (SpecializationId)DecodeAndAdvance(ref text, 2);
				var mixed = DecodeAndAdvance(ref text);
				var choices = new TraitLineChoices();
				for(int j = 0; j < 3; j++)
					choices[j] = (TraitLineChoice)((mixed >> (4 - j * 2)) & 0b00000011);
				code.Specializations[i] = new() {
					SpecializationId = id,
					Choices          = choices,
				};
			}
		}
		if(!EatToken(ref text, '~')) {
			code.WeaponSet1 = LoadWeaponSet(ref text);
			if(!EatToken(ref text, '~'))
				code.WeaponSet2 = LoadWeaponSet(ref text);
		}

		for(int i = 0; i < 5; i++)
			if(!EatToken(ref text, '_'))
				code.SlotSkills[i] = (SkillId)DecodeAndAdvance(ref text, 3);
		
		if(!EatToken(ref text, '_'))
			code.Rune = (ItemId)DecodeAndAdvance(ref text, 3);
		
		if(code.Kind != Kind.PvP)
			code.EquipmentAttributes = LoadAllEquipmentStats(ref text, code);
		else
			code.EquipmentAttributes.Amulet = (StatId)DecodeAndAdvance(ref text, 2);

		if(code.Kind != Kind.PvP) {
			if(!EatToken(ref text, '~'))
				code.Infusions = LoadAllEquipmentInfusions(ref text, code);
			if(!EatToken(ref text, '_'))
				code.Food = (ItemId)DecodeAndAdvance(ref text, 3);
			if(!EatToken(ref text, '_'))
				code.Utility = (ItemId)DecodeAndAdvance(ref text, 3);
		}

		code.ProfessionSpecific = LoadProfessionSpecific(ref text, code.Profession);
		code.Arbitrary          = LoadArbitrary(ref text);
		return code;
	}

	private static WeaponSet LoadWeaponSet(ref ReadOnlySpan<char> text)
	{
		var set = new WeaponSet();
		if(!EatToken(ref text, '_')) set.MainHand = WeaponType._FIRST + DecodeAndAdvance(ref text);
		if(set.MainHand != WeaponType._UNDEFINED)
			if(!EatToken(ref text, '_')) set.Sigil1 = (ItemId)DecodeAndAdvance(ref text, 3);
		
		if(set.MainHand == WeaponType._UNDEFINED || !Static.IsTwoHanded(set.MainHand))
			if(!EatToken(ref text, '_')) set.OffHand = WeaponType._FIRST + DecodeAndAdvance(ref text);
		if(set.OffHand != WeaponType._UNDEFINED || (set.MainHand != WeaponType._UNDEFINED && Static.IsTwoHanded(set.MainHand)))
			if(!EatToken(ref text, '_')) set.Sigil2 = (ItemId)DecodeAndAdvance(ref text, 3);
		return set;
	}

	private static AllEquipmentStats LoadAllEquipmentStats(ref ReadOnlySpan<char> text, BuildCode weaponRef)
	{
		var allData = new AllEquipmentStats();

		var repeatCount = 0;
		var data = StatId._UNDEFINED;
		for(int i = 0; i < ALL_EQUIPMENT_COUNT; i++) {
			if(repeatCount == 0) {
				data = (StatId)DecodeAndAdvance(ref text, 2);

				if(i == ALL_EQUIPMENT_COUNT - 1) repeatCount = 1;
				else repeatCount = DecodeAndAdvance(ref text);
			}

			switch(i) {
				case 11:
					if(!weaponRef.WeaponSet1.HasAny) { i += 3; continue; }
					else if(weaponRef.WeaponSet1.MainHand == WeaponType._UNDEFINED) { continue; }
					else break;
				case 12:
					if(weaponRef.WeaponSet1.OffHand == WeaponType._UNDEFINED) continue;
					else break;
				case 13:
					if(!weaponRef.WeaponSet2.HasAny) { i++; continue; }
					else if(weaponRef.WeaponSet2.MainHand == WeaponType._UNDEFINED) continue;
					else break;
				case 14:
					if(weaponRef.WeaponSet2.OffHand == WeaponType._UNDEFINED) continue;
					else break;
			}

			allData[i] = data;
			repeatCount--;
		}
		return allData;
	}

	private static AllEquipmentInfusions LoadAllEquipmentInfusions(ref ReadOnlySpan<char> text, BuildCode weaponRef)
	{
		var allData = new AllEquipmentInfusions();

		var repeatCount = 0;
		var data = ItemId._UNDEFINED;
		for(int i = 0; i < ALL_INFUSION_COUNT; i++)
		{
			if(repeatCount == 0)
			{
				data = EatToken(ref text, '_') ? ItemId._UNDEFINED : (ItemId)DecodeAndAdvance(ref text, 3);

				if(i == ALL_INFUSION_COUNT - 1) repeatCount = 1;
				else repeatCount = DecodeAndAdvance(ref text);
			}

			switch(i) {
				case 16:
					if(!weaponRef.WeaponSet1.HasAny) { i += 3; continue; }
					else if(weaponRef.WeaponSet1.MainHand == WeaponType._UNDEFINED) { continue; }
					else break;
				case 17:
					if(weaponRef.WeaponSet1.OffHand == WeaponType._UNDEFINED) continue;
					else break;
				case 18:
					if(!weaponRef.WeaponSet2.HasAny) { i++; continue; }
					else if(weaponRef.WeaponSet2.MainHand == WeaponType._UNDEFINED) continue;
					else break;
				case 19:
					if(weaponRef.WeaponSet2.OffHand == WeaponType._UNDEFINED) continue;
					else break;
			}

			allData[i] = data;
			repeatCount--;
		}
		return allData;
	}

	private static IProfessionSpecific LoadProfessionSpecific(ref ReadOnlySpan<char> text, Profession profession)
	{
		switch(profession)
		{
			case Profession.Ranger: {
				var data = new RangerData();
				if(!EatToken(ref text, '~')) {
					if(!EatToken(ref text, '_'))
						data.Pet1 = (PetId)DecodeAndAdvance(ref text, 2);
					if(!EatToken(ref text, '_'))
						data.Pet1 = (PetId)DecodeAndAdvance(ref text, 2);
				}
				return data;
			}

			case Profession.Revenant: {
				var data = new RevenantData();
				data.Legend1 = DecodeAndAdvance(ref text) + Legend._FIRST;
				if(!EatToken(ref text, '_')) {
					data.Legend2 = DecodeAndAdvance(ref text) + Legend._FIRST;
					if(!EatToken(ref text, '_'))
						data.AltUtilitySkill1 = (SkillId)DecodeAndAdvance(ref text, 3);
					if(!EatToken(ref text, '_'))
						data.AltUtilitySkill2 = (SkillId)DecodeAndAdvance(ref text, 3);
					if(!EatToken(ref text, '_'))
						data.AltUtilitySkill3 = (SkillId)DecodeAndAdvance(ref text, 3);
				}
				return data;
			}

			default: return IProfessionSpecific.NONE.Instance;
		}
	}

	private static IArbitrary LoadArbitrary(ref ReadOnlySpan<char> text)
	{
		//implement extensions here in the future
		return IArbitrary.NONE.Instance;
	}


	public static void WriteAndAdvance(ref Span<char> destination, char chr)
	{
		destination[0] = chr;
		destination = destination[1..];
	}

	public static void EncodeAndAdvance(ref Span<char> destination, int value, int width)
	{
		int pos = 0;
		do
		{
			destination[pos] = CHARSET[value & 0b00111111];
			value >>= 6;
			pos++;
		} while(value > 0);
		if(pos < width) destination[pos] = '~';
		destination = destination[(pos + 1)..];
	}

	public static void EncodeOrUnderscoreOnZeroAndAdvance(ref Span<char> destination, int value, int encodeWidth)
	{
		if(value == 0) WriteAndAdvance(ref destination, '_');
		else EncodeAndAdvance(ref destination, value, encodeWidth); 
	}

	public static string WriteBuildCode(BuildCode code)
	{
		Span<char> buffer = stackalloc char[256];
		var length = WriteBuildCode(code, buffer);
		return buffer[..length].ToString();
	}

	/// <returns> Number of characters written. </returns>
	public static int WriteBuildCode(BuildCode code, Span<char> destination)
	{
		var oldLen = destination.Length;
		WriteAndAdvance(ref destination, CHARSET[code.Version]);
		WriteAndAdvance(ref destination, CHARSET[(int)code.Kind]);
		WriteAndAdvance(ref destination, CHARSET[(int)code.Profession - 1]);
		for(int i = 0; i < 3; i++) {
			var spec = code.Specializations[i];
			if(spec.SpecializationId == SpecializationId._UNDEFINED) WriteAndAdvance(ref destination, '_');
			else {
				EncodeAndAdvance(ref destination, (int)spec.SpecializationId, 2);
				WriteAndAdvance(ref destination, CHARSET[
					((int)spec.Choices[0] << 4) | ((int)spec.Choices[1] << 2) | (int)spec.Choices[2]
				]);
			}
		}
		
		if(!code.WeaponSet1.HasAny) WriteAndAdvance(ref destination, '~');
		else
		{
			if(code.WeaponSet1.MainHand == WeaponType._UNDEFINED) WriteAndAdvance(ref destination, '_');
			else WriteAndAdvance(ref destination, CHARSET[code.WeaponSet1.MainHand - WeaponType._FIRST]);
			if(code.WeaponSet1.Sigil1 == ItemId._UNDEFINED) WriteAndAdvance(ref destination, '_');
			else EncodeAndAdvance(ref destination, (int)code.WeaponSet1.Sigil1, 3);

			if(!code.WeaponSet2.HasAny) WriteAndAdvance(ref destination, '~');
			else
			{
				if(code.WeaponSet2.MainHand == WeaponType._UNDEFINED) WriteAndAdvance(ref destination, '_');
				else WriteAndAdvance(ref destination, CHARSET[code.WeaponSet2.MainHand - WeaponType._FIRST]);
				if(code.WeaponSet2.Sigil1 == ItemId._UNDEFINED) WriteAndAdvance(ref destination, '_');
				else EncodeAndAdvance(ref destination, (int)code.WeaponSet2.Sigil1, 3);
			}
		}

		for(int i = 0; i < 5; i++)
			EncodeOrUnderscoreOnZeroAndAdvance(ref destination, (int)code.SlotSkills[i], 3);

		EncodeOrUnderscoreOnZeroAndAdvance(ref destination, (int)code.Rune, 3);

		if(code.Kind != Kind.PvP) EncodeStatsAndAdvance(ref destination, code);
		else EncodeAndAdvance(ref destination, (int)code.EquipmentAttributes.Amulet, 2);

		if(code.Kind != Kind.PvP)
		{
			if(!code.Infusions.HasAny()) WriteAndAdvance(ref destination, '~');
			else EncodeInfusionsAndAdvance(ref destination, code);

			EncodeOrUnderscoreOnZeroAndAdvance(ref destination, (int)code.Food, 3);
			EncodeOrUnderscoreOnZeroAndAdvance(ref destination, (int)code.Utility, 3);
		}

		EncodeProfessionArbitrary(ref destination, code.ProfessionSpecific);
		EncodeArbitrary(ref destination, code.Arbitrary);

		return oldLen - destination.Length;
	}

	private static void EncodeStatsAndAdvance(ref Span<char> destination, BuildCode weaponRef)
	{
		StatId? lastStat = default;
		var repeatCount = 0;
		for(int i = 0; i < ALL_EQUIPMENT_COUNT; i++)
		{
			switch(i) {
				case 11:
					if(!weaponRef.WeaponSet1.HasAny) { i += 3; continue; }
					else if(weaponRef.WeaponSet1.MainHand == WeaponType._UNDEFINED) { continue; }
					else break;
				case 12:
					if(weaponRef.WeaponSet1.OffHand == WeaponType._UNDEFINED) continue;
					else break;
				case 13:
					if(!weaponRef.WeaponSet2.HasAny) { i++; continue; }
					else if(weaponRef.WeaponSet2.MainHand == WeaponType._UNDEFINED) continue;
					else break;
				case 14:
					if(weaponRef.WeaponSet2.OffHand == WeaponType._UNDEFINED) continue;
					else break;
			}

			if(weaponRef.EquipmentAttributes[i] != lastStat)
			{
				if(lastStat.HasValue)
				{
					EncodeAndAdvance(ref destination, (int)lastStat.Value, 2);
					WriteAndAdvance(ref destination, CHARSET[repeatCount]);
				}

				lastStat = weaponRef.EquipmentAttributes[i];
				repeatCount = 1;
			}
			else
			{
				repeatCount++;
			}
		}

		EncodeAndAdvance(ref destination, (int)lastStat!.Value, 2);
		if(repeatCount > 1)
			WriteAndAdvance(ref destination, CHARSET[repeatCount]);
	}

	private static void EncodeInfusionsAndAdvance(ref Span<char> destination, BuildCode weaponRef)
	{
		var lastInfusion = ItemId._UNDEFINED;
		var repeatCount = 0;
		for(int i = 0; i < ALL_INFUSION_COUNT; i++)
		{
			switch(i) {
				case 16:
					if(!weaponRef.WeaponSet1.HasAny) { i += 3; continue; }
					else if(weaponRef.WeaponSet1.MainHand == WeaponType._UNDEFINED) { continue; }
					else break;
				case 17:
					if(weaponRef.WeaponSet1.OffHand == WeaponType._UNDEFINED) continue;
					else break;
				case 18:
					if(!weaponRef.WeaponSet2.HasAny) { i++; continue; }
					else if(weaponRef.WeaponSet2.MainHand == WeaponType._UNDEFINED) continue;
					else break;
				case 19:
					if(weaponRef.WeaponSet2.OffHand == WeaponType._UNDEFINED) continue;
					else break;
			}

			if(weaponRef.Infusions[i] != lastInfusion)
			{
				if(lastInfusion != ItemId._UNDEFINED)
				{
					EncodeAndAdvance(ref destination, (int)lastInfusion, 3);
					WriteAndAdvance(ref destination, CHARSET[repeatCount]);
				}

				lastInfusion = weaponRef.Infusions[i];
				repeatCount = 1;
			}
			else
			{
				repeatCount++;
			}
		}

		EncodeAndAdvance(ref destination, (int)lastInfusion, 2);
		if(repeatCount > 1)
			WriteAndAdvance(ref destination, CHARSET[repeatCount]);
	}

	private static void EncodeProfessionArbitrary(ref Span<char> destination, IProfessionSpecific professionSpecific)
	{
		switch(professionSpecific)
		{
			case RangerData rangerData:
				if(rangerData.Pet1 == PetId._UNDEFINED && rangerData.Pet2 == PetId._UNDEFINED) WriteAndAdvance(ref destination, '~');
				else
				{
					EncodeOrUnderscoreOnZeroAndAdvance(ref destination, (int)rangerData.Pet1, 2);
					EncodeOrUnderscoreOnZeroAndAdvance(ref destination, (int)rangerData.Pet2, 2);
				}
				break;

			case RevenantData revenantData:
				WriteAndAdvance(ref destination, CHARSET[revenantData.Legend1 - Legend._FIRST]);
				if(revenantData.Legend2 == Legend._UNDEFINED) WriteAndAdvance(ref destination, '_');
				else
				{
					WriteAndAdvance(ref destination, CHARSET[revenantData.Legend2 - Legend._FIRST]);
					EncodeOrUnderscoreOnZeroAndAdvance(ref destination, (int)revenantData.AltUtilitySkill1, 3);
					EncodeOrUnderscoreOnZeroAndAdvance(ref destination, (int)revenantData.AltUtilitySkill2, 3);
					EncodeOrUnderscoreOnZeroAndAdvance(ref destination, (int)revenantData.AltUtilitySkill3, 3);
				}
				break;
		}
	}

	private static void EncodeArbitrary(ref Span<char> destination, IArbitrary arbitraryData)
	{
		//space for expansions
		return;
	}

	#endregion

	#region official codes

	/// <inheritdoc cref="BinaryLoader.LoadOfficialBuildCode(ReadOnlySpan{byte}, bool)"/>
	public static BuildCode LoadOfficialBuildCode(string chatLink, bool aquatic = false)
	{
		var base64 = chatLink[0] == '[' ? chatLink.AsSpan(2, chatLink.Length - 3) : chatLink.AsSpan();
		Span<byte> buffer = stackalloc byte[OFFICIAL_CHAT_CODE_BYTE_LENGTH];
		Convert.TryFromBase64Chars(base64, buffer, out _);
		return BinaryLoader.LoadOfficialBuildCode(buffer, aquatic);
	}

	/// <inheritdoc cref="BinaryLoader.WriteOfficialBuildCode(BuildCode, Span{byte}, bool)"/>
	public static string WriteOfficialBuildCode(BuildCode code, bool aquatic = false)
	{
		Span<byte> buffer = stackalloc byte[OFFICIAL_CHAT_CODE_BYTE_LENGTH];
		BinaryLoader.WriteOfficialBuildCode(code, buffer, aquatic);

		return "[&" + Convert.ToBase64String(buffer) + ']';
	}
	/// <inheritdoc cref="BinaryLoader.WriteOfficialBuildCode(BuildCode, Span{byte}, bool)"/>
	public static void WriteOfficialBuildCode(BuildCode code, Span<char> destination, bool aquatic = false)
	{
		Span<byte> buffer = stackalloc byte[OFFICIAL_CHAT_CODE_BYTE_LENGTH];
		BinaryLoader.WriteOfficialBuildCode(code, buffer, aquatic);

		destination[0] = '[';
		destination[1] = '&';
		Convert.TryToBase64Chars(buffer, destination[1..], out var writtenBytes);
		destination[writtenBytes + 2] = ']';
	}

	#endregion
}