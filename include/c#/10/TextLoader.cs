using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Diagnostics;

using static Hardstuck.GuildWars2.BuildCodes.V2.Static;
using static System.Net.Mime.MediaTypeNames;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class TextLoader {
	public static readonly string CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-";
	public static int DecodeAndAdvance(ref ReadOnlySpan<char> text, int maxWidth)
	{
		int value = 0;
		var width = 0;
		do {
			var c = text[width];
			var mulShift = 6 * width; // shift by 6, 12, 18 = multiply by 64 64^2 64^3
			width++;
			if(c == '~') break;
			value += Decode(c) << mulShift;
		} while(width < maxWidth);

		text = text[width..];

		return value;
	}

	public static int DecodeAndAdvance(ref ReadOnlySpan<char> text)
	{
		var val = Decode(text[0]);
		text = text[1..];
		return val;
	}

	public static int Decode(char c)
	{
		var upperIndex = (c - 'A') & 63; // & 63 = bootleg range check
		if(CHARSET[upperIndex] == c) return upperIndex;
		else
		{
			var lowerIndex = (26 + c - 'a') & 63;
			if(CHARSET[lowerIndex] == c) return lowerIndex;
			else
			{
				var numericIndex = (26 * 2 + c - '0') & 63;
				if(CHARSET[numericIndex] == c) return numericIndex;
				else
				{
					if(c == '+') return 62;
					else if(c == '-') return 63;
					
					Debug.Assert(false, "Tried to decode invalid character");
					return -1;
				}
			}
		}
	}

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
		var code = new BuildCode();
		code.Version    = DecodeAndAdvance(ref text);
		Debug.Assert(code.Version == CURRENT_VERSION, "Code version mismatch");
		code.Kind       = (Kind)DecodeAndAdvance(ref text);
		Debug.Assert(code.Kind != Kind._UNDEFINED, "Code type not valid");
		code.Profession = Profession._FIRST + DecodeAndAdvance(ref text);
		for(var i = 0; i < 3; i++) {
			if(!EatToken(ref text, '_'))
				code.Specializations[i] = new() {
					SpecializationId = (SpecializationId)DecodeAndAdvance(ref text),
					Choices             = LoadTraitChoices(ref text),
				};
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
			code.Rune = DecodeAndAdvance(ref text, 3);
		
		if(code.Kind != Kind.PvP)
			code.EquipmentAttributes = LoadAllEquipmentStats(ref text, code);
		else
			code.EquipmentAttributes.Amulet = (StatId)DecodeAndAdvance(ref text, 2);

		if(code.Kind != Kind.PvP) {
			if(!EatToken(ref text, '~'))
				code.Infusions = LoadAllEquipmentInfusions(ref text, code);
			if(!EatToken(ref text, '_'))
				code.Food = DecodeAndAdvance(ref text, 3);
			if(!EatToken(ref text, '_'))
				code.Utility = DecodeAndAdvance(ref text, 3);
		}

		code.ProfessionSpecific = LoadProfessionArbitrary(ref text, code.Profession);
		code.Arbitrary          = LoadArbitrary(ref text);
		return code;
	}

	private static TraitLineChoices LoadTraitChoices(ref ReadOnlySpan<char> text)
	{
		var mixed = DecodeAndAdvance(ref text);
		var choices = new TraitLineChoices();
		for(int i = 0; i < 3; i++)
			choices[i] = (TraitLineChoice)((mixed >> (6 - i * 2)) & 0b00000011);
		return choices;
	}

	private static WeaponSet LoadWeaponSet(ref ReadOnlySpan<char> text)
	{
		var set = new WeaponSet();
		if(!EatToken(ref text, '_'))
			set.MainHand = WeaponType._FIRST + DecodeAndAdvance(ref text);
		if(!EatToken(ref text, '_'))
			set.Sigil1 = DecodeAndAdvance(ref text, 3);
		if(set.MainHand.HasValue && !IsTwoHanded(set.MainHand.Value) && !EatToken(ref text, '_'))
			set.OffHand = WeaponType._FIRST + DecodeAndAdvance(ref text);
		if(!EatToken(ref text, '_'))
			set.Sigil2 = DecodeAndAdvance(ref text, 3);
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
					else if(!weaponRef.WeaponSet1.MainHand.HasValue) { continue; }
					else break;
				case 12:
					if(!weaponRef.WeaponSet1.OffHand.HasValue) continue;
					else break;
				case 13:
					if(!weaponRef.WeaponSet2.HasAny) { i++; continue; }
					else if(!weaponRef.WeaponSet2.MainHand.HasValue) continue;
					else break;
				case 14:
					if(!weaponRef.WeaponSet2.OffHand.HasValue) continue;
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
		int? data = default!;
		for(int i = 0; i < ALL_INFUSION_COUNT; i++)
		{
			if(repeatCount == 0)
			{
				data = EatToken(ref text, '_') ? null : DecodeAndAdvance(ref text, 3);

				if(i == ALL_INFUSION_COUNT - 1) repeatCount = 1;
				else repeatCount = DecodeAndAdvance(ref text);
			}

			switch(i) {
				case 16:
					if(!weaponRef.WeaponSet1.HasAny) { i += 3; continue; }
					else if(!weaponRef.WeaponSet1.MainHand.HasValue) { continue; }
					else break;
				case 17:
					if(!weaponRef.WeaponSet1.OffHand.HasValue) continue;
					else break;
				case 18:
					if(!weaponRef.WeaponSet2.HasAny) { i++; continue; }
					else if(!weaponRef.WeaponSet2.MainHand.HasValue) continue;
					else break;
				case 19:
					if(!weaponRef.WeaponSet2.OffHand.HasValue) continue;
					else break;
			}

			allData[i] = data;
			repeatCount--;
		}
		return allData;
	}

	private static IProfessionSpecific LoadProfessionArbitrary(ref ReadOnlySpan<char> text, Profession profession)
	{
		switch(profession)
		{
			case Profession.Ranger: {
				var data = new RangerData();
				if(!EatToken(ref text, '~')) {
					if(!EatToken(ref text, '_'))
						data.Pet1 = DecodeAndAdvance(ref text, 2);
					if(!EatToken(ref text, '_'))
						data.Pet1 = DecodeAndAdvance(ref text, 2);
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

	public static void EncodeOrUnderscoreAndAdvance(ref Span<char> destination, int? value, int encodeWidth)
	{
		if(value.HasValue) EncodeAndAdvance(ref destination, value.Value, encodeWidth);
		else WriteAndAdvance(ref destination, '_');
	}

	public static string WriteBuildCode(BuildCode code)
	{
		Span<char> buffer = stackalloc char[256];
		var length = WriteBuildCode(code, buffer);
		return buffer[..length].ToString();
	}

	/// <summary> returns number of characters written </summary>
	public static int WriteBuildCode(BuildCode code, Span<char> destination)
	{
		var oldLen = destination.Length;
		WriteAndAdvance(ref destination, CHARSET[code.Version]);
		WriteAndAdvance(ref destination, CHARSET[(int)code.Kind]);
		WriteAndAdvance(ref destination, CHARSET[code.Profession - Profession._FIRST]);
		for(int i = 0; i < 3; i++) {
			var spec = code.Specializations[i];
			if(!spec.HasValue) WriteAndAdvance(ref destination, '_');
			else {
				WriteAndAdvance(ref destination, CHARSET[(int)spec.Value.SpecializationId]);
				WriteAndAdvance(ref destination, CHARSET[
					((int)spec.Value.Choices[0] << 4) | ((int)spec.Value.Choices[1] << 2) | (int)spec.Value.Choices[2]
				]);
			}
		}
		
		if(!code.WeaponSet1.HasAny) WriteAndAdvance(ref destination, '~');
		else
		{
			if(!code.WeaponSet1.MainHand.HasValue) WriteAndAdvance(ref destination, '_');
			else WriteAndAdvance(ref destination, CHARSET[code.WeaponSet1.MainHand.Value - WeaponType._FIRST]);
			if(!code.WeaponSet1.Sigil1.HasValue) WriteAndAdvance(ref destination, '_');
			else EncodeAndAdvance(ref destination, code.WeaponSet1.Sigil1.Value, 3);

			if(!code.WeaponSet2.HasAny) WriteAndAdvance(ref destination, '~');
			else
			{
				if(!code.WeaponSet2.MainHand.HasValue) WriteAndAdvance(ref destination, '_');
				else WriteAndAdvance(ref destination, CHARSET[code.WeaponSet2.MainHand.Value - WeaponType._FIRST]);
				if(!code.WeaponSet2.Sigil1.HasValue) WriteAndAdvance(ref destination, '_');
				else EncodeAndAdvance(ref destination, code.WeaponSet2.Sigil1.Value, 3);
			}
		}

		for(int i = 0; i < 5; i++)
			EncodeOrUnderscoreAndAdvance(ref destination, (int?)code.SlotSkills[i], 3);

		EncodeOrUnderscoreAndAdvance(ref destination, code.Rune, 3);

		if(code.Kind != Kind.PvP) EncodeStatsAndAdvance(ref destination, code);
		else EncodeAndAdvance(ref destination, (int)code.EquipmentAttributes.Amulet, 2);

		if(code.Kind != Kind.PvP)
		{
			if(!code.Infusions.HasAny()) WriteAndAdvance(ref destination, '~');
			else EncodeInfusionsAndAdvance(ref destination, code);

			EncodeOrUnderscoreAndAdvance(ref destination, code.Food, 3);
			EncodeOrUnderscoreAndAdvance(ref destination, code.Utility, 3);
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
					else if(!weaponRef.WeaponSet1.MainHand.HasValue) { continue; }
					else break;
				case 12:
					if(!weaponRef.WeaponSet1.OffHand.HasValue) continue;
					else break;
				case 13:
					if(!weaponRef.WeaponSet2.HasAny) { i++; continue; }
					else if(!weaponRef.WeaponSet2.MainHand.HasValue) continue;
					else break;
				case 14:
					if(!weaponRef.WeaponSet2.OffHand.HasValue) continue;
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
		int? lastInfusion = default;
		var repeatCount = 0;
		for(int i = 0; i < ALL_INFUSION_COUNT; i++)
		{
			switch(i) {
				case 16:
					if(!weaponRef.WeaponSet1.HasAny) { i += 3; continue; }
					else if(!weaponRef.WeaponSet1.MainHand.HasValue) { continue; }
					else break;
				case 17:
					if(!weaponRef.WeaponSet1.OffHand.HasValue) continue;
					else break;
				case 18:
					if(!weaponRef.WeaponSet2.HasAny) { i++; continue; }
					else if(!weaponRef.WeaponSet2.MainHand.HasValue) continue;
					else break;
				case 19:
					if(!weaponRef.WeaponSet2.OffHand.HasValue) continue;
					else break;
			}

			if(weaponRef.Infusions[i] != lastInfusion)
			{
				if(lastInfusion.HasValue)
				{
					EncodeAndAdvance(ref destination, lastInfusion.Value, 3);
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

		EncodeAndAdvance(ref destination, lastInfusion!.Value, 2);
		if(repeatCount > 1)
			WriteAndAdvance(ref destination, CHARSET[repeatCount]);
	}

	private static void EncodeProfessionArbitrary(ref Span<char> destination, IProfessionSpecific professionSpecific)
	{
		switch(professionSpecific)
		{
			case RangerData rangerData:
				if(!rangerData.Pet1.HasValue && !rangerData.Pet2.HasValue) WriteAndAdvance(ref destination, '~');
				else
				{
					EncodeOrUnderscoreAndAdvance(ref destination, rangerData.Pet1, 2);
					EncodeOrUnderscoreAndAdvance(ref destination, rangerData.Pet2, 2);
				}
				break;

			case RevenantData revenantData:
				WriteAndAdvance(ref destination, CHARSET[revenantData.Legend1 - Legend._FIRST]);
				if(!revenantData.Legend2.HasValue) WriteAndAdvance(ref destination, '_');
				else
				{
					WriteAndAdvance(ref destination, CHARSET[revenantData.Legend2.Value - Legend._FIRST]);
					EncodeOrUnderscoreAndAdvance(ref destination, (int?)revenantData.AltUtilitySkill1, 3);
					EncodeOrUnderscoreAndAdvance(ref destination, (int?)revenantData.AltUtilitySkill2, 3);
					EncodeOrUnderscoreAndAdvance(ref destination, (int?)revenantData.AltUtilitySkill3, 3);
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

	public static BuildCode LoadOfficialBuildCode(string chatLink, bool aquatic = false)
	{
		var base64 = chatLink[0] == '[' ? chatLink.AsSpan(2, chatLink.Length - 3) : chatLink.AsSpan();
		Span<byte> buffer = stackalloc byte[OFFICIAL_CHAT_CODE_BYTE_LENGTH];
		Convert.TryFromBase64Chars(base64, buffer, out _);
		return BinaryLoader.LoadOfficialBuildCode(buffer, aquatic);
	}

	public static string WriteOfficialBuildCode(BuildCode code, bool aquatic = false)
	{
		Span<byte> buffer = stackalloc byte[OFFICIAL_CHAT_CODE_BYTE_LENGTH];
		BinaryLoader.WriteOfficialBuildCode(code, buffer, aquatic);

		return "[&" + Convert.ToBase64String(buffer) + ']';
	}
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