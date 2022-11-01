using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Diagnostics;
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
		rawCode[0] = (byte)'b';
		Assert.ThrowsAny<Exception>(() => {
			var code = BinaryLoader.LoadBuildCode(rawCode);
		});
	}

	[Fact]
	public void MinimalPvPWithSkills()
	{
		var rawCode = BitStringToBytes(
			"b" + //version
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
		Assert.Equal(Profession.GUARDIAN, code.Profession);
		for(int i = 0; i < 3; i++)
			Assert.Null(code.Specializations[i]);
		Assert.False(code.Weapons.Land1.IsSet);
		Assert.False(code.Weapons.Land2.IsSet);
		Assert.False(code.Weapons.HasUnderwater);
		for(int i = 0; i < 5; i++)
			Assert.Equal((SkillId)i, code.SlotSkills[i]);
		Assert.Null(code.Rune);
		for(int i = 0; i < AllEquipmentData.ALL_EQUIPMENT_COUNT; i++) {
			if(11 <= i && i <= 16) Assert.Equal(default, code.EquipmentAttributes[i]);
			else Assert.Equal(1, code.EquipmentAttributes[i]);
		}
		for(int i = 0; i < AllEquipmentData.ALL_EQUIPMENT_COUNT; i++)
			Assert.Null(code.Infusions[i]);
		Assert.Null(code.Food);
		Assert.Null(code.Utility);
		Assert.Equal(IProfessionArbitrary.NONE.Instance, code.ArbitraryData.ProfessionSpecific);
		Assert.Equal(IArbitrary          .NONE.Instance, code.ArbitraryData.Arbitrary);
	}

	[Fact]
	public void MinimalPvE()
	{
		var rawCode = BitStringToBytes(
			"b" + //version
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
			"01101" +
			"000000000000000000000000" + // infusions
			"000000000000000000000000" + // food
			"000000000000000000000000" // utility
		 );
		var code = BinaryLoader.LoadBuildCode(rawCode);
		Assert.Equal(2                  , code.Version);
		Assert.Equal(Kind.PvE           , code.Kind);
		Assert.Equal(Profession.GUARDIAN, code.Profession);
		for(int i = 0; i < 3; i++)
			Assert.Null(code.Specializations[i]);
		Assert.False(code.Weapons.Land1.IsSet);
		Assert.False(code.Weapons.Land2.IsSet);
		Assert.False(code.Weapons.HasUnderwater);
		for(int i = 0; i < 5; i++)
			Assert.Null(code.SlotSkills[i]);
		Assert.Null(code.Rune);
		for(int i = 0; i < AllEquipmentData.ALL_EQUIPMENT_COUNT; i++) {
			if(11 <= i && i <= 16) Assert.Equal(default, code.EquipmentAttributes[i]);
			else Assert.Equal(1, code.EquipmentAttributes[i]);
		}
		for(int i = 0; i < AllEquipmentData.ALL_EQUIPMENT_COUNT; i++)
			Assert.Null(code.Infusions[i]);
		Assert.Null(code.Food);
		Assert.Null(code.Utility);
		Assert.Equal(IProfessionArbitrary.NONE.Instance, code.ArbitraryData.ProfessionSpecific);
		Assert.Equal(IArbitrary          .NONE.Instance, code.ArbitraryData.Arbitrary);
	}

	[Fact]
	public void MinimalRanger()
	{
		var rawCode = BitStringToBytes(
			"b" + //version
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
			"01101" +
			"000000000000000000000000" + // infusions
			"111100000000000000000110" + // food
			"111100000000000000000110" + // utility
			"0000000" + // land pets
			"0000000" // water pets
		 );
		var code = BinaryLoader.LoadBuildCode(rawCode);
		Assert.Equal(Profession.RANGER, code.Profession);
		Assert.IsType<RangerData>(code.ArbitraryData.ProfessionSpecific);
		var data = (RangerData)code.ArbitraryData.ProfessionSpecific;
		Assert.Null(data.Pet1);
		Assert.Null(data.Pet2);
	}

	[Fact]
	public void MinimalRevenant()
	{
		var rawCode = BitStringToBytes(
			"b" + //version
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
			"01101" +
			"000000000000000000000000" + // infusions
			"000000000000000000000000" + // food
			"000000000000000000000000" + // utility
			"0000_0000" // legends
		 );
		var code = BinaryLoader.LoadBuildCode(rawCode);
		Assert.Equal(Profession.REVENANT, code.Profession);
		Assert.IsType<RevenantData>(code.ArbitraryData.ProfessionSpecific);
		var data = (RevenantData)code.ArbitraryData.ProfessionSpecific;
		Assert.Null(data.Legend1);
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
		var fullLink = "[&DQkAAAAARQDcEdwRAAAAACsSAADUEQAAAAAAAAQCAwDUESsSAAAAAAAAAAA=]";
		var base64   = fullLink[2..^1];
		var raw = Convert.FromBase64String(base64);
		var code = BinaryLoader.LoadBuildCodeFromOfficialBuildCode(raw);
		Assert.Equal(Profession.REVENANT, code.Profession);
		Assert.Equal(SkillId.Empowering_Misery, code.SlotSkills[0]);
		Assert.Null(code.SlotSkills[1]);
		Assert.Equal(SkillId.Banish_Enchantment, code.SlotSkills[2]);
		Assert.Equal(SkillId.Call_to_Anguish1, code.SlotSkills[3]);
		Assert.Null(code.SlotSkills[4]);
	}
}

