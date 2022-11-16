using System.Text.Json;
using System.Text;

namespace Hardstuck.GuildWars2.BuildCodes.OfficialAPI;
public class SnakeCaseNamingPolicy : JsonNamingPolicy {
	readonly StringBuilder sb = new(128);
	public override string ConvertName(string name)
	{
		sb.Clear();
		sb.Append(char.ToLower(name[0]));
		for(int i = 1; i < name.Length; i++)
		{
			var c = name[i];
			if(char.IsUpper(c))
				sb.Append('_').Append(char.ToLower(c));
			else
				sb.Append(c);
		}
		return sb.ToString();
	}
}
