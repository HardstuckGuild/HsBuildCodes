namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests;

static class TestUtilities {
	public static Dictionary<string, string> CodesInvalid  = new();
	public static Dictionary<string, string> CodesV1       = new();
	public static Dictionary<string, string> CodesV2       = new();
	public static Dictionary<string, string> CodesIngame   = new();
	public static Dictionary<string, string> CodesV2Binary = new();

	static TestUtilities() {
		Dictionary<string, string> dict = null!;
		string currentKey = null!;
		string currentAccumulator = string.Empty;
		foreach(var line_ in File.ReadLines("codes.ini"))
		{
			var comment = line_.IndexOf(';');
			var line = (comment > -1 ? line_[..comment] : line_).Trim();
			if(string.IsNullOrWhiteSpace(line)) continue;

			if(line.StartsWith('[') && line.EndsWith(']'))
			{
				dict = line[1..^1] switch {
					"Invalid"  => CodesInvalid,
					"V1"       => CodesV1,
					"V2"       => CodesV2,
					"Ingame"   => CodesIngame,
					"V2Binary" => CodesV2Binary,
				};
			}
			else
			{
				if(dict != CodesV2Binary)
				{
					var split = line.IndexOf('=');
					dict[line[..split].Trim()] = line[(split + 1)..].Trim();
				}
				else if(line == "<end>")
				{
					dict[currentKey] = currentAccumulator;
					currentAccumulator = string.Empty;
				}
				else
				{
					var split = line.IndexOf('=');
					if(split != -1)
					{
						currentKey = line[..split].Trim();
					}
					else
					{
						currentAccumulator += line;
					}
				}
			}
		}
	}
}
