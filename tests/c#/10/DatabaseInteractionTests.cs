using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.Database;

public class InteractionTests
{
	[Fact]
	public async Task CanDownloadSkillPallettes() 
	{
		await PerProfessionData.Reload(Profession.Revenant);
	}

	[Fact]
	public async Task CanFindOfflinePallette()
	{
		await PerProfessionData.Reload(Profession.Revenant, false);
	}
}