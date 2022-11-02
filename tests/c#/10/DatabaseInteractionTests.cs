using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.Database;

public class InteractionTests
{
	[Fact]
	public void CanDownloadSkillPallettes() 
	{
		ProfessionSkillPallettes.Reload(Profession.Revenant);
	}

	[Fact]
	public void CanFindOfflinePallette()
	{
		ProfessionSkillPallettes.Reload(Profession.Revenant, false);
	}
}