using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.Text;

public class FunctionTests {
	[Fact]
	public void DecodeValueFixed()
	{
		for(int i = 0; i < 64; i++)
			Assert.Equal(i, TextLoader.INVERSE_CHARSET[TextLoader.CHARSET[i]]);
	}

	[Fact]
	public void SuccessiveDecodeAndEatValueFixed()
	{
		var text = "Aa-".AsSpan();
		Assert.Equal( 0, TextLoader.DecodeAndAdvance(ref text));
		Assert.Equal(26, TextLoader.DecodeAndAdvance(ref text));
		Assert.Equal(63, TextLoader.DecodeAndAdvance(ref text));
		Assert.Equal(0, text.Length);
	}

	[Fact]
	public void SuccessiveDecodeAndEatValueValirable()
	{
		var text = "Aa-".AsSpan();
		Assert.Equal( 0, TextLoader.DecodeAndAdvance(ref text, 1));
		Assert.Equal(26, TextLoader.DecodeAndAdvance(ref text, 1));
		Assert.Equal(63, TextLoader.DecodeAndAdvance(ref text, 1));
		Assert.Equal(0, text.Length);
	}

	[Fact]
	public void DecodeAndEatValueEarlyTerm()
	{
		var text = "A~".AsSpan();
		Assert.Equal(0, TextLoader.DecodeAndAdvance(ref text, 3));
		Assert.Equal(0, text.Length);
	}
}

public class BasicCodesTests {
	[Fact]
	public void ShouldThrowVersion()
	{
		Assert.ThrowsAny<Exception>(() => {
			var code = TextLoader.LoadBuildCode(TestUtilities.CodesInvalid["wrong-version"]);
		});
	}

	[Fact]
	public void ShouldThrowTooShort()
	{
		Assert.ThrowsAny<Exception>(() => {
			var code = TextLoader.LoadBuildCode(TestUtilities.CodesInvalid["too-short"]);
		});
	}

	[Fact]
	public void ShouldThrowInvalidCharacters()
	{
		Assert.ThrowsAny<Exception>(() => {
			var code = TextLoader.LoadBuildCode(TestUtilities.CodesInvalid["invalid-chars"]);
		});
	}

	[Fact]
	public void MinimalPvP()
	{
		var code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["minimal-pvp"]);
		Assert.Equal(3                  , code.Version);
		Assert.Equal(Kind.PvP           , code.Kind);
		Assert.Equal(Profession.Guardian, code.Profession);
		for(int i = 0; i < 3; i++)
			Assert.Equal(SpecializationId._UNDEFINED, code.Specializations[i].SpecializationId);
		Assert.False(code.WeaponSet1.HasAny);
		Assert.False(code.WeaponSet2.HasAny);
		for(int i = 0; i < 5; i++)
			Assert.Equal(SkillId._UNDEFINED, code.SlotSkills[i]);
		Assert.Equal(ItemId._UNDEFINED, code.Rune);
		for(int i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			if(i >= 11 && i <= 14) Assert.Equal(StatId._UNDEFINED, code.EquipmentAttributes[i]);
			else if(i == Static.ALL_EQUIPMENT_COUNT - 1) Assert.Equal((StatId)1, code.EquipmentAttributes[i]); 
			else Assert.Equal(StatId._UNDEFINED, code.EquipmentAttributes[i]);
		}
		for(int i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			Assert.Equal(ItemId._UNDEFINED, code.Infusions[i]);
		Assert.Equal(ItemId._UNDEFINED, code.Food);
		Assert.Equal(ItemId._UNDEFINED, code.Utility);
		Assert.Equal(IProfessionSpecific.NONE.Instance, code.ProfessionSpecific);
		Assert.Equal(IArbitrary         .NONE.Instance, code.Arbitrary);
	}

