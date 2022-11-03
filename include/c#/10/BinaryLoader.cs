using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Hardstuck.GuildWars2.BuildCodes.V2.Static;
using static Hardstuck.GuildWars2.BuildCodes.V2.Util.Static;

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

	#region hardstuck codes

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
			var traitLine = (SpecializationId)rawSpan.DecodeNext(4);
			if(traitLine != SpecializationId._NONE)
				code.Specializations[i] = new() {
					SpecializationId = traitLine,
					Choices             = LoadTraitChoices(ref rawSpan),
				};
		}
		if(!rawSpan.EatIfExpected(0, 5)) {
			code.Weapons.Set1 = LoadWeaponSet(ref rawSpan);
			if(!rawSpan.EatIfExpected(0, 5))
				code.Weapons.Set2 = LoadWeaponSet(ref rawSpan);
		}
		for(int i = 0; i < 5; i++)
			code.SlotSkills[i] = rawSpan.DecodeNext_WriteMinusMinIfAtLeast<SkillId>(1, 24);
		
		rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref code.Rune, 1, 24);
		
		if(code.Kind != Kind.PvP)
			code.EquipmentAttributes = LoadAllEquipmentStats(ref rawSpan, in code.Weapons);
		else
			code.EquipmentAttributes = LoadAllEquipmentStatsPvP(ref rawSpan, in code.Weapons);

		if(code.Kind != Kind.PvP) {
			if(!rawSpan.EatIfExpected(0, 24))
				code.Infusions = LoadAllEquipmentInfusions(ref rawSpan, in code.Weapons);
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

	private static AllEquipmentStats LoadAllEquipmentStats(ref BitSpan rawSpan, in AllWeapons loadedWeapons)
	{
		var allData = new AllEquipmentStats();

		var repeatCount = 0;
		int data = default!;
		for(int i = 0; i < ALL_EQUIPMENT_COUNT; i++) {
			if(repeatCount == 0) {
				data = rawSpan.DecodeNext(16);

				if(i == ALL_EQUIPMENT_COUNT - 1) repeatCount = 1;
				else repeatCount = rawSpan.DecodeNext(4);
			}

			switch(i) {
				case 11: if(!loadedWeapons.Set1.MainHand.HasValue) { i += 3; continue; } else break;
				case 12: if(!loadedWeapons.Set1.OffHand.HasValue)  {         continue; } else break;
				case 13: if(!loadedWeapons.Set2.IsSet)             { i++;    continue; } else break;
				case 14: if(!loadedWeapons.Set2.OffHand.HasValue)  {         continue; } else break;
			}

			allData[i] = data;
			repeatCount--;
		}
		return allData;
	}

	private static AllEquipmentStats LoadAllEquipmentStatsPvP(ref BitSpan rawSpan, in AllWeapons loadedWeapons)
	{
		var allData = new AllEquipmentStats();

		int data = rawSpan.DecodeNext(16);
		for(int i = 0; i < ALL_EQUIPMENT_COUNT; i++) {

			switch(i) {
				case 11: if(!loadedWeapons.Set1.MainHand.HasValue) { i += 3; continue; } else break;
				case 12: if(!loadedWeapons.Set1.OffHand.HasValue)  {         continue; } else break;
				case 13: if(!loadedWeapons.Set2.IsSet)             { i++;    continue; } else break;
				case 14: if(!loadedWeapons.Set2.OffHand.HasValue)  {         continue; } else break;
			}

			allData[i] = data;
		}
		return allData;
	}

	private static AllEquipmentInfusions LoadAllEquipmentInfusions(ref BitSpan rawSpan, in AllWeapons loadedWeapons)
	{
		var allData = new AllEquipmentInfusions();

		var repeatCount = 0;
		int? data = default!;
		for(int i = 0; i < ALL_EQUIPMENT_COUNT; i++) {
			if(repeatCount == 0) {
				data = rawSpan.DecodeNext_WriteMinusMinIfAtLeast<int>(2, 24);

				if(i == ALL_EQUIPMENT_COUNT - 1) repeatCount = 1;
				else repeatCount = rawSpan.DecodeNext(5);
			}

			switch(i) {
				case 16: if(!loadedWeapons.Set1.MainHand.HasValue) { i += 3; continue; } else break;
				case 17: if(!loadedWeapons.Set1.OffHand.HasValue)  {         continue; } else break;
				case 18: if(!loadedWeapons.Set2.IsSet)             { i++;    continue; } else break;
				case 19: if(!loadedWeapons.Set2.OffHand.HasValue)  {         continue; } else break;
			}

			allData[i] = data;
			repeatCount--;
		}
		return allData;
	}

	private static IProfessionArbitrary LoadProfessionArbitrary(ref BitSpan rawSpan, Profession profession)
	{
		switch(profession)
		{
			case Profession.Ranger: {
				var data = new RangerData();
				if(!rawSpan.EatIfExpected(0, 7)) {
					rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.Pet1, 2, 7);
					rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.Pet2, 2, 7);
				}
				return data;
			}

			case Profession.Revenant: {
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

	/// <summary> Requires pallette ids to be loaded </summary>
	public static BuildCode LoadOfficialBuildCode(ReadOnlySpan<byte> raw, bool aquatic = false)
	{
		var codeType = SliceAndAdvance(ref raw);
		Debug.Assert(codeType == 0x0D);

		var code = new BuildCode();
		code.Version    = CURRENT_VERSION;
		code.Profession = (Profession)SliceAndAdvance(ref raw);
		for(int i = 0; i < 3; i++, raw = raw[2..]) {
			var spec = (SpecializationId)raw[0];
			if(spec == SpecializationId._NONE) continue;

			var choices = new TraitLineChoices();
			var mix = raw[1];
			for(int j = 0; j < 3; j++) {
				choices[j] = (TraitLineChoice)((mix >> (j * 2)) & 0b00000011);
			}
			code.Specializations[i] = new() {
				SpecializationId = spec,
				Choices = choices,
			};
		}

		var skillPallette = ProfessionSkillPallettes.ByProfession(code.Profession);

		var offset = aquatic ? 2 : 0;
		for(int i = 0; i < 5; i++)
		{
			var palletteId = BinaryPrimitives.ReadUInt16LittleEndian(raw.Slice(i * 4  + offset, 2));
			if(palletteId != 0) code.SlotSkills[i] = skillPallette.PalletteToSkill[palletteId];
		}

		raw = raw[(5 * 4)..];

		switch(code.Profession)
		{
			case Profession.Ranger:
				raw = raw[offset..];

				var rangerData = new RangerData();
				if(raw[0] != 0) rangerData.Pet1 = raw[0];
				if(raw[1] != 0) rangerData.Pet2 = raw[1];
				code.ArbitraryData.ProfessionSpecific = rangerData;
				break;

			case Profession.Revenant:
				raw = raw[offset..];

				var revenantData = new RevenantData();
				if(raw[0] != 0) revenantData.Legend1 = raw[0] - Legend._FIRST;
				if(raw[1] != 0) {
					revenantData.Legend2 = raw[1] - Legend._FIRST;
					var revSkillOffset = aquatic ? 6: 2;
					raw = raw[revSkillOffset..];

					var skill1 = BinaryPrimitives.ReadUInt16LittleEndian(raw[..2]);
					if(skill1 != 0) revenantData.AltUtilitySkill1 = skillPallette.PalletteToSkill[skill1];
					var skill2 = BinaryPrimitives.ReadUInt16LittleEndian(raw[2..4]);
					if(skill2 != 0) revenantData.AltUtilitySkill2 = skillPallette.PalletteToSkill[skill2];
					var skill3 = BinaryPrimitives.ReadUInt16LittleEndian(raw[4..6]);
					if(skill3 != 0) revenantData.AltUtilitySkill3 = skillPallette.PalletteToSkill[skill3];
				}
				code.ArbitraryData.ProfessionSpecific = revenantData;
				break;
		}

		code.ArbitraryData.Arbitrary = IArbitrary.NONE.Instance;

		return code;
	}

	public static void WriteOfficialBuildCode(BuildCode code, Span<byte> destination, bool aquatic = false)
	{
		Debug.Assert(destination.Length >= OFFICIAL_CHAT_CODE_BYTE_LENGTH, "destination is not large enough to write code");

		destination[0] = 0x0d; //code type
		destination[1] = (byte)code.Profession;
		for(int i = 0; i < 3; i++) {
			if(!code.Specializations[i].HasValue) continue;

			var spec = code.Specializations[i]!.Value;
			destination[2 + i * 2] = (byte)spec.SpecializationId;
			destination[3 + i * 2] = (byte)((int)spec.Choices[0] | ((int)spec.Choices[1] << 2) | ((int)spec.Choices[2] << 4));
		}

		var pallette = ProfessionSkillPallettes.ByProfession(code.Profession);

		var skillsDestination = destination[(aquatic ? 10 : 8)..];
		for(int i = 0; i < 5; i++) {
			ushort palletteIndex = 0;
			if(code.SlotSkills[i].HasValue) palletteIndex = pallette.SkillToPallette[code.SlotSkills[i]!.Value];
			BinaryPrimitives.WriteUInt16LittleEndian(skillsDestination[(i * 4)..], palletteIndex);
		}

		var profSpecificDest = destination[28..];
		switch(code.Profession)
		{
			case Profession.Ranger:
				var rangerData = (RangerData)code.ArbitraryData.ProfessionSpecific;
				var offset = aquatic ? 2 : 0;
				profSpecificDest[offset + 0] = (byte)(rangerData.Pet1 ?? 0);
				profSpecificDest[offset + 1] = (byte)(rangerData.Pet2 ?? 0);
				break;

			case Profession.Revenant:
				var revenantData = (RevenantData)code.ArbitraryData.ProfessionSpecific;
				int legendOffset, skillOffset;
				if(aquatic) {
					legendOffset = 2;
					skillOffset  = 9;
				}
				else {
					legendOffset = 0;
					skillOffset  = 4;
				}

				profSpecificDest[legendOffset] = (byte)(revenantData.Legend1 ?? 0);
				profSpecificDest[legendOffset + 1] = (byte)(revenantData.Legend2 ?? 0);

				ushort altSkill1PalletteId = revenantData.AltUtilitySkill1.HasValue ? pallette.SkillToPallette[revenantData.AltUtilitySkill1.Value] : (ushort)0;
				BinaryPrimitives.WriteUInt16LittleEndian(profSpecificDest[skillOffset..], altSkill1PalletteId);

				ushort altSkill2PalletteId = revenantData.AltUtilitySkill2.HasValue ? pallette.SkillToPallette[revenantData.AltUtilitySkill2.Value] : (ushort)0;
				BinaryPrimitives.WriteUInt16LittleEndian(profSpecificDest[(skillOffset + 2)..], altSkill2PalletteId);

				ushort altSkill3PalletteId = revenantData.AltUtilitySkill3.HasValue ? pallette.SkillToPallette[revenantData.AltUtilitySkill3.Value] : (ushort)0;
				BinaryPrimitives.WriteUInt16LittleEndian(profSpecificDest[(skillOffset + 4)..], altSkill3PalletteId);
				break;
		}
	}

	#endregion
}