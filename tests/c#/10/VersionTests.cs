using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.Version;

public class VersionTests {
	public static IEnumerable<object[]> DataProvider() {
		var invalidCodes = new[] {
			TestUtilities.CodesInvalid["wrong-version"],
		};
		
		var v1Codes = TestUtilities.CodesV1.Values;
		
		var v2Codes = TestUtilities.CodesV2.Values;

		return invalidCodes.Select(c => new object[] { c, -1 })
			.Concat(v1Codes.Select(c => new object[] { c,  1 }))
			.Concat(v2Codes.Select(c => {
				var version = TextLoader.INVERSE_CHARSET[c[0]];
				if(version >= 26) version -= 26;
				return new object[] { c, version };
			}));
	}

	[Theory] [MemberData(nameof(DataProvider))]
	public void VersionDetectionTest(string code, int expectedVersion) {
		Assert.Equal(expectedVersion, Static.DetermineCodeVersion(code));
	}
}