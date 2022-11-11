<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

class API {
	use Util\_Static;

	public const BASE_PATH = "https://api.guildwars2.com/v2";

	public static function RequestJson(string $path, ?string $apiToken = null, string $schemaVersion = 'latest') : mixed
	{ return json_decode(API::Request($path, $apiToken, $schemaVersion)); }

	public static function Request(string $path, ?string $apiToken = null, string $schemaVersion = 'latest') : string|false
	{
		$headers = ["X-Schema-Version: $schemaVersion"];
		if($apiToken) $headers[] = "Authorization: Bearer $apiToken";

		$context = stream_context_create(['http' => [
			'method'  => 'GET',
			'header'  => $headers,
		]]);

		return file_get_contents(API::BASE_PATH . $path, context: $context);
	}
}