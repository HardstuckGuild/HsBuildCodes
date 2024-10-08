using static Hardstuck.GuildWars2.BuildCodes.V2.APICache;
using Hardstuck.GuildWars2.BuildCodes.V2.OfficialAPI;
using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.API;

public class FunctionTests {
	public const string VALID_KEY = "92CE5A6C-E594-9D4D-B92B-5621ACFE047D436C02BD-0810-47D9-B9D4-2620EB7DD598";
	public const string UMLAUT_KEY = "D95CE863-D1B6-284F-B347-4B66C993759EDD490996-37AE-4E71-839A-DA51A0B6D40B";
	public const string MISSING_PERMS_KEY = "AD041D99-AEEF-2E45-8732-0057285EFE370740BF1D-6427-4191-8C4F-84DD1C97F05F";

	[Fact]
	public async Task ShouldThrowNotAToken()
	{
		await Assert.ThrowsAsync<InvalidAccessTokenException>(async () => {
			var code = await APILoader.LoadBuildCode("xxx", "sss", default);
		});
	}

	[Fact]
	public async Task ShouldThrowInvalidScopes()
	{
		await Assert.ThrowsAsync<MissingScopesException>(async () => {
			var code = await APILoader.LoadBuildCode(MISSING_PERMS_KEY, "sss", default);
		});
	}

	[Fact]
	public async Task ShouldFindMissingScopes()
	{
		var missingScopes = await APILoader.ValidateScopes(MISSING_PERMS_KEY);

		Assert.Equal(new[] {
			Permission.Characters, Permission.Builds,
		}, missingScopes);
	}

	[Fact]
	public async Task ShouldThrowNoSuchCharacter()
	{
		await Assert.ThrowsAsync<NotFoundException>(async () => {
			var code = await APILoader.LoadBuildCode(VALID_KEY, "does not exist", default);
		});
	}
}

public class BasicCodesTests {
	[Fact(Skip = "Teapot keeps changing the build")]
	public async Task LoadBuild()
	{
		var code = await APILoader.LoadBuildCode(FunctionTests.VALID_KEY, "Hardstuck Thief", default);
		Assert.Equal(Profession.Thief, code.Profession);
		Assert.Equal(new Specialization() {
			SpecializationId = SpecializationId.Deadly_Arts,
			Choices = {
				Adept       = TraitLineChoice.BOTTOM,
				Master      = TraitLineChoice.MIDDLE,
				Grandmaster = TraitLineChoice.TOP,
			}
		}, code.Specializations[0]);
		Assert.Equal(new Specialization() {
			SpecializationId = SpecializationId.Trickery,
			Choices = {
				Adept       = TraitLineChoice.BOTTOM,
				Master      = TraitLineChoice.TOP,
				Grandmaster = TraitLineChoice.TOP,
			}
		}, code.Specializations[1]);
		Assert.Equal(new Specialization() {
			SpecializationId = SpecializationId.Specter,
			Choices = {
				Adept       = TraitLineChoice.BOTTOM,
				Master      = TraitLineChoice.BOTTOM,
				Grandmaster = TraitLineChoice.TOP,
			}
		}, code.Specializations[2]);

		Assert.Equal(WeaponType.Scepter, code.WeaponSet1.MainHand);
		Assert.Equal(WeaponType.Dagger , code.WeaponSet1.OffHand);
		Assert.Equal(WeaponType._UNDEFINED, code.WeaponSet2.MainHand);
		Assert.Equal(WeaponType.Pistol , code.WeaponSet2.OffHand);

		Assert.Equal(ItemId.Legendary_Sigil_of_Demons, code.WeaponSet1.Sigil1);
		Assert.Equal(ItemId.Legendary_Sigil_of_Concentration, code.WeaponSet1.Sigil2);
		Assert.Equal(ItemId._UNDEFINED, code.WeaponSet2.Sigil1);
		Assert.Equal(ItemId.Legendary_Sigil_of_Paralyzation, code.WeaponSet2.Sigil2);

		var celestialStatsKEKW = new StatId[]{ StatId.Celestial1, StatId.Celestial2, StatId.Celestial3, StatId.Celestial4 };
		for(var i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++)
			if(i != 13) // empty second main hand
				Assert.Contains(code.EquipmentAttributes[i], celestialStatsKEKW);

		Assert.Equal(SkillId.Well_of_Gloom  , code.SlotSkills.Heal);
		Assert.Equal(SkillId.Well_of_Silence, code.SlotSkills.Utility1);
		Assert.Equal(SkillId.Well_of_Bounty , code.SlotSkills.Utility2);
		Assert.Equal(SkillId.Well_of_Sorrow , code.SlotSkills.Utility3);
		Assert.Equal(SkillId.Shadowfall     , code.SlotSkills.Elite);

		Assert.Equal(ItemId.Legendary_Rune_of_the_Traveler, code.Rune);
	}