	[Fact]
	public void MinimalPvE()
	{
		var code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["minimal-pve"]);
		Assert.Equal(3                  , code.Version);
		Assert.Equal(Kind.PvE           , code.Kind);
		Assert.Equal(Profession.Guardian, code.Profession);
		for(int i = 0; i < 3; i++)
			Assert.Equal(SpecializationId._UNDEFINED, code.Specializations[i].SpecializationId);
		Assert.False(code.WeaponSet1.HasAny);
		Assert.False(code.WeaponSet2.HasAny);
		for(int i = 0; i < 5; i++)
			Assert.Equal(SkillId._UNDEFINED, code.SlotSkills[i]);
		Assert.Equal(ItemId._UNDEFINED, code.Rune);
		for(int i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			if(11 <= i && i <= 14) Assert.Equal(default, code.EquipmentAttributes[i]);
			else Assert.Equal((StatId)1, code.EquipmentAttributes[i]);
		}
		for(int i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			Assert.Equal(ItemId._UNDEFINED, code.Infusions[i]);
		Assert.Equal(ItemId._UNDEFINED, code.Food);
		Assert.Equal(ItemId._UNDEFINED, code.Utility);
		Assert.Equal(IProfessionSpecific.NONE.Instance, code.ProfessionSpecific);
		Assert.Equal(IArbitrary         .NONE.Instance, code.Arbitrary);
	}

