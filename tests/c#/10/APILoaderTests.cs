using Hardstuck.GuildWars2.BuildCodes.V2.OfficialAPI;
using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.API;

public class FunctionTests {
	public const string VALID_KEY = "92CE5A6C-E594-9D4D-B92B-5621ACFE047D436C02BD-0810-47D9-B9D4-2620EB7DD598";
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
	public async Task ShouldFindMissinScopes()
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
	[Fact]
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

		Assert.Equal(ItemId.Superior_Sigil_of_Deamons2, code.WeaponSet1.Sigil1);
		Assert.Equal(ItemId.Superior_Sigil_of_Concentration2, code.WeaponSet1.Sigil2);
		Assert.Equal(ItemId._UNDEFINED, code.WeaponSet2.Sigil1);
		Assert.Equal(ItemId.Superior_Sigil_of_Paralysation2, code.WeaponSet2.Sigil2);

		var celestialStatsKEKW = new StatId[]{ StatId.Celestial1, StatId.Celestial2, StatId.Celestial3, StatId.Celestial4 };
		for(var i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++)
			if(i != 13) // empty second main hand
				Assert.Contains(code.EquipmentAttributes[i], celestialStatsKEKW);

		Assert.Equal(SkillId.Well_of_Gloom  , code.SlotSkills.Heal);
		Assert.Equal(SkillId.Well_of_Silence, code.SlotSkills.Utility1);
		Assert.Equal(SkillId.Well_of_Bounty , code.SlotSkills.Utility2);
		Assert.Equal(SkillId.Well_of_Sorrow , code.SlotSkills.Utility3);
		Assert.Equal(SkillId.Shadowfall     , code.SlotSkills.Elite);

		Assert.Equal(ItemId.Superior_Rune_of_the_Traveler2, code.Rune);
	}

	[Fact]
	public async Task FromCurrentCharacterNullIfUnavailable()
	{
		var code = await APILoader.LoadBuildCodeFromCurrentCharacter(FunctionTests.VALID_KEY);
		Assert.Null(code);
	}
}
