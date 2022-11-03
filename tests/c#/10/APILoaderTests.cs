using Gw2Sharp.WebApi.V2.Models;
using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.API;

public class FunctionTests {
	[Fact]
	public void ShouldThrowNotAToken()
	{
		Assert.Throws<AggregateException>(() => {
			var code = APILoader.LoadBuildCode("xxx", "sss", default);
		});
	}
	
	[Fact]
	public void ShouldThrowInvalidScopes()
	{
		Assert.Throws<AggregateException>(() => {
			var code = APILoader.LoadBuildCode("AD041D99-AEEF-2E45-8732-0057285EFE370740BF1D-6427-4191-8C4F-84DD1C97F05F", "sss", default);
		});
	}

	[Fact]
	public void ShouldFindMissinScopes()
	{
		var connection = new Gw2Sharp.Connection("AD041D99-AEEF-2E45-8732-0057285EFE370740BF1D-6427-4191-8C4F-84DD1C97F05F");
		using var client = new Gw2Sharp.Gw2Client(connection);

		var missingScopes = APILoader.ValidateScopes(client).Result;

		Assert.Equal(new[] {
			TokenPermission.Characters, TokenPermission.Builds,
		}, missingScopes);
	}

	[Fact]
	public void ShouldThrowNoSuchCharacter()
	{
		Assert.Throws<AggregateException>(() => {
			var code = APILoader.LoadBuildCode("92CE5A6C-E594-9D4D-B92B-5621ACFE047D436C02BD-0810-47D9-B9D4-2620EB7DD598", "does not exist", default);
		});
	}
}

public class BasicCodesTests {
	[Fact]
	public void LoadBuild()
	{
		var code = APILoader.LoadBuildCode("92CE5A6C-E594-9D4D-B92B-5621ACFE047D436C02BD-0810-47D9-B9D4-2620EB7DD598", "Hardstuck Thief", default);
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

		Assert.Equal(WeaponType.Scepter, code.Weapons.Set1.MainHand);
		Assert.Equal(WeaponType.Dagger , code.Weapons.Set1.OffHand);
		Assert.Null(code.Weapons.Set2.MainHand);
		Assert.Equal(WeaponType.Pistol , code.Weapons.Set2.OffHand);

		Assert.Equal(91388 /*deamons*/      , code.Weapons.Set1.Sigil1);
		Assert.Equal(91473 /*concentration*/, code.Weapons.Set1.Sigil2);
		Assert.Null(code.Weapons.Set2.Sigil1);
		Assert.Equal(91398 /*paralysation*/ , code.Weapons.Set2.Sigil2);

		var celestialStatsKEKW = new StatId[]{ StatId.Celestial1, StatId.Celestial2, StatId.Celestial3, StatId.Celestial4 };
		for(var i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++)
			if(i != 13) // empty second main hand
				Assert.Contains(code.EquipmentAttributes[i]!.Value, celestialStatsKEKW);

		Assert.Equal(SkillId.Well_of_Gloom  , code.SlotSkills.Heal);
		Assert.Equal(SkillId.Well_of_Silence, code.SlotSkills.Utility1);
		Assert.Equal(SkillId.Well_of_Bounty , code.SlotSkills.Utility2);
		Assert.Equal(SkillId.Well_of_Sorrow , code.SlotSkills.Utility3);
		Assert.Equal(SkillId.Shadowfall     , code.SlotSkills.Elite);

		Assert.Equal(91485 /*traveler*/, code.Rune);
	}
}
