using Hardstuck.GuildWars2.BuildCodes.OfficialAPI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

static class API {
	public const string BASE_PATH = "https://api.guildwars2.com/v2";

	internal const string LATEST_SCHEMA = "latest";
	static readonly HttpClient _client = new();
	static readonly JsonSerializerOptions _jsonConfig = new() {
		PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
		IncludeFields        = true,
		Converters           = {
			new JsonStringEnumConverter(allowIntegerValues: true),
		},
	};

	/// <inheritdoc cref="Request(string, string?, string)"/>
	public static async Task<T> RequestJson<T>(string path, string? apiToken = null, string schemaVersion = LATEST_SCHEMA)
		=> JsonDocument.Parse(await Request(path, apiToken, schemaVersion)).Deserialize<T>(_jsonConfig)!;

	/// <exception cref="HttpRequestException">If there where anya problems</exception>
	public static async Task<string> Request(string path, string? apiToken = null, string schemaVersion = LATEST_SCHEMA)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, BASE_PATH + path);
		request.Headers.Add("X-Schema-Version", schemaVersion);
		if(apiToken != null) request.Headers.Add("Authorization", $"Bearer {apiToken}");

		var response = await _client.SendAsync(request);

		string body;
		switch(response.StatusCode)
		{
			case System.Net.HttpStatusCode.Unauthorized:
				body = await response.Content.ReadAsStringAsync();
				if(body.Contains("Invalid access token")) 
					throw new InvalidAccessTokenException(JsonDocument.Parse(body).RootElement.GetProperty("text").GetString()!);
				break;

			case System.Net.HttpStatusCode.Forbidden:
				body = await response.Content.ReadAsStringAsync();
				if(body.Contains("requires scope")) 
					throw new MissingScopesException(JsonDocument.Parse(body).RootElement.GetProperty("text").GetString()!);
				break;

			case System.Net.HttpStatusCode.NotFound:
				body = await response.Content.ReadAsStringAsync();
				if(body.Contains("no such id")) 
					throw new NotFoundException(JsonDocument.Parse(body).RootElement.GetProperty("text").GetString()!);
				break;
		} 
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}
}

public class InvalidAccessTokenException : UnauthorizedAccessException {
	public InvalidAccessTokenException(string message) : base(message) {}
}
public class MissingScopesException : UnauthorizedAccessException {
	public MissingScopesException(string message) : base(message) { }
}
public class NotFoundException : KeyNotFoundException {
	public NotFoundException(string message) : base(message) { }
}
