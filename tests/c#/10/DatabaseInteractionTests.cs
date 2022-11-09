using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.Database;

public class InteractionTests
{
	[Fact]
	public async Task CanDownloadSkillPallettes() 
	{
		await PerProfessionData.Reload(Profession.Revenant);
		Assert.InRange(PerProfessionData.Revenant.PalletteToSkill.Count, 2, 999999);
	}

	[Fact]
	public async Task CanFindOfflinePallette()
	{
		await PerProfessionData.Reload(Profession.Necromancer, true);
		Assert.InRange(PerProfessionData.Necromancer.PalletteToSkill.Count, 2, 999999);
	}
}