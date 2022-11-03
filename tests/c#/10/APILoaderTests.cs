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
			var code = APILoader.LoadBuildCode("F7B821CF-B7FF-8F4C-AA32-424DAE4E799578A913A7-A6A9-4592-A1F6-1524163DE4DA", "sss", default);
		});
	}

	[Fact]
	public void ShouldFindMissinScopes()
	{
		var connection = new Gw2Sharp.Connection("F7B821CF-B7FF-8F4C-AA32-424DAE4E799578A913A7-A6A9-4592-A1F6-1524163DE4DA");
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
			var code = APILoader.LoadBuildCode("D95CE863-D1B6-284F-B347-4B66C993759EDD490996-37AE-4E71-839A-DA51A0B6D40B", "does not exist", default);
		});
	}
}

public class BasicCodesTests {
	[Fact]
	public void LoadBuild()
	{
		var code = APILoader.LoadBuildCode("D95CE863-D1B6-284F-B347-4B66C993759EDD490996-37AE-4E71-839A-DA51A0B6D40B", "Ivy Rennorb", default);
		Assert.Equal(Profession.Thief, code.Profession);
	}
}
