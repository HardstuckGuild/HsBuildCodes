using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Buffers.Binary;
using System.Diagnostics;
using static Hardstuck.GuildWars2.BuildCodes.V2.Static;
using static Hardstuck.GuildWars2.BuildCodes.V2.Util.Static;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class BinaryLoader {
	public ref struct BitReader {
		public ReadOnlySpan<byte> Data;
		public int BitPos;

		public BitReader(ReadOnlySpan<byte> data) {
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

		public T DecodeNext_WriteMinusMinIfAtLeast<T>(int min, int width) where T : unmanaged
		{
			T val = default;
			DecodeNext_WriteMinusMinIfAtLeast(ref val, min, width);
			return val;
		}
		public void DecodeNext_WriteMinusMinIfAtLeast<T>(ref T target, int min, int width) where T : unmanaged
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

	public ref struct BitWriter {
		public Span<byte> Data;
		public int BitPos;

		public BitWriter(Span<byte> data) {
			this.Data = data;
			this.BitPos = 0;
		}

		public void Write(int value, int bitWidth)
		{
			var posInByte = this.BitPos & 7;
			var bytesTouched = (posInByte + bitWidth + 7) >> 3;

			Span<byte> buffer = stackalloc byte[4];
			BinaryPrimitives.WriteInt32BigEndian(buffer, value << (32 - bitWidth - posInByte));

			var dest = this.Data.Slice(this.BitPos >> 3, bytesTouched);
			for(int i = 0; i < bytesTouched; i++)
				dest[i] |= buffer[i];

			this.BitPos += bitWidth;
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
		var rawSpan = new BitReader(raw);

		var code = new BuildCode();
		code.Version = rawSpan.DecodeNext(8) - 'a';
		Debug.Assert(code.Version >= FIRST_VERSIONED_VERSION && code.Version <= CURRENT_VERSION, "Code version mismatch");
		code.Kind    = (rawSpan.DecodeNext(2)) switch {
			0 => Kind.PvP,
			1 => Kind.WvW,
			2 => Kind.PvE,
			_ => Kind._UNDEFINED,
		};
		Debug.Assert(code.Kind != Kind._UNDEFINED, "Code type not valid");
		code.Profession = (Profession)1 + rawSpan.DecodeNext(4);


		for(var i = 0; i < 3; i++) {
			var traitLine = (SpecializationId)rawSpan.DecodeNext(7);
			if(traitLine != SpecializationId._UNDEFINED) {
				var choices = new TraitLineChoices();
				for(var j = 0; j < 3; j++)
					choices[j] = (TraitLineChoice)rawSpan.DecodeNext(2);
				code.Specializations[i] = new() {
					SpecializationId = traitLine,
					Choices          = choices,
				};
			}
		}
		if(!rawSpan.EatIfExpected(0, 5)) {
			code.WeaponSet1 = LoadWeaponSet(ref rawSpan);
			if(!rawSpan.EatIfExpected(0, 5))
				code.WeaponSet2 = LoadWeaponSet(ref rawSpan);
		}
		for(int i = 0; i < 5; i++)
			code.SlotSkills[i] = (SkillId)rawSpan.DecodeNext(24);

		code.Rune = (ItemId)rawSpan.DecodeNext(24);
		
		if(code.Kind != Kind.PvP)
			code.EquipmentAttributes = LoadAllEquipmentStats(ref rawSpan, code);
		else
			code.EquipmentAttributes.Amulet = (StatId)rawSpan.DecodeNext(16);

		if(code.Kind != Kind.PvP) {
			if(!rawSpan.EatIfExpected(0, 24))
				code.Infusions = LoadAllEquipmentInfusions(ref rawSpan, code);

			code.Food    = (ItemId)rawSpan.DecodeNext(24);
			code.Utility = (ItemId)rawSpan.DecodeNext(24);
		}
		code.ProfessionSpecific = LoadProfessionSpecific(ref rawSpan, code.Profession);
		code.Arbitrary          = LoadArbitrary(ref rawSpan);

		return code;
	}

	private static WeaponSet LoadWeaponSet(ref BitReader rawSpan)
	{
		var set = new WeaponSet();
		set.MainHand = WeaponType._FIRST + rawSpan.DecodeNext_WriteMinusMinIfAtLeast<int>(2, 5);
		set.Sigil1 = (ItemId)rawSpan.DecodeNext(24);
		if(set.MainHand != WeaponType._UNDEFINED && !IsTwoHanded(set.MainHand))
			set.OffHand = WeaponType._FIRST + rawSpan.DecodeNext_WriteMinusMinIfAtLeast<int>(2, 5);
		set.Sigil2 = (ItemId)rawSpan.DecodeNext(24);
		return set;
	}

	private static AllEquipmentStats LoadAllEquipmentStats(ref BitReader rawSpan, BuildCode weaponRef)
	{
		var allData = new AllEquipmentStats();

		var repeatCount = 0;
		var data = StatId._UNDEFINED;
		for(int i = 0; i < ALL_EQUIPMENT_COUNT; i++) {
			if(repeatCount == 0) {
				data = (StatId)rawSpan.DecodeNext(16);

				if(i == ALL_EQUIPMENT_COUNT - 1) repeatCount = 1;
				else repeatCount = rawSpan.DecodeNext(4) + 1;
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

	private static AllEquipmentInfusions LoadAllEquipmentInfusions(ref BitReader rawSpan, BuildCode weaponRef)
	{
		var allData = new AllEquipmentInfusions();

		var repeatCount = 0;
		var data = ItemId._UNDEFINED;
		for(int i = 0; i < ALL_INFUSION_COUNT; i++) {
			if(repeatCount == 0) {
				data = rawSpan.DecodeNext_WriteMinusMinIfAtLeast<ItemId>(1, 24);

				if(i == ALL_EQUIPMENT_COUNT - 1) repeatCount = 1;
				else repeatCount = rawSpan.DecodeNext(5) + 1;
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

	private static IProfessionSpecific LoadProfessionSpecific(ref BitReader rawSpan, Profession profession)
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
				data.Legend1 = (Legend)rawSpan.DecodeNext(4);
				if(!rawSpan.EatIfExpected(0, 4)){
					data.Legend2 = (Legend)rawSpan.DecodeNext(4);
					rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.AltUtilitySkill1, 1, 24);
					rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.AltUtilitySkill2, 1, 24);
					rawSpan.DecodeNext_WriteMinusMinIfAtLeast(ref data.AltUtilitySkill3, 1, 24);
				}
				return data;
			}

			default: return IProfessionSpecific.NONE.Instance;
		}
	}

	private static IArbitrary LoadArbitrary(ref BitReader rawSpan)
	{
		//implement extensions here in the future
		return IArbitrary.NONE.Instance;
	}

	/// <returns> The amount of bytes written. </returns>
	public static int WriteCode(BuildCode code, Span<byte> destination)
	{
		var rawBits = new BitWriter(destination);
		rawBits.Data[0] = (byte)('a' + code.Version);
		rawBits.BitPos += 8;
		
		rawBits.Write((code.Kind) switch {
			Kind.PvP => 0,
			Kind.WvW => 1,
			Kind.PvE => 2,
			_=> throw new ArgumentOutOfRangeException(nameof(code.Kind)),
		}, 2);
		
		rawBits.Write((int)code.Profession - 1, 4);


		for(int i = 0; i < 3; i++)
		{
			if(code.Specializations[i].SpecializationId == SpecializationId._UNDEFINED) rawBits.Write(0, 7);
			else
			{
				rawBits.Write((int)code.Specializations[i].SpecializationId, 7);
				for(int j = 0; j < 3; j++)
					rawBits.Write((int)code.Specializations[i].Choices[j], 2);
			}
		}

		if(!code.WeaponSet1.HasAny && !code.WeaponSet2.HasAny) rawBits.Write(0, 5);
		else
		{
			rawBits.Write(1 + (int)code.WeaponSet1.MainHand, 5);
			rawBits.Write((int)code.WeaponSet1.Sigil1, 24);
			rawBits.Write(1 + (int)code.WeaponSet1.OffHand, 5);
			rawBits.Write((int)code.WeaponSet1.Sigil2, 24);

			if(!code.WeaponSet2.HasAny) rawBits.Write(0, 5);
			else
			{
				rawBits.Write(1 + (int)code.WeaponSet2.MainHand, 5);
				rawBits.Write((int)code.WeaponSet2.Sigil1, 24);
				rawBits.Write(1 + (int)code.WeaponSet2.OffHand, 5);
				rawBits.Write((int)code.WeaponSet2.Sigil2, 24);
			}
		}

		for(int i = 0; i < 5; i++)
			rawBits.Write((int)code.SlotSkills[i], 24);

		rawBits.Write((int)code.Rune, 24);

		if(code.Kind == Kind.PvP) rawBits.Write((int)code.EquipmentAttributes.Amulet, 16);
		else
		{
			{
				StatId? lastStat = default;
				var repeatCount = 0;
				for(int i = 0; i < ALL_EQUIPMENT_COUNT; i++)
				{
					switch(i)
					{
						case 11:
							if(!code.WeaponSet1.HasAny) { i += 3; continue; }
							else if(code.WeaponSet1.MainHand == WeaponType._UNDEFINED) { continue; }
							else break;
						case 12:
							if(code.WeaponSet1.OffHand == WeaponType._UNDEFINED) continue;
							else break;
						case 13:
							if(!code.WeaponSet2.HasAny) { i++; continue; }
							else if(code.WeaponSet2.MainHand == WeaponType._UNDEFINED) continue;
							else break;
						case 14:
							if(code.WeaponSet2.OffHand == WeaponType._UNDEFINED) continue;
							else break;
					}

					if(code.EquipmentAttributes[i] != lastStat)
					{
						if(lastStat.HasValue)
						{
							rawBits.Write((int)lastStat.Value, 16);
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

				rawBits.Write((int)lastStat!.Value, 16);
				if(repeatCount > 1)
					rawBits.Write(repeatCount - 1, 4);
			}

			if(!code.Infusions.HasAny()) rawBits.Write(0, 24);
			else
			{
				var lastInfusion = ItemId._UNDEFINED;
				var repeatCount = 0;
				for(int i = 0; i < ALL_INFUSION_COUNT; i++)
				{
					switch(i)
					{
						case 16:
							if(!code.WeaponSet1.HasAny) { i += 3; continue; }
							else if(code.WeaponSet1.MainHand == WeaponType._UNDEFINED) { continue; }
							else break;
						case 17:
							if(code.WeaponSet1.OffHand == WeaponType._UNDEFINED) continue;
							else break;
						case 18:
							if(!code.WeaponSet2.HasAny) { i++; continue; }
							else if(code.WeaponSet2.MainHand == WeaponType._UNDEFINED) continue;
							else break;
						case 19:
							if(code.WeaponSet2.OffHand == WeaponType._UNDEFINED) continue;
							else break;
					}

					if(code.Infusions[i] != lastInfusion)
					{
						if(lastInfusion != ItemId._UNDEFINED)
						{
							rawBits.Write((int)lastInfusion + 1, 24);
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

				rawBits.Write((int)lastInfusion + 1, 24);
				if(repeatCount > 1)
					rawBits.Write(repeatCount - 1, 5);
			}

			rawBits.Write((int)code.Food, 24);
			rawBits.Write((int)code.Utility, 24);
		}

		switch(code.Profession)
		{
			case Profession.Ranger:
				var rangerData = (RangerData)code.ProfessionSpecific;
				if(rangerData.Pet1 == PetId._UNDEFINED && rangerData.Pet2 == PetId._UNDEFINED) rawBits.Write(0, 7);
				else
				{
					rawBits.Write(1 + (int)rangerData.Pet1, 7);
					rawBits.Write(1 + (int)rangerData.Pet2, 7);
				}
				break;

			case Profession.Revenant:
				var revenantData = (RevenantData)code.ProfessionSpecific;
				rawBits.Write((int)revenantData.Legend1, 4);
				if(revenantData.Legend2 == Legend._UNDEFINED) rawBits.Write(0, 4);
				else
				{
					rawBits.Write((int)revenantData.Legend2, 4);
					rawBits.Write((int)revenantData.AltUtilitySkill1, 24);
					rawBits.Write((int)revenantData.AltUtilitySkill2, 24);
					rawBits.Write((int)revenantData.AltUtilitySkill3, 24);
				}
				break;
		}

		return (rawBits.BitPos + 7) / 8;
	}

	#endregion

	#region official codes

	/// <remarks> Requires PerProfessionData to be loaded or <see cref="PerProfessionData.LazyLoadMode"/> to be set to something other than <see cref="LazyLoadMode.NONE"/>. </remarks>
	public static BuildCode LoadOfficialBuildCode(ReadOnlySpan<byte> raw, bool aquatic = false)
	{
		var codeType = SliceAndAdvance(ref raw);
		Debug.Assert(codeType == 0x0D);

		var code = new BuildCode();
		code.Version    = CURRENT_VERSION;
		code.Profession = (Profession)SliceAndAdvance(ref raw);

		if(PerProfessionData.LazyLoadMode >= LazyLoadMode.OFFLINE_ONLY) PerProfessionData.Reload(code.Profession, PerProfessionData.LazyLoadMode < LazyLoadMode.FULL).Wait();
		var professionData = PerProfessionData.ByProfession(code.Profession);

		for(int i = 0; i < 3; i++, raw = raw[2..]) {
			var spec = (SpecializationId)raw[0];
			if(spec == SpecializationId._UNDEFINED) continue;

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

		var offset = aquatic ? 2 : 0;
		var specRaw = raw[(5 * 4)..];

		switch(code.Profession)
		{
			case Profession.Ranger:
				specRaw = specRaw[offset..];

				var rangerData = new RangerData();
				if(specRaw[0] != 0) rangerData.Pet1 = (PetId)specRaw[0];
				if(specRaw[1] != 0) rangerData.Pet2 = (PetId)specRaw[1];

				code.ProfessionSpecific = rangerData;
				goto default;

			case Profession.Revenant:
				specRaw = specRaw[offset..];

				var revenantData = new RevenantData();

				if(specRaw[0] != 0)
				{
					revenantData.Legend1 = specRaw[0] - Legend._FIRST;

					for(int i = 0; i < 5; i++) {
						var palletteId = BinaryPrimitives.ReadUInt16LittleEndian(raw.Slice(i * 4  + offset, 2));
						if(palletteId != 0) code.SlotSkills[i] = Overrides.RevPalletteToSkill(revenantData.Legend1, palletteId);
					}
				}
				else
				{
					//NOTE(Rennorb): no legend available, here we can only guess the right skils.
					ReadSlotSkillsNormally(code, professionData, raw[offset..]);
				}

				if(specRaw[1] != 0)
				{
					revenantData.Legend2 = specRaw[1] - Legend._FIRST;
					var revSkillOffset = aquatic ? 6: 2;
					specRaw = specRaw[revSkillOffset..];

					Span<SkillId> altSkills = stackalloc SkillId[3] {
						Overrides.RevPalletteToSkill(revenantData.Legend2, BinaryPrimitives.ReadUInt16LittleEndian(specRaw[..2])),
						Overrides.RevPalletteToSkill(revenantData.Legend2, BinaryPrimitives.ReadUInt16LittleEndian(specRaw[2..4])),
						Overrides.RevPalletteToSkill(revenantData.Legend2, BinaryPrimitives.ReadUInt16LittleEndian(specRaw[4..6])),
					};

					if(specRaw[0] != 0)
					{
						if(altSkills[0] != 0) revenantData.AltUtilitySkill1 = altSkills[0];
						if(altSkills[1] != 0) revenantData.AltUtilitySkill2 = altSkills[1];
						if(altSkills[2] != 0) revenantData.AltUtilitySkill3 = altSkills[2];
					}
					else //flip skills so the first legend is always set
					{
						revenantData.Legend1 = revenantData.Legend2;
						revenantData.Legend2 = Legend._UNDEFINED;

						revenantData.AltUtilitySkill1 = code.SlotSkills.Utility1;
						revenantData.AltUtilitySkill2 = code.SlotSkills.Utility2;
						revenantData.AltUtilitySkill3 = code.SlotSkills.Utility3;

						for(int i = 0; i < 3; i++)
							if(altSkills[i] != 0)
								code.SlotSkills[1 + i] = altSkills[i];
					}
				}

				code.ProfessionSpecific = revenantData;
				break;

			default:
				ReadSlotSkillsNormally(code, professionData, raw[offset..]);
				break;
		}

		return code;
	}

	private static void ReadSlotSkillsNormally(BuildCode code, PerProfessionData skillData, ReadOnlySpan<byte> raw)
	{
		for(int i = 0; i < 5; i++) {
			var palletteId = BinaryPrimitives.ReadUInt16LittleEndian(raw.Slice(i * 4, 2));
			if(palletteId != 0) code.SlotSkills[i] = skillData.PalletteToSkill[palletteId];
		}
	}

	/// <remarks> Requires PerProfessionData to be loaded or <see cref="PerProfessionData.LazyLoadMode"/> to be set to something other than <see cref="LazyLoadMode.NONE"/>. </remarks>
	public static void WriteOfficialBuildCode(BuildCode code, Span<byte> destination, bool aquatic = false)
	{
		Debug.Assert(destination.Length >= OFFICIAL_CHAT_CODE_BYTE_LENGTH, "destination is not large enough to write code");
		if(PerProfessionData.LazyLoadMode >= LazyLoadMode.OFFLINE_ONLY) PerProfessionData.Reload(code.Profession, PerProfessionData.LazyLoadMode < LazyLoadMode.FULL).Wait();
		var professionData = PerProfessionData.ByProfession(code.Profession);

		destination[0] = 0x0d; //code type
		destination[1] = (byte)code.Profession;
		for(int i = 0; i < 3; i++) {
			if(code.Specializations[i].SpecializationId == SpecializationId._UNDEFINED) continue;

			var spec = code.Specializations[i];
			destination[2 + i * 2] = (byte)spec.SpecializationId;
			destination[3 + i * 2] = (byte)((int)spec.Choices[0] | ((int)spec.Choices[1] << 2) | ((int)spec.Choices[2] << 4));
		}

		var skillsDestination = destination[(aquatic ? 10 : 8)..];
		for(int i = 0; i < 5; i++) {
			ushort palletteIndex = 0;
			if(code.SlotSkills[i] != SkillId._UNDEFINED) palletteIndex = professionData.SkillToPallette[code.SlotSkills[i]];
			BinaryPrimitives.WriteUInt16LittleEndian(skillsDestination[(i * 4)..], palletteIndex);
		}

		var profSpecificDest = destination[28..];
		switch(code.Profession)
		{
			case Profession.Ranger:
				var rangerData = (RangerData)code.ProfessionSpecific;
				var offset = aquatic ? 2 : 0;
				profSpecificDest[offset + 0] = (byte)(1 + rangerData.Pet1);
				profSpecificDest[offset + 1] = (byte)(1 + rangerData.Pet2);
				break;

			case Profession.Revenant:
				var revenantData = (RevenantData)code.ProfessionSpecific;
				int legendOffset, skillOffset;
				if(aquatic) {
					legendOffset = 2;
					skillOffset  = 9;
				}
				else {
					legendOffset = 0;
					skillOffset  = 4;
				}

				profSpecificDest[legendOffset] = (byte)revenantData.Legend1;
				profSpecificDest[legendOffset + 1] = (byte)revenantData.Legend2;

				ushort altSkill1PalletteId = professionData.SkillToPallette[revenantData.AltUtilitySkill1];
				BinaryPrimitives.WriteUInt16LittleEndian(profSpecificDest[skillOffset..], altSkill1PalletteId);

				ushort altSkill2PalletteId = professionData.SkillToPallette[revenantData.AltUtilitySkill2];
				BinaryPrimitives.WriteUInt16LittleEndian(profSpecificDest[(skillOffset + 2)..], altSkill2PalletteId);

				ushort altSkill3PalletteId = professionData.SkillToPallette[revenantData.AltUtilitySkill3];
				BinaryPrimitives.WriteUInt16LittleEndian(profSpecificDest[(skillOffset + 4)..], altSkill3PalletteId);
				break;
		}
	}

	#endregion
}