	[Fact]
	public async Task LoadCharacterWithUmlaut()
	{
		var code = await APILoader.LoadBuildCode(FunctionTests.UMLAUT_KEY, "Brönski Van Gönski", default);
	}

	[Fact(Skip = "Teapot keeps changing the build")] /* regression: revenant skills would always show the alliance stance*/
	public async Task Teapot1()
	{
		var code = await APILoader.LoadBuildCode(FunctionTests.VALID_KEY, "Hardstuck Revenant", default);
		var altSkills = Static.ResolveAltRevSkills((RevenantData)code.ProfessionSpecific);
		if(code.SlotSkills.Heal != SkillId.Facet_of_Light)
			(code.SlotSkills, altSkills) = (altSkills, code.SlotSkills);

		Assert.Equal(SkillId.Facet_of_Light   , code.SlotSkills.Heal);
		Assert.Equal(SkillId.Facet_of_Darkness, code.SlotSkills.Utility1);
		Assert.Equal(SkillId.Facet_of_Elements, code.SlotSkills.Utility2);
		Assert.Equal(SkillId.Facet_of_Strength, code.SlotSkills.Utility3);
		Assert.Equal(SkillId.Facet_of_Chaos   , code.SlotSkills.Elite);

		Assert.Equal(SkillId.Empowering_Misery   , altSkills.Heal);
		Assert.Equal(SkillId.Pain_Absorption     , altSkills.Utility1);
		Assert.Equal(SkillId.Banish_Enchantment  , altSkills.Utility2);
		Assert.Equal(SkillId.Call_to_Anguish1    , altSkills.Utility3);
		Assert.Equal(SkillId.Embrace_the_Darkness, altSkills.Elite);
	}

	[Fact] /* spears should just work */
	public async Task LandSpears()
	{
		var code = await APILoader.LoadBuildCode(FunctionTests.VALID_KEY, "Hardstuck Revenant", default);
		
		WeaponSet set;
		if(code.WeaponSet1.MainHand == WeaponType.Spear)   set = code.WeaponSet1;
		else if(code.WeaponSet2.MainHand == WeaponType.Spear)   set = code.WeaponSet2;
		else {
			Console.WriteLine("This character no longer holds a land spear.");
			return;
		}

		Assert.Equal(SkillId.Abyssal_Strike, await ResolveWeaponSkill(code, set, 0));
		Assert.Equal(SkillId.Abyssal_Force , await ResolveWeaponSkill(code, set, 1));
		Assert.Equal(SkillId.Abyssal_Blitz , await ResolveWeaponSkill(code, set, 2));
		Assert.Equal(SkillId.Abyssal_Blot  , await ResolveWeaponSkill(code, set, 3));
		Assert.Equal(SkillId.Abyssal_Raze  , await ResolveWeaponSkill(code, set, 4));
	}


	[PlatformFact(PlatformID.Win32NT)]
	public async Task FromCurrentCharacterNullIfUnavailable()
	{
		var code = await APILoader.LoadBuildCodeFromCurrentCharacter(FunctionTests.VALID_KEY);
		Assert.Null(code);
	}
}

public class PlatformFactAttribute : FactAttribute {
	public PlatformFactAttribute(PlatformID platform)
	{
		var currentPlatform = Environment.OSVersion.Platform;
		if(currentPlatform != platform)
			Skip = $"Skipped because current platform {currentPlatform} does not equal {platform}";
	}
}

