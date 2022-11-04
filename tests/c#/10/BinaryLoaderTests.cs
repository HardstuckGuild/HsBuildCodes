using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.Binary;

public class FunctionTests {
	[Fact]
	public void DecodeByteValue()
	{
		var data = new byte[] { 0b00110011 };
		var bitspan = new BinaryLoader.BitSpan(data);
		Assert.Equal(3, bitspan.DecodeNext(4));
	}

	[Fact]
	public void SuccessiveDecodeByteValue()
	{
		var data = new byte[] { 0b0011_1100 };
		var bitspan = new BinaryLoader.BitSpan(data);
		Assert.Equal( 3, bitspan.DecodeNext(4));
		Assert.Equal(12, bitspan.DecodeNext(4));
	}

	[Fact]
	public void DecodeMultibyteValue()
	{
		var data = new byte[] { 0b0000_0000, 0b0001_0000 };
		var bitspan = new BinaryLoader.BitSpan(data);
		Assert.Equal(1, bitspan.DecodeNext(12));

		var data2 = new byte[] { 0b1000_0000, 0b0000_0000 };
		var bitspan2 = new BinaryLoader.BitSpan(data2);
		Assert.Equal(Math.Pow(2, 11), bitspan2.DecodeNext(12));
	}

	[Fact]
	public void DecodeMultibyteValue2()
	{
		var data = new byte[] { 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0010 };
		var bitspan = new BinaryLoader.BitSpan(data);
		bitspan.BitPos = 7;
		Assert.Equal(1, bitspan.DecodeNext(24));
	}

	[Fact]
	public void SuccessiveDecodeMultibyteValue()
	{
		var data = new byte[] { 0b00110011, 0b00110001 };
		var bitspan = new BinaryLoader.BitSpan(data);
		Assert.Equal(0b001100110011, bitspan.DecodeNext(12));
		Assert.Equal(1, bitspan.DecodeNext(4));
	}

	[Fact]
	public void SuccessiveDecodeValueCrossByteBoundary()
	{
		var data = new byte[] { 0b01010101, 0b10101010, 0b11110000 };
		var bitspan = new BinaryLoader.BitSpan(data);
		Assert.Equal(0b010101011010, bitspan.DecodeNext(12));
		Assert.Equal(0b101011, bitspan.DecodeNext(6));
	}

	[Fact]
	public void EatIfExpected()
	{
		var data = new byte[] { 0b00000011, 0b00110001 };
		var bitspan = new BinaryLoader.BitSpan(data);
		Assert.False(bitspan.EatIfExpected(8, 5));
		Assert.Equal(0, bitspan.BitPos);
		Assert.True(bitspan.EatIfExpected(0, 5));
		Assert.Equal(5, bitspan.BitPos);
	}
}

public class BasicCodeTests {
	static byte[] BitStringToBytes(string data) {
		var list  = new List<byte>(60);
		var counter = 0;
		byte current = 0;
		foreach(char c in data) {
			switch(c)
			{
				case '0':
					current <<= 1;
					counter++;
					break;

				case '1':
					current <<= 1;
					current |= 1;
					counter++;
					break;

				case >= 'a' and <= 'z' or (>= 'A' and <= 'Z'):
					if(counter != 0) Debug.Assert(false, "only on byte boundries");
					list.Add((byte)c);
					break;

			}

			if(counter == 8)
			{
				list.Add(current);
				current = 0;
				counter = 0;
			}
		}

		if(counter > 0) {
			list.Add((byte)(current << (8 - counter)));
		}

		return list.ToArray();
	}

	[Fact]
	public void HelperWorking()
	{
		var rawCode0 = BitStringToBytes("b01010101");
		Assert.Equal(new byte[] { (byte)'b', 0b01010101 }, rawCode0);

		var rawCode1 = BitStringToBytes("b010001");
		Assert.Equal(new byte[] { (byte)'b', 0b01000100 }, rawCode1);

		var rawCode2 = BitStringToBytes("b0100_01");
		Assert.Equal(new byte[] { (byte)'b', 0b01000100 }, rawCode2);
	}


	[Fact]
	public void ShouldThrowVersion()
	{
		var rawCode = new byte[80];
		Array.Fill(rawCode, (byte)0x2);
		rawCode[0] = (byte)'B';
		Assert.ThrowsAny<Exception>(() => {
			var code = BinaryLoader.LoadBuildCode(rawCode);
		});
	}

	[Fact]
	public void ShouldThrowTooShort()
	{
		var rawCode = new byte[2];
		Array.Fill(rawCode, (byte)0x2);
		rawCode[0] = (byte)'c';
		Assert.ThrowsAny<Exception>(() => {
			var code = BinaryLoader.LoadBuildCode(rawCode);
		});
	}

