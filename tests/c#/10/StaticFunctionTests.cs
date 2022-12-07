using Xunit;
using static Hardstuck.GuildWars2.BuildCodes.V2.Static;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.StaticFunctions;

public class CompressionTests
{
	[Fact]
	public void DoNothing() 
	{
		var text = TestUtilities.CodesV2["uncompressed1"];
		var code = TextLoader.LoadBuildCode(text);
		Compress(code, CompressionOptions.NONE);
		var text_compressed = TextLoader.WriteBuildCode(code);
		Assert.Equal(text, text_compressed);
	}

	[Fact]
	public void ReplaceNonStatInfusions()
	{
		var text = TestUtilities.CodesV2["uncompressed1"];
		var code = TextLoader.LoadBuildCode(text);
		Compress(code, CompressionOptions.REMOVE_NON_STAT_INFUSIONS);
		var text_compressed = TextLoader.WriteBuildCode(code);
		Assert.Equal(TestUtilities.CodesV2["compressed1-no-agony-inf"], text_compressed);
	}

	[Fact]
	public void RearangeInfusions()
	{
		var text = TestUtilities.CodesV2["uncompressed1"];
		var code = TextLoader.LoadBuildCode(text);
		Compress(code, CompressionOptions.REARRANGE_INFUSIONS);

		//NOTE(Rennorb): cant directly compare since the order could be different
		static Dictionary<ItemId, int> ExtractInfusions(BuildCode code)
		{
			var infusions = new Dictionary<ItemId, int>(ALL_INFUSION_COUNT);
			for(int i = 0; i < ALL_INFUSION_COUNT; i++)
			{
				if(!HasInfusionSlot(code, i)) continue;

				var item = code.Infusions[i];
				infusions[item] = infusions.TryGetValue(item, out var count) ? count + 1 : 1;
			}
			return infusions;
		}
		
		var code_reference = TextLoader.LoadBuildCode(TestUtilities.CodesV2["compressed1-rearange-inf"]);

		Assert.Equal(ExtractInfusions(code_reference), ExtractInfusions(code));
	}

	[Fact]
	public void SubstituteInfusions()
	{
		var text = TestUtilities.CodesV2["uncompressed1"];
		var code = TextLoader.LoadBuildCode(text);
		Compress(code, CompressionOptions.SUBSTITUTE_INFUSIONS);
		var text_compressed = TextLoader.WriteBuildCode(code);
		Assert.Equal(TestUtilities.CodesV2["compressed1-subst-inf"], text_compressed);
	}

	[Fact]
	public void All()
	{
		var text = TestUtilities.CodesV2["uncompressed1"];
		var code = TextLoader.LoadBuildCode(text);
		Compress(code, CompressionOptions.ALL);
		var text_compressed = TextLoader.WriteBuildCode(code);
		Assert.Equal(TestUtilities.CodesV2["compressed1-all"], text_compressed);
	}
}