	[Fact]
	public void MinimalRanger()
	{
		var code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["minimal-ranger"]);
		Assert.IsType<RangerData>(code.ProfessionSpecific);
		var data = (RangerData)code.ProfessionSpecific;
		Assert.Equal(PetId._UNDEFINED, data.Pet1);
		Assert.Equal(PetId._UNDEFINED, data.Pet2);
	}

	[Fact]
	public void MinimalRevenant()
	{
		var code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["minimal-revenant"]);
		Assert.IsType<RevenantData>(code.ProfessionSpecific);
		var data = (RevenantData)code.ProfessionSpecific;
		Assert.Equal(Legend.SHIRO, data.Legend1);
		Assert.Equal(Legend._UNDEFINED, data.Legend2);
		Assert.Equal(SkillId._UNDEFINED, data.AltUtilitySkill1);
		Assert.Equal(SkillId._UNDEFINED, data.AltUtilitySkill2);
		Assert.Equal(SkillId._UNDEFINED, data.AltUtilitySkill3);
	}

	[Fact]
	public void CycleBasicCode()
	{
		var text1 = TestUtilities.CodesV2["minimal-revenant"];
		var code = TextLoader.LoadBuildCode(text1);
		var text2 = TextLoader.WriteBuildCode(code);
		Assert.Equal(text1, text2);
	}

	[Fact]
	public void MidNecro()
	{
		var code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["mid-necro"]);
		Assert.Equal(Profession.Necromancer, code.Profession);

		Assert.Equal(WeaponType._UNDEFINED, code.WeaponSet1.MainHand);
		Assert.Equal(WeaponType._UNDEFINED, code.WeaponSet1.OffHand);
		Assert.Equal(WeaponType._UNDEFINED, code.WeaponSet2.MainHand);
		Assert.Equal(WeaponType._UNDEFINED, code.WeaponSet2.OffHand);

		Assert.Equal(ItemId._UNDEFINED, code.Rune);
		for(var i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			Assert.Equal(StatId._UNDEFINED, code.EquipmentAttributes[i]);
		}

		Assert.Equal(SpecializationId.Spite, code.Specializations[0].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.TOP,
			Master = TraitLineChoice.MIDDLE,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[0].Choices);

		Assert.Equal(SpecializationId.Soul_Reaping, code.Specializations[1].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.TOP,
			Master = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[1].Choices);

		Assert.Equal(SpecializationId.Reaper, code.Specializations[2].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.MIDDLE,
			Master = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.BOTTOM,
		}, code.Specializations[2].Choices);

		Assert.Equal(SkillId.Your_Soul_Is_Mine, code.SlotSkills[0]);
		Assert.Equal(SkillId.Well_of_Suffering1, code.SlotSkills[1]);
		Assert.Equal(SkillId.Well_of_Darkness1, code.SlotSkills[2]);
		Assert.Equal(SkillId.Signet_of_Spite, code.SlotSkills[3]);
		Assert.Equal(SkillId.Summon_Flesh_Golem, code.SlotSkills[4]);
	}

	[Fact]
	public void FullNecro()
	{
		var code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["full-necro"]);
		Assert.Equal(Profession.Necromancer, code.Profession);

		Assert.Equal(WeaponType.Axe    , code.WeaponSet1.MainHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Paralyzation, code.WeaponSet1.Sigil1);
		Assert.Equal(WeaponType.Dagger , code.WeaponSet1.OffHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Paralyzation, code.WeaponSet1.Sigil2);
		Assert.Equal(WeaponType.Scepter, code.WeaponSet2.MainHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Paralyzation, code.WeaponSet2.Sigil1);
		Assert.Equal(WeaponType.Focus  , code.WeaponSet2.OffHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Paralyzation, code.WeaponSet2.Sigil2);

		Assert.Equal(ItemId.Superior_Rune_of_the_Scholar, code.Rune);
		var berserkers = new []{ StatId.Berserkers1, StatId.Berserkers2, StatId.Berserkers3, StatId.Berserkers4, StatId.Berserkers5};
		for(var i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			Assert.Contains(code.EquipmentAttributes[i], berserkers);
		}

		for(var i = 0; i < Static.ALL_INFUSION_COUNT; i++) {
			Assert.Equal(ItemId.Agony_Infusion_05_Mighty, code.Infusions[i]);
		}

		Assert.Equal(SpecializationId.Spite, code.Specializations[0].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.TOP,
			Master = TraitLineChoice.MIDDLE,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[0].Choices);

		Assert.Equal(SpecializationId.Soul_Reaping, code.Specializations[1].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.TOP,
			Master = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[1].Choices);

		Assert.Equal(SpecializationId.Reaper, code.Specializations[2].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.MIDDLE,
			Master = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.BOTTOM,
		}, code.Specializations[2].Choices);

		Assert.Equal(SkillId.Your_Soul_Is_Mine, code.SlotSkills[0]);
		Assert.Equal(SkillId.Well_of_Suffering1, code.SlotSkills[1]);
		Assert.Equal(SkillId.Well_of_Darkness1, code.SlotSkills[2]);
		Assert.Equal(SkillId.Signet_of_Spite, code.SlotSkills[3]);
		Assert.Equal(SkillId.Summon_Flesh_Golem, code.SlotSkills[4]);

		Assert.Equal(ItemId.Bowl_of_Sweet_and_spicy_Butternut_Squash_Soup, code.Food);
		Assert.Equal(ItemId.Tin_of_Fruitcake, code.Utility);
	}

	public static IEnumerable<object[]> AllCodesProvider() => TestUtilities.CodesV2.Where(a => !a.Key.Contains("binary", StringComparison.OrdinalIgnoreCase)).Select(a => new [] { a.Key, a.Value });

	[Theory] [MemberData(nameof(AllCodesProvider))]
	public void ParseAll(string name, string code)
	{
		var code_ = TextLoader.LoadBuildCode(code);
		Assert.True(true);
	}

	[Theory] [MemberData(nameof(AllCodesProvider))]
	public void ReencodeAll(string name, string code)
	{
		var code_ = TextLoader.LoadBuildCode(code);
		var reencoded = TextLoader.WriteBuildCode(code_);
		Assert.Equal(code, reencoded);
	}

	[Fact]
	public void FullNecroCompressed()
	{
		var code = TextLoader.LoadBuildCode(TestUtilities.CodesV2["full-necro-binary"]);

		Assert.Equal(Profession.Necromancer, code.Profession);

		Assert.Equal(WeaponType.Axe, code.WeaponSet1.MainHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Paralyzation, code.WeaponSet1.Sigil1);
		Assert.Equal(WeaponType.Dagger, code.WeaponSet1.OffHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Paralyzation, code.WeaponSet1.Sigil2);
		Assert.Equal(WeaponType.Scepter, code.WeaponSet2.MainHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Paralyzation, code.WeaponSet2.Sigil1);
		Assert.Equal(WeaponType.Focus, code.WeaponSet2.OffHand);
		Assert.Equal(ItemId.Legendary_Sigil_of_Paralyzation, code.WeaponSet2.Sigil2);

		Assert.Equal(ItemId.Superior_Rune_of_the_Scholar, code.Rune);
		var berserkers = new []{ StatId.Berserkers1, StatId.Berserkers2, StatId.Berserkers3, StatId.Berserkers4, StatId.Berserkers5};
		for(var i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++)
		{
			Assert.Contains(code.EquipmentAttributes[i], berserkers);
		}

		for(var i = 0; i < Static.ALL_INFUSION_COUNT; i++)
		{
			Assert.Equal(ItemId.Agony_Infusion_05_Mighty, code.Infusions[i]);
		}

		Assert.Equal(SpecializationId.Spite, code.Specializations[0].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.TOP,
			Master = TraitLineChoice.MIDDLE,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[0].Choices);

		Assert.Equal(SpecializationId.Soul_Reaping, code.Specializations[1].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.TOP,
			Master = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[1].Choices);

		Assert.Equal(SpecializationId.Reaper, code.Specializations[2].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.MIDDLE,
			Master = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.BOTTOM,
		}, code.Specializations[2].Choices);

		Assert.Equal(SkillId.Your_Soul_Is_Mine, code.SlotSkills[0]);
		Assert.Equal(SkillId.Well_of_Suffering1, code.SlotSkills[1]);
		Assert.Equal(SkillId.Well_of_Darkness1, code.SlotSkills[2]);
		Assert.Equal(SkillId.Signet_of_Spite, code.SlotSkills[3]);
		Assert.Equal(SkillId.Summon_Flesh_Golem, code.SlotSkills[4]);

		Assert.Equal(ItemId.Bowl_of_Sweet_and_spicy_Butternut_Squash_Soup, code.Food);
		Assert.Equal(ItemId.Tin_of_Fruitcake, code.Utility);
	}

	[Fact] /* regression: mixed infusions w+w/ empty slots in the middle */
	public void Plenyx1()
	{
		var code = new BuildCode() {
			EquipmentAttributes	  = {
				Accessory1  = StatId.Celestial2,
				Accessory2  = StatId.Celestial2,
				Amulet      = StatId.Celestial2,
				BackItem   = StatId.Celestial2,
				Boots  = StatId.Celestial1,
				Chest  = StatId.Celestial1,
				Gloves  = StatId.Celestial1,
				Helmet  = StatId.Celestial1,
				Leggings  = StatId.Celestial1,
				Ring1  = StatId.Celestial2,
				Ring2  = StatId.Celestial2,
				Shoulders  = StatId.Celestial1,
				WeaponSet1MainHand  = StatId.Celestial1,
				WeaponSet1OffHand  = StatId._UNDEFINED,
				WeaponSet2MainHand  = StatId.Celestial1,
				WeaponSet2OffHand  = StatId.Celestial1,
			},
			Infusions  = {
				Helmet  = ItemId.Agony_Infusion_09,
				Shoulders  = ItemId.Agony_Infusion_09,
				Chest  = ItemId.Agony_Infusion_09,
				Gloves  = ItemId.Agony_Infusion_09,
				Leggings  = ItemId.Agony_Infusion_09,
				Boots  = ItemId.Agony_Infusion_09,

				BackItem_1  = ItemId.Agony_Infusion_09,
				BackItem_2  = ItemId._UNDEFINED,
				Accessory1  = ItemId.WvW_Infusion_Expertise,
				Accessory2  = ItemId.WvW_Infusion_Expertise,

				Ring1_1  = ItemId.Agony_Infusion_09,
				Ring1_2  = ItemId.Agony_Infusion_09,
				Ring1_3  = ItemId.Agony_Infusion_09,
				Ring2_1  = ItemId.Agony_Infusion_09,
				Ring2_2  = ItemId.Agony_Infusion_09,
				Ring2_3  = ItemId.Agony_Infusion_09,

				WeaponSet1_1  = ItemId._UNDEFINED,
				WeaponSet1_2  = ItemId._UNDEFINED,
				WeaponSet2_1  = ItemId._UNDEFINED,
				WeaponSet2_2  = ItemId._UNDEFINED,

				Amulet  = ItemId.WxP_Enrichment,
			},
			Kind  = Kind.PvE,
			Profession  = Profession.Warrior,
			Version  = 3,
			WeaponSet1  = {
				MainHand  = WeaponType.Hammer,
				OffHand  = WeaponType._UNDEFINED,
				Sigil1  = ItemId.Legendary_Sigil_of_Transference,
				Sigil2  = ItemId.Legendary_Sigil_of_Renewal,
			},
			WeaponSet2  = {
				MainHand  = WeaponType.Sword,
				OffHand  = WeaponType.Warhorn,
				Sigil1  = ItemId.Legendary_Sigil_of_Transference,
				Sigil2  = ItemId.Legendary_Sigil_of_Energy,
			}
		};

		var text = TextLoader.WriteBuildCode(code);
		var reencode = TextLoader.LoadBuildCode(text);

		var options = new JsonSerializerOptions() { IncludeFields = true, };
		Assert.Equal(JsonSerializer.Serialize(code, options), JsonSerializer.Serialize(reencode, options));
	}

	[Fact] /* regression: specific infusion orders break encoding */
	public void Plenyx2()
	{
		var code = new BuildCode();
		code.EquipmentAttributes.Helmet             = StatId.Berserkers1;
		code.EquipmentAttributes.Shoulders          = StatId.Berserkers1;
		code.EquipmentAttributes.Chest              = StatId.Berserkers1;
		code.EquipmentAttributes.Gloves             = StatId.Berserkers1;
		code.EquipmentAttributes.Leggings           = StatId.Berserkers1;
		code.EquipmentAttributes.Boots              = StatId.Berserkers1;
		code.EquipmentAttributes.BackItem           = StatId.Berserkers2;
		code.EquipmentAttributes.Accessory1         = StatId.Berserkers2;
		code.EquipmentAttributes.Accessory2         = StatId.Berserkers2;
		code.EquipmentAttributes.Ring1              = StatId.Berserkers2;
		code.EquipmentAttributes.Ring2              = StatId.Berserkers2;
		code.EquipmentAttributes.WeaponSet1MainHand = StatId.Berserkers1;
		code.EquipmentAttributes.WeaponSet1OffHand  = StatId._UNDEFINED;
		code.EquipmentAttributes.WeaponSet2MainHand = StatId._UNDEFINED;
		code.EquipmentAttributes.WeaponSet2OffHand  = StatId._UNDEFINED;
		code.EquipmentAttributes.Amulet             = StatId.Berserkers2;
		code.Infusions.Helmet       = ItemId.Agony_Infusion_07_Mighty;
		code.Infusions.Shoulders    = ItemId.Agony_Infusion_07_Precise;
		code.Infusions.Chest        = ItemId.Agony_Infusion_07_Precise;
		code.Infusions.Gloves       = ItemId.Agony_Infusion_07_Precise;
		code.Infusions.Leggings     = ItemId.Agony_Infusion_07_Precise;
		code.Infusions.Boots        = ItemId.Agony_Infusion_07_Precise;
		code.Infusions.BackItem_1   = ItemId.WvW_Infusion_Concentration;
		code.Infusions.BackItem_2   = ItemId._UNDEFINED;
		code.Infusions.Accessory1   = ItemId.WvW_Infusion_Concentration;
		code.Infusions.Accessory2   = ItemId.WvW_Infusion_Precise;
		code.Infusions.Ring1_1      = ItemId.WvW_Infusion_Concentration;
		code.Infusions.Ring1_2      = ItemId.WvW_Infusion_Concentration;
		code.Infusions.Ring1_3      = ItemId.WvW_Infusion_Concentration;
		code.Infusions.Ring2_1      = ItemId.WvW_Infusion_Concentration;
		code.Infusions.Ring2_2      = ItemId.WvW_Infusion_Concentration;
		code.Infusions.Ring2_3      = ItemId.WvW_Infusion_Concentration;
		code.Infusions.WeaponSet1_1 = ItemId.WvW_Infusion_Concentration;
		code.Infusions.WeaponSet1_2 = ItemId.WvW_Infusion_Concentration;
		code.Infusions.WeaponSet2_1 = ItemId._UNDEFINED;
		code.Infusions.WeaponSet2_2 = ItemId._UNDEFINED;
		code.Infusions.Amulet       = ItemId._UNDEFINED;
		code.Kind = Kind.PvE;
		code.Profession = Profession.Engineer;
		code.Version = 3;
		code.WeaponSet1.MainHand = WeaponType.Rifle;
		code.WeaponSet1.OffHand  = WeaponType._UNDEFINED;
		code.WeaponSet1.Sigil1   = ItemId.Legendary_Sigil_of_Force;
		code.WeaponSet1.Sigil2   = ItemId.Legendary_Sigil_of_the_Night;
		code.WeaponSet2.MainHand = WeaponType._UNDEFINED;
		code.WeaponSet2.OffHand  = WeaponType._UNDEFINED;
		code.WeaponSet2.Sigil1   = ItemId._UNDEFINED;
		code.WeaponSet2.Sigil2   = ItemId._UNDEFINED;

		var text = TextLoader.WriteBuildCode(code);
		var reencode = TextLoader.LoadBuildCode(text);

		var options = new JsonSerializerOptions() { IncludeFields = true, };
		Assert.Equal(JsonSerializer.Serialize(code, options), JsonSerializer.Serialize(reencode, options));
	}

	[Fact] /* regression: encoder did not properly handle empty second weapon*/
	public void Plenyx3()
	{
		var code = new BuildCode();
		code.Kind = Kind.PvE;
		code.Version = 3;
		code.Profession = Profession.Warrior;
		code.WeaponSet1.MainHand = WeaponType.Rifle;
		code.WeaponSet1.OffHand  = WeaponType._UNDEFINED;
		code.WeaponSet1.Sigil1   = ItemId.Legendary_Sigil_of_Force;
		code.WeaponSet1.Sigil2   = ItemId.Legendary_Sigil_of_the_Night;
		code.WeaponSet2.MainHand = WeaponType.Scepter;
		code.WeaponSet2.OffHand  = WeaponType._UNDEFINED;
		code.WeaponSet2.Sigil1   = ItemId.Superior_Sigil_of_Absorption;
		code.WeaponSet2.Sigil2   = ItemId._UNDEFINED;

		var text = TextLoader.WriteBuildCode(code);
		var reencode = TextLoader.LoadBuildCode(text);

		var options = new JsonSerializerOptions() { IncludeFields = true, };
		Assert.Equal(JsonSerializer.Serialize(code, options), JsonSerializer.Serialize(reencode, options));
	}
}