	[Fact]
	public void MinimalPvPWithSkills()
	{
		var rawCode = BitStringToBytes(
			"c" + //version
			"00" + //type
			"0000" + //profession
			"0000_0000_0000" + //traits
			"00000" + //weapons
			"000000000000000000000001" + //skills
			"000000000000000000000010" +
			"000000000000000000000011" +
			"000000000000000000000100" +
			"000000000000000000000101" +
			"000000000000000000000000" + //rune
			"0000000000000001" //stats (pvp)
		 );
		var code = BinaryLoader.LoadBuildCode(rawCode);
		Assert.Equal(2                  , code.Version);
		Assert.Equal(Kind.PvP           , code.Kind);
		Assert.Equal(Profession.Guardian, code.Profession);
		for(int i = 0; i < 3; i++)
			Assert.Null(code.Specializations[i]);
		Assert.False(code.WeaponSet1.HasAny);
		Assert.False(code.WeaponSet2.HasAny);
		for(int i = 0; i < 5; i++)
			Assert.Equal((SkillId)i, code.SlotSkills[i]);
		Assert.Null(code.Rune);
		for(int i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			if(i >= 11 && i <= 14) Assert.Null(code.EquipmentAttributes[i]);
			else if(i == Static.ALL_EQUIPMENT_COUNT - 1)  Assert.Equal((StatId)1, code.EquipmentAttributes[i]);
			else Assert.Equal(StatId._UNDEFINED, code.EquipmentAttributes[i]);
		}
		for(int i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			Assert.Null(code.Infusions[i]);
		Assert.Null(code.Food);
		Assert.Null(code.Utility);
		Assert.Equal(IProfessionSpecific.NONE.Instance, code.ProfessionSpecific);
		Assert.Equal(IArbitrary         .NONE.Instance, code.Arbitrary);
	}

	[Fact]
	public void MinimalPvE()
	{
		var rawCode = BitStringToBytes(
			"c" + //version
			"10" + //type
			"0000" + //profession
			"0000_0000_0000" + //traits
			"00000" + //weapons
			"000000000000000000000000" + //skills
			"000000000000000000000000" +
			"000000000000000000000000" +
			"000000000000000000000000" +
			"000000000000000000000000" +
			"000000000000000000000000" + //rune
			"0000000000000001" + //stats (pve)
			"1100" +
			"000000000000000000000000" + // infusions
			"000000000000000000000000" + // food
			"000000000000000000000000" // utility
		 );
		var code = BinaryLoader.LoadBuildCode(rawCode);
		Assert.Equal(2                  , code.Version);
		Assert.Equal(Kind.PvE           , code.Kind);
		Assert.Equal(Profession.Guardian, code.Profession);
		for(int i = 0; i < 3; i++)
			Assert.Null(code.Specializations[i]);
		Assert.False(code.WeaponSet1.HasAny);
		Assert.False(code.WeaponSet2.HasAny);
		for(int i = 0; i < 5; i++)
			Assert.Null(code.SlotSkills[i]);
		Assert.Null(code.Rune);
		for(int i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			if(11 <= i && i <= 14) Assert.Equal(default, code.EquipmentAttributes[i]);
			else Assert.Equal((StatId)1, code.EquipmentAttributes[i]);
		}
		for(int i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			Assert.Null(code.Infusions[i]);
		Assert.Null(code.Food);
		Assert.Null(code.Utility);
		Assert.Equal(IProfessionSpecific.NONE.Instance, code.ProfessionSpecific);
		Assert.Equal(IArbitrary         .NONE.Instance, code.Arbitrary);
	}

	[Fact]
	public void MinimalRanger()
	{
		var rawCode = BitStringToBytes(
			"c" + //version
			"10" + //type
			"0011" + //profession
			"0000_0000_0000" + //traits
			"00000" + //weapons
			"000000000000000000000000" + //skills
			"000000000000000000000000" +
			"000000000000000000000000" +
			"000000000000000000000000" +
			"000000000000000000000000" +
			"000000000000000000000000" + //rune
			"0000000000000001" + //stats (pve)
			"1100" +
			"000000000000000000000000" + // infusions
			"111100000000000000000110" + // food
			"111100000000000000000110" + // utility
			"0000000" // pets
		 );
		var code = BinaryLoader.LoadBuildCode(rawCode);
		Assert.Equal(Profession.Ranger, code.Profession);
		Assert.IsType<RangerData>(code.ProfessionSpecific);
		var data = (RangerData)code.ProfessionSpecific;
		Assert.Null(data.Pet1);
		Assert.Null(data.Pet2);
	}

	[Fact]
	public void MinimalRevenant()
	{
		var rawCode = BitStringToBytes(
			"c" + //version
			"10" + //type
			"1000" + //profession
			"0000_0000_0000" + //traits
			"00000" + //weapons
			"000000000000000000000000" + //skills
			"000000000000000000000000" +
			"000000000000000000000000" +
			"000000000000000000000000" +
			"000000000000000000000000" +
			"000000000000000000000000" + //rune
			"0000000000000001" + //stats (pve)
			"1100" +
			"000000000000000000000000" + // infusions
			"000000000000000000000000" + // food
			"000000000000000000000000" + // utility
			"0001_0000" // legends
		 );
		var code = BinaryLoader.LoadBuildCode(rawCode);
		Assert.Equal(Profession.Revenant, code.Profession);
		Assert.IsType<RevenantData>(code.ProfessionSpecific);
		var data = (RevenantData)code.ProfessionSpecific;
		Assert.Equal(Legend.SHIRO, data.Legend1);
		Assert.Null(data.Legend2);
		Assert.Null(data.AltUtilitySkill1);
		Assert.Null(data.AltUtilitySkill2);
		Assert.Null(data.AltUtilitySkill3);
	}
}

public class OfficialChatLinks
{
	[Fact]
	public void LoadOfficialLink()
	{
		ProfessionSkillPallettes.Reload(Profession.Necromancer, true);

		var fullLink = "[&DQg1KTIlIjbBEgAAgQB1AUABgQB1AUABlQCVAAAAAAAAAAAAAAAAAAAAAAA=]";
		var base64   = fullLink[2..^1];
		var raw = Convert.FromBase64String(base64);
		var code = BinaryLoader.LoadOfficialBuildCode(raw);
		Assert.Equal(Profession.Necromancer, code.Profession);

		Assert.Equal(SpecializationId.Spite, code.Specializations[0]!.Value.SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept       = TraitLineChoice.TOP,
			Master      = TraitLineChoice.MIDDLE,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[0]!.Value.Choices);

		Assert.Equal(SpecializationId.Soul_Reaping, code.Specializations[1]!.Value.SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept       = TraitLineChoice.TOP,
			Master      = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[1]!.Value.Choices);

		Assert.Equal(SpecializationId.Reaper, code.Specializations[2]!.Value.SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept       = TraitLineChoice.MIDDLE,
			Master      = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.BOTTOM,
		}, code.Specializations[2]!.Value.Choices);

		Assert.Equal(SkillId.Your_Soul_Is_Mine, code.SlotSkills[0]);
		Assert.Equal(SkillId.Well_of_Suffering1, code.SlotSkills[1]);
		Assert.Equal(SkillId.Well_of_Darkness1, code.SlotSkills[2]);
		Assert.Equal(SkillId.Signet_of_Spite, code.SlotSkills[3]);
		Assert.Equal(SkillId.Summon_Flesh_Golem, code.SlotSkills[4]);
	}

	[Fact]
	public void WriteOfficialLink()
	{
		var code = new BuildCode {
			Profession = Profession.Necromancer,
			Specializations = {
				Choice1 = new Specialization() {
					SpecializationId = SpecializationId.Spite,
					Choices          = {
						Adept        = TraitLineChoice.TOP,
						Master       = TraitLineChoice.MIDDLE,
						Grandmaster  = TraitLineChoice.MIDDLE,
					},
				},
				Choice2 = new Specialization() {
					SpecializationId = SpecializationId.Soul_Reaping,
					Choices          = {
						Adept        = TraitLineChoice.TOP,
						Master       = TraitLineChoice.TOP,
						Grandmaster  = TraitLineChoice.MIDDLE,
					},
				},
				Choice3 = new Specialization() {
					SpecializationId = SpecializationId.Reaper,
					Choices          = {
						Adept        = TraitLineChoice.MIDDLE,
						Master       = TraitLineChoice.TOP,
						Grandmaster  = TraitLineChoice.BOTTOM,
					},
				},
			},
			SlotSkills = {
				Heal = SkillId.Your_Soul_Is_Mine,
				Utility1 = SkillId.Well_of_Suffering1,
				Utility2 = SkillId.Well_of_Darkness1,
				Utility3 = SkillId.Signet_of_Spite,
				Elite   = SkillId.Summon_Flesh_Golem,
			}
		};

		ProfessionSkillPallettes.Reload(Profession.Necromancer, true);

		var buffer = new byte[44];
		BinaryLoader.WriteOfficialBuildCode(code, buffer);

		var reference = "[&DQg1KTIlIjbBEgAAgQAAAEABAAB1AQAAlQAAAAAAAAAAAAAAAAAAAAAAAAA=]";
		var referenceBase64 = reference[2..^1];
		var referenceBytes = Convert.FromBase64String(referenceBase64);

		Assert.Equal(referenceBytes, buffer);
	}

	[Fact]
	public void LoadOfficiaRevlLink() // our very special boy spec
	{
		ProfessionSkillPallettes.Reload(Profession.Revenant, true);

		var fullLink = "[&DQkAAAAARQDcEdwRAAAAACsSAADUEQAAAAAAAAQCAwDUESsSAAAAAAAAAAA=]";
		var base64   = fullLink[2..^1];
		var raw = Convert.FromBase64String(base64);
		var code = BinaryLoader.LoadOfficialBuildCode(raw);
		Assert.Equal(Profession.Revenant, code.Profession);
		Assert.Equal(SkillId.Empowering_Misery, code.SlotSkills[0]);
		Assert.Null(code.SlotSkills[1]);
		Assert.Equal(SkillId.Banish_Enchantment, code.SlotSkills[2]);
		Assert.Equal(SkillId.Call_to_Anguish1, code.SlotSkills[3]);
		Assert.Null(code.SlotSkills[4]);
	}
}

