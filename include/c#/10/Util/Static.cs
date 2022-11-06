namespace Hardstuck.GuildWars2.BuildCodes.V2.Util;

internal static class Static {
	public static ReadOnlySpan<T> SliceAndAdvance<T>(int index, ref ReadOnlySpan<T> input) {
		var ret = input[..index];
		input = input[index..];
		return ret;
	}

	public static T SliceAndAdvance<T>(ref ReadOnlySpan<T> input) {
		var ret = input[0];
		input = input[1..];
		return ret;
	}

	public static ReadOnlySpan<T> SliceAndAdvancePlus1<T>(int index, ref ReadOnlySpan<T> input)
	{
		var ret = input[..index];
		input = input[(index + 1)..];
		return ret;
	}
}
