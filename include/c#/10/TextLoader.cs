using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Diagnostics;

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
	public static T DecodeEnum<T>(ref ReadOnlySpan<char> text) where T : unmanaged, Enum
	{
		Debug.Assert(Enum.GetUnderlyingType(typeof(T)) == typeof(ushort), 
			$"Enum {typeof(T)} should be of type ushort, but is {Enum.GetUnderlyingType(typeof(T))}");

		var val = (ushort)Decode(text[0]);
		text = text[1..];
		T t;
		unsafe { *(&t) = *((T*)&val); }
		Debug.Assert(Enum.IsDefined(t), $"Parsed undefined value {val} for {typeof(T)}");
		return t;
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

	public static BuildCode LoadBuildCode(ReadOnlySpan<char> text) {

		var code = new BuildCode();
		code.Version    = DecodeNextChar(ref text) + 1;
		Debug.Assert(code.Version == Database.CURRENT_VERSION, "Code version mismatch");
		code.Kind       = DecodeEnum<Kind>(ref text);
		code.Profession = DecodeEnum<Profession>(ref text) + 1;
		for(var i = 0; i < 3; i++) {
			if(!EatToken(ref text, '_'))
				code.Specializations[i] = new() {
					SpecializationIndex = DecodeNextChar(ref text),
					Choices             = LoadTraitChoices(ref text),
				};
		}
		if(!EatToken(ref text, '~'))
		{
			code.Weapons.Land1 = LoadWeaponSet(ref text);
			if(!EatToken(ref text, '~'))
				code.Weapons.Land2 = LoadWeaponSet(ref text);
			if(!EatToken(ref text, '~')) {
				code.Weapons.Underwater1 = LoadUnderwaterWeapon(ref text);
				if(!EatToken(ref text, '_'))
					code.Weapons.Underwater2 = LoadUnderwaterWeapon(ref text);
			}
		}
		for(int i = 0; i < 5; i++)
			if(!EatToken(ref text, '_'))
				code.SlotSkills[i] = Decode(ref text, 3);
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
		set.MainHand = DecodeEnum<WeaponType>(ref text);
		if(!EatToken(ref text, '_'))
			set.Sigil1 = Decode(ref text, 3);
		if(!Database.IsTwoHanded(set.MainHand.Value))
			set.OffHand = DecodeEnum<WeaponType>(ref text);
		if(!EatToken(ref text, '_'))
			set.Sigil2 = Decode(ref text, 3);
		return set;
	}

	public static UnderwaterWeapon LoadUnderwaterWeapon(ref ReadOnlySpan<char> text)
	{
		var set = new UnderwaterWeapon();
		set.Weapon = DecodeEnum<WeaponType>(ref text);
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
				case 11: if(!loadedWeapons.Land1.MainHand.HasValue) { i += 5; continue; } else break;
				case 12: if(!loadedWeapons.Land1.OffHand.HasValue)  {         continue; } else break;
				case 13: if(!loadedWeapons.Land2.IsSet)             { i++;    continue; } else break;
				case 14: if(!loadedWeapons.Land2.OffHand.HasValue)  {         continue; } else break;
				case 15: if(!loadedWeapons.HasUnderwater)           { i++;    continue; } else break;
				case 16: if(!loadedWeapons.Underwater2.HasValue)    {         continue; } else break;
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
				case 11: if(!loadedWeapons.Land1.MainHand.HasValue) { i += 5; continue; } else break;
				case 12: if(!loadedWeapons.Land1.OffHand.HasValue)  {         continue; } else break;
				case 13: if(!loadedWeapons.Land2.IsSet)             { i++;    continue; } else break;
				case 14: if(!loadedWeapons.Land2.OffHand.HasValue)  {         continue; } else break;
				case 15: if(!loadedWeapons.HasUnderwater)           { i++;    continue; } else break;
				case 16: if(!loadedWeapons.Underwater2.HasValue)    {         continue; } else break;
			}

			allData[i] = data;
		}
		return allData;
	}

	public static IProfessionArbitrary LoadProfessionArbitrary(ref ReadOnlySpan<char> text, Profession profession)
	{
		switch(profession)
		{
			case Profession.RANGER: {
				var data = new RangerData();
				if(!EatToken(ref text, '~')) {
					if(!EatToken(ref text, '_'))
						data.PetLand1 = Decode(ref text, 2);
					if(!EatToken(ref text, '_'))
						data.PetLand1 = Decode(ref text, 2);
				}
				if(!EatToken(ref text, '~')) {
					if(!EatToken(ref text, '_'))
						data.PetWater1 = Decode(ref text, 2);
					if(!EatToken(ref text, '_'))
						data.PetWater2 = Decode(ref text, 2);
				}
				return data;
			}

			case Profession.REVENANT: {
				var data = new RevenantData();
					if(!EatToken(ref text, '_'))
						data.Legend1 = DecodeNextChar(ref text);
					if(!EatToken(ref text, '_'))
						data.Legend2 = DecodeNextChar(ref text);
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
}