using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Buffers.Binary;
using System.Diagnostics;

using static Hardstuck.GuildWars2.BuildCodes.V2.Static;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class BinaryLoader {
	public ref struct BitSpan {
		public ReadOnlySpan<byte> Data;
		public int BitPos;

		public BitSpan(ReadOnlySpan<byte> data)
		{
			this.Data = data;
			this.BitPos = 0;
		}

		//NOTE(Rennorb): its more efficient to not use this, but its really handy
		public bool EatIfExpected(int expected, int width)
		{
			var val = DecodeNext(width);
			if(val != expected) BitPos -= width;
			return val == expected;
		}

		public T? DecodeNext_WriteMinusMinIfAtLeast<T>(int min, int width) where T : unmanaged
		{
			T? val = null;
			DecodeNext_WriteMinusMinIfAtLeast(ref val, min, width);
			return val;
		}
		public void DecodeNext_WriteMinusMinIfAtLeast<T>(ref T? target, int min, int width) where T: unmanaged
		{
			var id = DecodeNext(width);
			var actualValue = id - min;
			if(id >= min) unsafe{ target = *(T*)&actualValue; };
		}

		public int DecodeNext(int width)
		{
			//NOTE(Rennorb): this is way overcompluicated
			//TODO(Rennorb): @cleanup refactor this mess

			var sourceByteStart = this.BitPos >> 3;
			var sourceByteWidth = ((this.BitPos & 7) + width + 7) >> 3;
			//sadly required because  BinaryPrimitives.ReadInt32BigEndian always wants to decode 4 bytes
			Span<byte> containsData = stackalloc byte[4];
			var additionalBytes = containsData.Length - sourceByteWidth;
			this.Data.Slice(sourceByteStart, sourceByteWidth).CopyTo(containsData[additionalBytes..]);
			var bitShiftRight = 8 - (this.BitPos + width) & 7;
			var bitShiftLeft = ((this.BitPos & 7) - (8 - (width & 7))) & 7;
			for(int i = 3; i > additionalBytes; i--)
			{
				containsData[i] >>= bitShiftRight;
				containsData[i] |= (byte)(containsData[i - 1] << bitShiftLeft);
			}
			var firstTargetByte = containsData.Length - ((width + 7) >> 3);
			if(firstTargetByte != additionalBytes) containsData[additionalBytes] = 0;
			else containsData[additionalBytes] >>= bitShiftRight;
			containsData[firstTargetByte] &= (byte)((1 << width) - 1);

			this.BitPos += width;

			return unchecked((int)BinaryPrimitives.ReadUInt32BigEndian(containsData));
		}

		public string DebugPrint()
		{
			var s = "";
			for(int i = 0; i < this.Data.Length; i++)
			{
				if(i > 0 && i % 8 == 0) s += "| ";

				if(i == this.BitPos / 8)
					s += "_["+Convert.ToString(this.Data[i], toBase: 2).PadLeft(8, '0')+']';
				else s += '_'+Convert.ToString(this.Data[i], toBase: 2).PadLeft(8, '0');
			}
			return s;
		}
	}

	#region HSBuildCodes

	public static BuildCode LoadBuildCode(ReadOnlySpan<byte> raw)
	{
		var rawSpan = new BitSpan(raw);

		var code = new BuildCode();
		code.Version = rawSpan.DecodeNext(8) - 'a' + 1;
		Debug.Assert(code.Version == CURRENT_VERSION, "Code version mismatch");
		code.Kind    = (rawSpan.DecodeNext(2)) switch {
			0 => Kind.PvP,
			1 => Kind.WvW,
			2 => Kind.PvE,
			_ => Kind._UNDEFINED,
		};
		Debug.Assert(code.Kind != Kind._UNDEFINED, "Code type not valid");
		code.Profession = Profession._FIRST + rawSpan.DecodeNext(4);
		for(var i = 0; i < 3; i++) {
			var traitLine = rawSpan.DecodeNext(4);
			if(traitLine != 0)
				code.Specializations[i] = new() {
					SpecializationIndex = traitLine,
					Choices             = LoadTraitChoices(ref rawSpan),
				};
		}
		if(!rawSpan.EatIfExpected(0, 5)) {
			code.Weapons.Land1 = LoadWeaponSet(ref rawSpan);
			if(!rawSpan.EatIfExpected(0, 5))
				code.Weapons.Land2 = LoadWeaponSet(ref rawSpan);
			if(!rawSpan.EatIfExpected(0, 5)) {
				code.Weapons.Underwater1 = LoadUnderwaterWeapon(ref rawSpan);
				if(!rawSpan.EatIfExpected(1, 5))
					code.Weapons.Underwater2 = LoadUnderwaterWeapon(ref rawSpan);
			}
		}
		for(int i = 0; i < 5; i++)
			code.SlotSkills[i] = rawSpan.DecodeNext_WriteMinusMinIfAtLeast<SkillId>(1, 24);
		
		rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref code.Rune, 1, 24);
		
		if(code.Kind != Kind.PvP)
			code.EquipmentAttributes = LoadAllEquipmentData(ref rawSpan, in code.Weapons, DecodeAttrib);
		else
			code.EquipmentAttributes = LoadAllEquipmentDataPvP(ref rawSpan, in code.Weapons);

		if(code.Kind != Kind.PvP) {
			if(!rawSpan.EatIfExpected(0, 24))
				code.Infusions = LoadAllEquipmentData(ref rawSpan, in code.Weapons, DecodeInfusion);
			rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref code.Food   , 1, 24);
			rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref code.Utility, 1, 24);
		}
		code.ArbitraryData.ProfessionSpecific = LoadProfessionArbitrary(ref rawSpan, code.Profession);
		code.ArbitraryData.Arbitrary          = LoadArbitrary(ref rawSpan);

		return code;
	}

	private static TraitLineChoices LoadTraitChoices(ref BitSpan rawSpan)
	{
		var choices = new TraitLineChoices();
		for(var i = 0; i < 3; i++)
			choices[i] = (TraitLineChoice)rawSpan.DecodeNext(2);
		return choices;
	}

	private static WeaponSet LoadWeaponSet(ref BitSpan rawSpan)
	{
		var set = new WeaponSet();
		set.MainHand = WeaponType._FIRST + rawSpan.DecodeNext_WriteMinusMinIfAtLeast<int>(2, 5);
		rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref set.Sigil1, 1, 24);
		if(set.MainHand.HasValue && !IsTwoHanded(set.MainHand.Value))
			set.OffHand = WeaponType._FIRST + rawSpan.DecodeNext_WriteMinusMinIfAtLeast<int>(2, 5);
		rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref set.Sigil2, 1, 24);
		return set;
	}

	private static UnderwaterWeapon? LoadUnderwaterWeapon(ref BitSpan rawSpan)
	{
		var set = new UnderwaterWeapon();
		set.Weapon = WeaponType._FIRST + rawSpan.DecodeNext_WriteMinusMinIfAtLeast<int>(2, 5)!.Value;
		rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref set.Sigil1, 1, 24);
		rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref set.Sigil2, 1, 24);
		return set;
	}

	public delegate T DataLoader<T>(ref BitSpan rawSpan);
	private static AllEquipmentData<T> LoadAllEquipmentData<T>(ref BitSpan rawSpan, in AllWeapons loadedWeapons, DataLoader<T> loader)
	{
		var allData = new AllEquipmentData<T>();

		var repeatCount = 0;
		T data = default!;
		for(int i = 0; i < AllEquipmentData.ALL_EQUIPMENT_COUNT; i++) {
			if(repeatCount == 0) {
				data = loader.Invoke(ref rawSpan);

				if(i == AllEquipmentData.ALL_EQUIPMENT_COUNT - 1) repeatCount = 1;
				else repeatCount = rawSpan.DecodeNext(5);
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
	static int DecodeAttrib(ref BitSpan rawSpan) => rawSpan.DecodeNext(16);
	static int? DecodeInfusion(ref BitSpan rawSpan) => rawSpan.DecodeNext_WriteMinusMinIfAtLeast<int>(2, 24);

	private static AllEquipmentData<int> LoadAllEquipmentDataPvP(ref BitSpan rawSpan, in AllWeapons loadedWeapons)
	{
		var allData = new AllEquipmentData<int>();

		int data = rawSpan.DecodeNext(16);
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

	private static IProfessionArbitrary LoadProfessionArbitrary(ref BitSpan rawSpan, Profession profession)
	{
		switch(profession)
		{
			case Profession.RANGER: {
				var data = new RangerData();
				if(!rawSpan.EatIfExpected(0, 7)) {
					rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.Pet1, 2, 7);
					rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.Pet2, 2, 7);
				}
				return data;
			}

			case Profession.REVENANT: {
				var data = new RevenantData();
				rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.Legend1, 1, 4);
				rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.Legend2, 1, 4);
				if(data.Legend2.HasValue) {
					rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.AltUtilitySkill1, 1, 24);
					rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.AltUtilitySkill2, 1, 24);
					rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.AltUtilitySkill3, 1, 24);
				}
				return data;
			}

			default: return IProfessionArbitrary.NONE.Instance;
		}
	}

	private static IArbitrary LoadArbitrary(ref BitSpan rawSpan)
	{
		//implement extensions here in the future
		return IArbitrary.NONE.Instance;
	}

	#endregion

	#region official codes

	public static BuildCode LoadBuildCodeFromOfficialBuildCode(ReadOnlySpan<byte> raw)
	{
		static byte SliceAndShift(ref ReadOnlySpan<byte> raw)
		{
			byte b = raw[0];
			raw = raw[1..];
			return b;
		}

		var codeType = SliceAndShift(ref raw);
		Debug.Assert(codeType == 0x0D);

		var code = new BuildCode();
		code.Version    = CURRENT_VERSION;
		code.Profession = (Profession)SliceAndShift(ref raw);
		for(int i = 0; i < 3; i++) {
			int spec = SliceAndShift(ref raw);
			var choices = new TraitLineChoices();
			var mix = SliceAndShift(ref raw);
			for(int j = 0; j < 3; j++) {
				choices[j] = (TraitLineChoice)((mix >> (j * 2)) & 0b00000011);
			}
			code.Specializations[i] = new() {
				SpecializationIndex = spec,
				Choices             = choices,
			};
		}

		for(int i = 0; i < 10; i++)
		{
			var terrestrial = BinaryPrimitives.ReadUInt16LittleEndian(raw.Slice(i * 2    , 2));
			var aquatic     = BinaryPrimitives.ReadUInt16LittleEndian(raw.Slice(i * 2 + 1, 2));

			if(terrestrial != 0)  code.SlotSkills[i / 2] = TranslatePalleteSkill(terrestrial);
			else if(aquatic != 0) code.SlotSkills[i / 2] = TranslatePalleteSkill(aquatic);
		}

		raw = raw[20..];

		switch(code.Profession)
		{
			case Profession.RANGER:
				var rangerData = new RangerData();
				if(raw[0] != 0) rangerData.Pet1 = raw[0];
				if(raw[1] != 0) rangerData.Pet2 = raw[1];
				code.ArbitraryData.ProfessionSpecific = rangerData;
				break;

			case Profession.REVENANT:
				var revenantData = new RevenantData();
				if(raw[0] != 0) revenantData.Legend1 = (Legend)raw[0];
				if(raw[1] != 0) {
					revenantData.Legend2 = (Legend)raw[1];
					raw = raw[1..];

					var skill1 = BinaryPrimitives.ReadUInt16LittleEndian(raw[..2]);
					if(skill1 != 0) revenantData.AltUtilitySkill1 = TranslatePalleteSkill(skill1);
					var skill2 = BinaryPrimitives.ReadUInt16LittleEndian(raw[2..4]);
					if(skill2 != 0) revenantData.AltUtilitySkill2 = TranslatePalleteSkill(skill2);
					var skill3 = BinaryPrimitives.ReadUInt16LittleEndian(raw[4..6]);
					if(skill3 != 0) revenantData.AltUtilitySkill3 = TranslatePalleteSkill(skill3);
				}
				code.ArbitraryData.ProfessionSpecific = revenantData;
				break;
		}

		code.ArbitraryData.Arbitrary = IArbitrary.NONE.Instance;

		return code;
	}

	#endregion
}