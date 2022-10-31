using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests;

public class LoaderBasicTests {
	[Fact]
	public void ShouldThrowVersion()
	{
		Assert.ThrowsAny<Exception>(() => {
			var code = TextLoader.LoadBuildCode("Xaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
		});
	}

	[Fact]
	public void ShouldThrowTooShort()
	{
		Assert.ThrowsAny<Exception>(() => {
			var code = TextLoader.LoadBuildCode("Btoo-short");
		});
	}

	[Fact]
	public void ShouldThrowInvalidCharacters()
	{
		Assert.ThrowsAny<Exception>(() => {
			var code = TextLoader.LoadBuildCode("B���������������������������������������������������������������������");
		});
	}

	[Fact]
	public void MinimalPvP()
	{
		var code = TextLoader.LoadBuildCode("BpA___~S~S~S~S~S~_B~N");
	}

	[Fact]
	public void MinimalPvE()
	{
		var code = TextLoader.LoadBuildCode("BoA___~S~S~S~S~S~_A~N~__");
	}

	[Fact]
	public void MinimalRanger()
	{
		var code = TextLoader.LoadBuildCode("BoD___~S~S~S~S~S~_A~N~__~~");
	}

	[Fact]
	public void MinimalRevenant()
	{
		var code = TextLoader.LoadBuildCode("BoI___~S~S~S~S~S~_A~N~____");
	}
}