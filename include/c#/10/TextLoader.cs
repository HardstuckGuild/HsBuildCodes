using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Diagnostics;

using static Hardstuck.GuildWars2.BuildCodes.V2.Static;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class TextLoader {
	public static readonly string CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-";
	public static int Decode(ref ReadOnlySpan<char> text, int maxWidth)
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

	public static int DecodeNextChar(ref ReadOnlySpan<char> text)
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
					
					Debug.Assert(false, "not gud");
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
		code.Version    = DecodeNextChar(ref text) + 1;
		Debug.Assert(code.Version == CURRENT_VERSION, "Code version mismatch");
		code.Kind       = (Kind)DecodeNextChar(ref text);
		Debug.Assert(code.Kind != Kind._UNDEFINED, "Code type not valid");
		code.Profession = Profession._FIRST + DecodeNextChar(ref text);
		for(var i = 0; i < 3; i++) {
			if(!EatToken(ref text, '_'))
				code.Specializations[i] = new() {
					SpecializationId = (SpecializationId)DecodeNextChar(ref text),
					Choices             = LoadTraitChoices(ref text),
				};
		}
		if(!EatToken(ref text, '~')) {
			code.Weapons.Set1 = LoadWeaponSet(ref text);
			if(!EatToken(ref text, '~'))
				code.Weapons.Set2 = LoadWeaponSet(ref text);
		}
		for(int i = 0; i < 5; i++)
			if(!EatToken(ref text, '_'))
				code.SlotSkills[i] = (SkillId)Decode(ref text, 3);
		if(!EatToken(ref text, '_'))
			code.Rune = Decode(ref text, 3);
		if(code.Kind != Kind.PvP)
			code.EquipmentAttributes = LoadAllEquipmentData(ref text, in code.Weapons, DecodeAttrib);
		else
			code.EquipmentAttributes = LoadAllEquipmentDataPvP(ref text, in code.Weapons);

		if(code.Kind != Kind.PvP) {
			if(!EatToken(ref text, '~'))
				code.Infusions = LoadAllEquipmentData(ref text, in code.Weapons, DecodeInfusion);
			if(!EatToken(ref text, '_'))
				code.Food = Decode(ref text, 3);
			if(!EatToken(ref text, '_'))
				code.Utility = Decode(ref text, 3);
		}
		code.ArbitraryData.ProfessionSpecific = LoadProfessionArbitrary(ref text, code.Profession);
		code.ArbitraryData.Arbitrary          = LoadArbitrary(ref text);
		return code;
	}

	public static TraitLineChoices LoadTraitChoices(ref ReadOnlySpan<char> text)
	{
		var mixed = DecodeNextChar(ref text);
		var choices = new TraitLineChoices();
		for(int i = 0; i < 3; i++)
			choices[i] = (TraitLineChoice)((mixed >> (6 - i * 2)) & 0b00000011);
		return choices;
	}

	public static WeaponSet LoadWeaponSet(ref ReadOnlySpan<char> text)
	{
		var set = new WeaponSet();
		if(!EatToken(ref text, '_'))
			set.MainHand = WeaponType._FIRST + DecodeNextChar(ref text);
		if(!EatToken(ref text, '_'))
			set.Sigil1 = Decode(ref text, 3);
		if(set.MainHand.HasValue && !IsTwoHanded(set.MainHand.Value) && !EatToken(ref text, '_'))
			set.OffHand = WeaponType._FIRST + DecodeNextChar(ref text);
		if(!EatToken(ref text, '_'))
			set.Sigil2 = Decode(ref text, 3);
		return set;
	}

	public static UnderwaterWeapon LoadUnderwaterWeapon(ref ReadOnlySpan<char> text)
	{
		var set = new UnderwaterWeapon();
		set.Weapon = WeaponType._FIRST + DecodeNextChar(ref text);
		if(!EatToken(ref text, '_'))
			set.Sigil1 = Decode(ref text, 3);
		if(!EatToken(ref text, '_'))
			set.Sigil2 = Decode(ref text, 3);
		return set;
	}

	public delegate T DataLoader<T>(ref ReadOnlySpan<char> text);
	public static AllEquipmentData<T> LoadAllEquipmentData<T>(ref ReadOnlySpan<char> text, in AllWeapons loadedWeapons, DataLoader<T> loader)
	{
		var allData = new AllEquipmentData<T>();

		var repeatCount = 0;
		T data = default!;
		for(int i = 0; i < AllEquipmentData.ALL_EQUIPMENT_COUNT; i++) {
			if(repeatCount == 0) {
				data = loader.Invoke(ref text);

				if(i == AllEquipmentData.ALL_EQUIPMENT_COUNT - 1) repeatCount = 1;
				else repeatCount = DecodeNextChar(ref text);
			}

			switch(i) {
				case 11: if(!loadedWeapons.Set1.MainHand.HasValue) { i += 5; continue; } else break;
				case 12: if(!loadedWeapons.Set1.OffHand.HasValue)  {         continue; } else break;
				case 13: if(!loadedWeapons.Set2.IsSet)             { i++;    continue; } else break;
				case 14: if(!loadedWeapons.Set2.OffHand.HasValue)  {         continue; } else break;
			}

			allData[i] = data;
			repeatCount--;
		}
		return allData;
	}
	static int DecodeAttrib(ref ReadOnlySpan<char> text) => Decode(ref text, 2);
	static int? DecodeInfusion(ref ReadOnlySpan<char> text) => EatToken(ref text, '_') ? null : Decode(ref text, 3);

	public static AllEquipmentData<int> LoadAllEquipmentDataPvP(ref ReadOnlySpan<char> text, in AllWeapons loadedWeapons)
	{
		var allData = new AllEquipmentData<int>();

		int data = Decode(ref text, 3);
		for(int i = 0; i < AllEquipmentData.ALL_EQUIPMENT_COUNT; i++) {

			switch(i) {
				case 11: if(!loadedWeapons.Set1.MainHand.HasValue) { i += 5; continue; } else break;
				case 12: if(!loadedWeapons.Set1.OffHand.HasValue)  {         continue; } else break;
				case 13: if(!loadedWeapons.Set2.IsSet)             { i++;    continue; } else break;
				case 14: if(!loadedWeapons.Set2.OffHand.HasValue)  {         continue; } else break;
			}

			allData[i] = data;
		}
		return allData;
	}

	public static IProfessionArbitrary LoadProfessionArbitrary(ref ReadOnlySpan<char> text, Profession profession)
	{
		switch(profession)
		{
			case Profession.Ranger: {
				var data = new RangerData();
				if(!EatToken(ref text, '~')) {
					if(!EatToken(ref text, '_'))
						data.Pet1 = Decode(ref text, 2);
					if(!EatToken(ref text, '_'))
						data.Pet1 = Decode(ref text, 2);
				}
				return data;
			}

			case Profession.Revenant: {
				var data = new RevenantData();
				if(!EatToken(ref text, '_'))
					data.Legend1 = (Legend)DecodeNextChar(ref text);
				if(!EatToken(ref text, '_')) {
					data.Legend2 = (Legend)DecodeNextChar(ref text);
					if(!EatToken(ref text, '_'))
						data.AltUtilitySkill1 = (SkillId)Decode(ref text, 3);
					if(!EatToken(ref text, '_'))
						data.AltUtilitySkill2 = (SkillId)Decode(ref text, 3);
					if(!EatToken(ref text, '_'))
						data.AltUtilitySkill3 = (SkillId)Decode(ref text, 3);
				}
				return data;
			}

			default: return IProfessionArbitrary.NONE.Instance;
		}
	}

	public static IArbitrary LoadArbitrary(ref ReadOnlySpan<char> text)
	{
		//implement extensions here in the future
		return IArbitrary.NONE.Instance;
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