public class OfficialChatLinks {
	[Theory] [InlineData(true)] [InlineData(false)]
	public async Task LoadOfficialLink(bool lazyload)
	{
		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Necromancer, true);

		var code = TextLoader.LoadOfficialBuildCode(TestUtilities.CodesIngame["full-necro"]);
		Assert.Equal(Profession.Necromancer, code.Profession);

		Assert.Equal(SpecializationId.Spite, code.Specializations[0].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.TOP,
			Master = TraitLineChoice.MIDDLE,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[0].Choices);

		Assert.Equal(SpecializationId.Soul_Reaping, code.Specializations[1].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.TOP,
			Master = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[1].Choices);

		Assert.Equal(SpecializationId.Reaper, code.Specializations[2].SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.MIDDLE,
			Master = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.BOTTOM,
		}, code.Specializations[2].Choices);

		Assert.Equal(SkillId.Your_Soul_Is_Mine, code.SlotSkills[0]);
		Assert.Equal(SkillId.Well_of_Suffering1, code.SlotSkills[1]);
		Assert.Equal(SkillId.Well_of_Darkness1, code.SlotSkills[2]);
		Assert.Equal(SkillId.Signet_of_Spite, code.SlotSkills[3]);
		Assert.Equal(SkillId.Summon_Flesh_Golem, code.SlotSkills[4]);
	}

	[Theory] [InlineData(true)] [InlineData(false)]
	public async Task WriteOfficialLink(bool lazyload)
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

		if(lazyload) PerProfessionData.LazyLoadMode = LazyLoadMode.OFFLINE_ONLY;
		else await PerProfessionData.Reload(Profession.Necromancer, true);

		var reference = TestUtilities.CodesIngame["full-necro2"];
		var result = TextLoader.WriteOfficialBuildCode(code);
		Assert.Equal(reference, result);
	}
}
